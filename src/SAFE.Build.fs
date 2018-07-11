namespace SAFE.Build

open System

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators

open Microsoft.Azure.Management.ResourceManager.Fluent.Core

open Cit.Helpers.Arm
open Cit.Helpers.Arm.Parameters

type JsDeps =
    | NPM
    | Yarn

type DockerParams =
    { DockerUser : string
      DockerImageName : string }

type SAFEBuildParams =
    { ServerPath : string
      ClientPath : string
      DeployPath : string
      JsDeps : JsDeps
      Docker : DockerParams }

    static member Create () =
        { ServerPath = "./src/Server"
          ClientPath = "./src/Client"
          DeployPath = "./deploy"
          JsDeps = Yarn
          Docker =
            { DockerUser = "safe-template"
              DockerImageName = "safe-template" } }

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

module private Tool =

    let platformTool tool winTool =
        let tool = if Environment.isUnix then tool else winTool
        match Process.tryFindFileOnPath tool with
        | Some t -> t
        | _ ->
            let errorMsg =
                tool + " was not found in path. " +
                "Please install it and make sure it's available from your path. " +
                "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
            failwith errorMsg

    let runTool cmd args workingDir =
        let result =
            Process.execSimple (fun info ->
                { info with
                    FileName = cmd
                    WorkingDirectory = workingDir
                    Arguments = args })
                TimeSpan.MaxValue
        if result <> 0 then failwithf "'%s %s' failed" cmd args

type SAFEDockerIntegration (safeBuildParams : SAFEBuildParams) =

    member __.Bundle () =
        let serverDir = Path.combine safeBuildParams.DeployPath "Server"
        let clientDir = Path.combine safeBuildParams.DeployPath "Client"
        let publicDir = Path.combine clientDir "public"

        let publishArgs = sprintf "publish -c Release -o \"%s\"" serverDir
        SAFEDotnet.run publishArgs safeBuildParams.ServerPath

        Shell.copyDir publicDir "src/Client/public" FileFilter.allFiles

    member __.Build () =
        let dockerUser = safeBuildParams.Docker.DockerUser
        let dockerImageName = safeBuildParams.Docker.DockerImageName
        let dockerFullName = sprintf "%s/%s" dockerUser dockerImageName

        let buildArgs = sprintf "build -t %s ." dockerFullName
        Tool.runTool "docker" buildArgs "."

        let tagArgs = sprintf "tag %s %s" dockerFullName dockerFullName
        Tool.runTool "docker" tagArgs "."

type private ArmOutput =
    { WebAppName : ParameterValue<string>
      WebAppPassword : ParameterValue<string> }

open System.Net

// https://github.com/SAFE-Stack/SAFE-template/issues/120
// https://stackoverflow.com/a/6994391/3232646
type TimeoutWebClient() =
    inherit WebClient()
    override this.GetWebRequest uri =
        let request = base.GetWebRequest uri
        request.Timeout <- 30 * 60 * 1000
        request

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
        let client = new TimeoutWebClient(Credentials = NetworkCredential("$" + appName, appPassword))
        Trace.tracefn "Uploading %s to %s" zipFile destinationUri
        client.UploadData(destinationUri, IO.File.ReadAllBytes zipFile) |> ignore

type SAFEBuild (setParams : SetSAFEBuildParams) =
    let safeBuildParams =
        SAFEBuildParams.Create()
        |> setParams

    let serverPath = safeBuildParams.ServerPath
    let clientPath = safeBuildParams.ClientPath
    let deployPath = safeBuildParams.DeployPath


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
        let nodeTool = Tool.platformTool "node" "node.exe"
        printfn "Node version:"
        Tool.runTool nodeTool "--version" __SOURCE_DIRECTORY__

        match safeBuildParams.JsDeps with
        | Yarn ->
            let yarnTool = Tool.platformTool "yarn" "yarn.cmd"
            printfn "Yarn version:"
            Tool.runTool yarnTool "--version" __SOURCE_DIRECTORY__
            Tool.runTool yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
        | NPM ->
            let npmTool = Tool.platformTool "npm" "npm.cmd"
            printfn "Npm version:"
            Tool.runTool npmTool "--version"  __SOURCE_DIRECTORY__
            Tool.runTool npmTool "install" __SOURCE_DIRECTORY__

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

    member __.Docker = SAFEDockerIntegration safeBuildParams