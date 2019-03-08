#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

-:cnd:noEmit
#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif
+:cnd:noEmit

open System

open Fake.Core
open Fake.DotNet
open Fake.IO
//#if (deploy == "azure")
#load @"paket-files/build/CompositionalIT/fshelpers/src/FsHelpers/ArmHelper/ArmHelper.fs"
open Cit.Helpers.Arm
open Cit.Helpers.Arm.Parameters
open Microsoft.Azure.Management.ResourceManager.Fluent.Core
//#endif
//#if (deploy == "gcp-kubernetes")
open System.Text.RegularExpressions
//#endif

let serverPath = Path.getFullName "./src/Server"
let clientPath = Path.getFullName "./src/Client"
let clientDeployPath = Path.combine clientPath "deploy"
let deployDir = Path.getFullName "./deploy"

let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool
    match ProcessUtils.tryFindFileOnPath tool with
    | Some t -> t
    | _ ->
        let errorMsg =
            tool + " was not found in path. " +
            "Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
        failwith errorMsg

let nodeTool = platformTool "node" "node.exe"
//#if (js-deps == "npm")
let npmTool = platformTool "npm" "npm.cmd"
let npxTool = platformTool "npx" "npx.cmd"
//#else
let yarnTool = platformTool "yarn" "yarn.cmd"
//#endif

let runTool cmd args workingDir =
    let arguments = args |> String.split ' ' |> Arguments.OfArgs
    Command.RawCommand (cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let runDotNet cmd workingDir =
    let result =
        DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

let openBrowser url =
    //https://github.com/dotnet/corefx/issues/10361
    Command.ShellCommand url
    |> CreateProcess.fromCommand
    |> CreateProcess.ensureExitCodeWithMessage "opening browser failed"
    |> Proc.run
    |> ignore

//#if (deploy == "gcp-kubernetes")
let runToolWithOutput cmd args workingDir =
    let arguments = args |> String.split ' ' |> Arguments.OfArgs
    let result =
        Command.RawCommand (cmd, arguments)
        |> CreateProcess.fromCommand
        |> CreateProcess.withWorkingDirectory workingDir
        |> CreateProcess.ensureExitCode
        |> CreateProcess.redirectOutput
        |> Proc.run
    result.Result.Output |> (fun s -> s.TrimEnd())

let getGcloudProject() =
    runToolWithOutput "gcloud" "config get-value project -q" "."

let getDockerTag() = "v1"

let createDockerImageName projectName =
    let dockerTag = getDockerTag()
    let projectId = getGcloudProject()
    sprintf "gcr.io/%s/%s:%s" projectId projectName dockerTag

let deployExists appName =
    let result = runToolWithOutput "kubectl" "get deploy" "."
    let pattern = "^" + appName + "\s+"
    Regex.IsMatch(result, pattern, RegexOptions.Multiline)

let updateKubernetesDeploy appName dockerTag =
    let updateArgs = sprintf "set image deployment/%s %s=%s" appName appName dockerTag
    runTool "kubectl" updateArgs "."

let createAndExposeKubernetesDeploy appName dockerTag port =
    let deployArgs = sprintf "run %s --image=%s --port %i" appName dockerTag port
    runTool "kubectl" deployArgs "."

    let exposeArgs = sprintf "expose deployment %s --type=LoadBalancer --port 80 --target-port %i" appName port
    runTool "kubectl" exposeArgs "."
//#endif

Target.create "Clean" (fun _ ->
    [ deployDir
      clientDeployPath ]
    |> Shell.cleanDirs
)

Target.create "InstallClient" (fun _ ->
    printfn "Node version:"
    runTool nodeTool "--version" __SOURCE_DIRECTORY__
//#if (js-deps == "npm")
    printfn "Npm version:"
    runTool npmTool "--version"  __SOURCE_DIRECTORY__
    runTool npmTool "install" __SOURCE_DIRECTORY__
//#else
    printfn "Yarn version:"
    runTool yarnTool "--version" __SOURCE_DIRECTORY__
    runTool yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
//#endif
    runDotNet "restore" clientPath
)

Target.create "Build" (fun _ ->
    runDotNet "build" serverPath
//#if (js-deps == "npm")
    runTool npxTool "webpack-cli -p" __SOURCE_DIRECTORY__
//#else
    runTool yarnTool "webpack-cli -p" __SOURCE_DIRECTORY__
//#endif
)

Target.create "Run" (fun _ ->
    let server = async {
        runDotNet "watch run" serverPath
    }
    let client = async {
//#if (js-deps == "npm")
        runTool npxTool "webpack-dev-server" __SOURCE_DIRECTORY__
//#else
        runTool yarnTool "webpack-dev-server" __SOURCE_DIRECTORY__
//#endif
    }
    let browser = async {
        do! Async.Sleep 5000
        openBrowser "http://localhost:8080"
    }

    let vsCodeSession = Environment.hasEnvironVar "vsCodeSession"
    let safeClientOnly = Environment.hasEnvironVar "safeClientOnly"

    let tasks =
        [ if not safeClientOnly then yield server
          yield client
          if not vsCodeSession then yield browser ]

    tasks
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

//#if (deploy == "docker" || deploy == "gcp-kubernetes" || deploy == "gcp-appengine")
let buildDocker tag =
    let args = sprintf "build -t %s ." tag
    runTool "docker" args "."

Target.create "Bundle" (fun _ ->
    let serverDir = Path.combine deployDir "Server"
    let clientDir = Path.combine deployDir "Client"
    let publicDir = Path.combine clientDir "public"

    let publishArgs = sprintf "publish -c Release -o \"%s\"" serverDir
    runDotNet publishArgs serverPath

    Shell.copyDir publicDir clientDeployPath FileFilter.allFiles
)

let dockerUser = "safe-template"
let dockerImageName = "safe-template"
//#endif
//#if (deploy == "docker" || deploy == "gcp-appengine")
let dockerFullName = sprintf "%s/%s" dockerUser dockerImageName

Target.create "Docker" (fun _ ->
    buildDocker dockerFullName
)

//#endif
//#if (deploy == "gcp-kubernetes")
Target.create "Docker" (fun _ ->
    let imageName = createDockerImageName dockerImageName
    buildDocker imageName
)

Target.create "Publish" (fun _ ->
    let imageName = createDockerImageName dockerImageName
    let pushArgs = sprintf "push %s" imageName
    runTool "docker" pushArgs "."
)

Target.create "ClusterAuth" (fun _ ->
    let clusterName = Environment.environVarOrDefault "SAFE_CLUSTER" "safe-cluster"
    let authArgs = sprintf "container clusters get-credentials %s" clusterName
    runTool "gcloud" authArgs "."
)

Target.create "Deploy" (fun _ ->
    let imageName = createDockerImageName dockerImageName
    let appName = dockerImageName
    let port = 8085
    if deployExists appName
    then
        updateKubernetesDeploy appName imageName
    else
        createAndExposeKubernetesDeploy appName imageName port
)
//#endif


//#if (deploy == "azure")
Target.create "Bundle" (fun _ ->
    let serverDir = deployDir
    let publicDir = Path.combine deployDir "public"

    let publishArgs = sprintf "publish -c Release -o \"%s\"" serverDir
    runDotNet publishArgs serverPath

    Shell.copyDir publicDir clientDeployPath FileFilter.allFiles
)

type ArmOutput =
    { WebAppName : ParameterValue<string>
      WebAppPassword : ParameterValue<string> }
let mutable deploymentOutputs : ArmOutput option = None

Target.create "ArmTemplate" (fun _ ->
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
)

open Fake.IO.Globbing.Operators
open System.Net

// https://github.com/SAFE-Stack/SAFE-template/issues/120
// https://stackoverflow.com/a/6994391/3232646
type TimeoutWebClient() =
    inherit WebClient()
    override this.GetWebRequest uri =
        let request = base.GetWebRequest uri
        request.Timeout <- 30 * 60 * 1000
        request

Target.create "AppService" (fun _ ->
    let zipFile = "deploy.zip"
    IO.File.Delete zipFile
    Zip.zip deployDir zipFile !!(deployDir + @"\**\**")

    let appName = deploymentOutputs.Value.WebAppName.value
    let appPassword = deploymentOutputs.Value.WebAppPassword.value

    let destinationUri = sprintf "https://%s.scm.azurewebsites.net/api/zipdeploy" appName
    let client = new TimeoutWebClient(Credentials = NetworkCredential("$" + appName, appPassword))
    Trace.tracefn "Uploading %s to %s" zipFile destinationUri
    client.UploadData(destinationUri, IO.File.ReadAllBytes zipFile) |> ignore)
//#endif

//#if (deploy == "gcp-appengine")
Target.create "Deploy" (fun _ ->
    runTool "gcloud" "app deploy --quiet" "."
)
//#endif

open Fake.Core.TargetOperators

"Clean"
    ==> "InstallClient"
    ==> "Build"
//#if (deploy == "docker" || deploy == "gcp-appengine")
    ==> "Bundle"
    ==> "Docker"
//#elseif (deploy == "azure")
    ==> "Bundle"
    ==> "ArmTemplate"
    ==> "AppService"
//#elseif (deploy == "gcp-kubernetes")
    ==> "Bundle"
    ==> "Docker"
    ==> "Publish"
    ==> "ClusterAuth"
    ==> "Deploy"
//#endif

//#if (deploy == "gcp-appengine")
"Bundle"
    ==> "Deploy"
//#endif

"Clean"
    ==> "InstallClient"
    ==> "Run"

Target.runOrDefaultWithArguments "Build"
