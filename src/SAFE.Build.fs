namespace SAFE.Build

open System

open Fake.Core
open Fake.DotNet
open Fake.IO

open Cit.Helpers.Arm.Parameters

[<RequireQualifiedAccess>]
module SAFE =
    [<AutoOpen>]
    module private Internal =
        let serverPath = Path.getFullName "./src/Server"
        let clientPath = Path.getFullName "./src/Client"
        let deployPath = Path.getFullName "./deploy"

        let platformTool tool winTool =
            let tool = if Environment.isUnix then tool else winTool
            match Process.tryFindFileOnPath tool with Some t -> t | _ -> failwithf "%s not found" tool

        let nodeTool = platformTool "node" "node.exe"
        let yarnTool = platformTool "yarn" "yarn.cmd"

        let install = lazy DotNet.install DotNet.Release_2_1_300

        let inline withWorkDir wd =
            DotNet.Options.lift install.Value
            >> DotNet.Options.withWorkingDirectory wd

        let runTool cmd args workingDir =
            let result =
                Process.execSimple (fun info ->
                    { info with
                        FileName = cmd
                        WorkingDirectory = workingDir
                        Arguments = args })
                    TimeSpan.MaxValue
            if result <> 0 then failwithf "'%s %s' failed" cmd args

        let runDotNet cmd workingDir =
            let result =
                DotNet.exec (withWorkDir workingDir) cmd ""
            if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

        let openBrowser url =
            let result =
                //https://github.com/dotnet/corefx/issues/10361
                Process.execSimple (fun info ->
                    { info with
                        FileName = url
                        UseShellExecute = true })
                    TimeSpan.MaxValue
            if result <> 0 then failwithf "opening browser failed"

    let cleanBuildDirs () =
        Shell.cleanDirs [ deployPath ]

    let restoreClient () =
        printfn "Node version:"
        runTool nodeTool "--version" __SOURCE_DIRECTORY__
        printfn "Yarn version:"
        runTool yarnTool "--version" __SOURCE_DIRECTORY__
        runTool yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
        runDotNet "restore" clientPath

    let restoreServer () =
        runDotNet "restore" serverPath

    let buildClient () =
        runDotNet "fable webpack -- -p" clientPath

    let buildServer () =
        runDotNet "build" serverPath

    let runServer = async {
        runDotNet "watch run" serverPath
    }

    let runClient = async {
        runDotNet "fable webpack-dev-server" clientPath
    }

    let runBrowser = async {
        Threading.Thread.Sleep 5000
        openBrowser "http://localhost:8080"
    }

    let runInParallel tasks =
        tasks
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore

    module Azure =
        open Cit.Helpers.Arm
        open Cit.Helpers.Arm.Parameters
        open Microsoft.Azure.Management.ResourceManager.Fluent.Core

        type ArmOutput =
            { WebAppName : ParameterValue<string>
              WebAppPassword : ParameterValue<string> }

        let mutable private deploymentOutputs : ArmOutput option = None

        let bundle () =
            runDotNet (sprintf "publish %s -c release -o %s" serverPath deployPath) __SOURCE_DIRECTORY__
            Shell.copyDir (Path.combine deployPath "public") (Path.combine clientPath "public") FileFilter.allFiles

        let provisionARM () =
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

        open Fake.IO.Globbing.Operators

        let deploy () =
            let zipFile = "deploy.zip"
            IO.File.Delete zipFile
            Zip.zip deployPath zipFile !!(deployPath + @"\**\**")

            let appName = deploymentOutputs.Value.WebAppName.value
            let appPassword = deploymentOutputs.Value.WebAppPassword.value

            let destinationUri = sprintf "https://%s.scm.azurewebsites.net/api/zipdeploy" appName
            let client = new Net.WebClient(Credentials = Net.NetworkCredential("$" + appName, appPassword))
            Trace.tracefn "Uploading %s to %s" zipFile destinationUri
            client.UploadData(destinationUri, IO.File.ReadAllBytes zipFile) |> ignore