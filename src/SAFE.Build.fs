namespace SAFE.Build

open System

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators

open Microsoft.Azure.Management.ResourceManager.Fluent.Core

open Cit.Helpers.Arm
open Cit.Helpers.Arm.Parameters

type SAFEBuildParams =
    { ServerPath : string
      ClientPath : string
      DeployPath : string }

    static member Create () =
        { ServerPath = "./src/Server"
          ClientPath = "./src/Client"
          DeployPath = "./deploy" }

type SetSAFEBuildParams = SAFEBuildParams -> SAFEBuildParams

module private SAFEDotnet =
    let install = lazy DotNet.install DotNet.Release_2_1_300

    let withWorkDir wd =
        DotNet.Options.lift install.Value
        >> DotNet.Options.withWorkingDirectory wd

    let run cmd workingDir =
        let result =
            DotNet.exec (withWorkDir workingDir) cmd ""
        if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

type private ArmOutput =
    { WebAppName : ParameterValue<string>
      WebAppPassword : ParameterValue<string> }

type SAFEAzureIntegration (safeBuildParams : SAFEBuildParams) =
    let mutable deploymentOutputs : ArmOutput option = None

    let serverPath = safeBuildParams.ServerPath
    let clientPath = safeBuildParams.ClientPath
    let deployPath = safeBuildParams.DeployPath

    member __.Bundle () =
        SAFEDotnet.run (sprintf "publish %s -c release -o %s" serverPath deployPath) __SOURCE_DIRECTORY__
        Shell.copyDir (Path.combine deployPath "public") (Path.combine clientPath "public") FileFilter.allFiles

    member __.ProvisionARM () =
        let environment = Environment.environVarOrDefault "environment" (Guid.NewGuid().ToString().ToLower().Split '-' |> Array.head)
        let armTemplate = @"arm-template.json"
        let resourceGroupName = "safe-" + environment

        let authCtx =
            // You can safely replace these with your own subscription and client IDs hard-coded into this script.
            let subscriptionId = try Environment.environVar "subscriptionId" |> Guid.Parse with _ -> failwith "Invalid Subscription ID. This should be your Azure Subscription ID."
            let clientId = try Environment.environVar "clientId" |> Guid.Parse with _ -> failwith "Invalid Client ID. This should be the Client ID of a Native application registered in Azure with permission to create resources in your subscription."

            Trace.tracefn "Deploying template '%s' to resource group '%s' in subscription '%O'..." armTemplate resourceGroupName subscriptionId
            subscriptionId
            |> authenticateDevice Trace.trace { ClientId = clientId; TenantId = None }
            |> Async.RunSynchronously

        let deployment =
            let location = Environment.environVarOrDefault "location" Region.EuropeWest.Name
            let pricingTier = Environment.environVarOrDefault "pricingTier" "F1"
            { DeploymentName = "SAFE-template-deploy"
              ResourceGroup = New(resourceGroupName, Region.Create location)
              ArmTemplate = IO.File.ReadAllText armTemplate
              Parameters =
                  Simple
                      [ "environment", ArmString environment
                        "location", ArmString location
                        "pricingTier", ArmString pricingTier ]
              DeploymentMode = Incremental }

        deployment
        |> deployWithProgress authCtx
        |> Seq.iter(function
            | DeploymentInProgress (state, operations) -> Trace.tracefn "State is %s, completed %d operations." state operations
            | DeploymentError (statusCode, message) -> Trace.traceError <| sprintf "DEPLOYMENT ERROR: %s - '%s'" statusCode message
            | DeploymentCompleted d -> deploymentOutputs <- d)

    member __.Deploy () =
        let zipFile = "deploy.zip"
        IO.File.Delete zipFile
        Zip.zip deployPath zipFile !!(deployPath + @"\**\**")

        let appName = deploymentOutputs.Value.WebAppName.value
        let appPassword = deploymentOutputs.Value.WebAppPassword.value

        let destinationUri = sprintf "https://%s.scm.azurewebsites.net/api/zipdeploy" appName
        let client = new Net.WebClient(Credentials = Net.NetworkCredential("$" + appName, appPassword))
        Trace.tracefn "Uploading %s to %s" zipFile destinationUri
        client.UploadData(destinationUri, IO.File.ReadAllBytes zipFile) |> ignore

type SAFEBuild (setParams : SetSAFEBuildParams) =
    let safeBuildParams =
        SAFEBuildParams.Create()
        |> setParams

    let serverPath = safeBuildParams.ServerPath
    let clientPath = safeBuildParams.ClientPath
    let deployPath = safeBuildParams.DeployPath

    let platformTool tool winTool =
        let tool = if Environment.isUnix then tool else winTool
        match Process.tryFindFileOnPath tool with Some t -> t | _ -> failwithf "%s not found" tool

    let nodeTool = platformTool "node" "node.exe"
    let yarnTool = platformTool "yarn" "yarn.cmd"

    let runTool cmd args workingDir =
        let result =
            Process.execSimple (fun info ->
                { info with
                    FileName = cmd
                    WorkingDirectory = workingDir
                    Arguments = args })
                TimeSpan.MaxValue
        if result <> 0 then failwithf "'%s %s' failed" cmd args

    let openBrowser url =
        let result =
            //https://github.com/dotnet/corefx/issues/10361
            Process.execSimple (fun info ->
                { info with
                    FileName = url
                    UseShellExecute = true })
                TimeSpan.MaxValue
        if result <> 0 then failwithf "opening browser failed"

    member __.CleanBuildDirs () =
        Shell.cleanDirs [ deployPath ]

    member __.RestoreClient () =
        printfn "Node version:"
        runTool nodeTool "--version" __SOURCE_DIRECTORY__
        printfn "Yarn version:"
        runTool yarnTool "--version" __SOURCE_DIRECTORY__
        runTool yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
        SAFEDotnet.run "restore" clientPath

    member __.RestoreServer () =
        SAFEDotnet.run "restore" serverPath

    member __.BuildClient () =
        SAFEDotnet.run "fable webpack -- -p" clientPath

    member __.BuildServer () =
        SAFEDotnet.run "build" serverPath

    member __.RunServer = async {
        SAFEDotnet.run "watch run" serverPath
    }

    member __.RunClient = async {
        SAFEDotnet.run "fable webpack-dev-server" clientPath
    }

    member __.RunBrowser = async {
        Threading.Thread.Sleep 5000
        openBrowser "http://localhost:8080"
    }

    member __.RunInParallel tasks =
        tasks
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore

    member __.Azure = SAFEAzureIntegration safeBuildParams
