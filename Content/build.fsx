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

Target.initEnvironment ()

let serverPath = Path.getFullName "./src/Server"
let clientPath = Path.getFullName "./src/Client"
let clientDeployPath = Path.combine clientPath "deploy"
let deployDir = Path.getFullName "./deploy"

let release = ReleaseNotes.load "RELEASE_NOTES.md"

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
//#if (deploy == "gcp-kubernetes" || deploy == "gcp-appengine")
let gcloudTool () = platformTool "gcloud" "gcloud.cmd"
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

//#if (deploy == "gcp-kubernetes" || deploy == "heroku")
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
//#endif
//#if (deploy == "gcp-kubernetes")
let getGcloudProject() =
    runToolWithOutput (gcloudTool ()) "config get-value project -q" __SOURCE_DIRECTORY__

let getDockerTag() = "v1"

let createDockerImageName projectName =
    let dockerTag = getDockerTag()
    let projectId = getGcloudProject()
    sprintf "gcr.io/%s/%s:%s" projectId projectName dockerTag

let deployExists appName =
    let result = runToolWithOutput "kubectl" "get deploy" __SOURCE_DIRECTORY__
    let pattern = "^" + appName + "\s+"
    Regex.IsMatch(result, pattern, RegexOptions.Multiline)

let updateKubernetesDeploy appName dockerTag =
    let updateArgs = sprintf "set image deployment/%s %s=%s" appName appName dockerTag
    runTool "kubectl" updateArgs __SOURCE_DIRECTORY__

let createAndExposeKubernetesDeploy appName dockerTag port =
    let deployArgs = sprintf "run %s --image=%s --port %i" appName dockerTag port
    runTool "kubectl" deployArgs __SOURCE_DIRECTORY__

    let exposeArgs = sprintf "expose deployment %s --type=LoadBalancer --port 80 --target-port %i" appName port
    runTool "kubectl" exposeArgs __SOURCE_DIRECTORY__
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
)

Target.create "Build" (fun _ ->
    runDotNet "build" serverPath
    Shell.regexReplaceInFileWithEncoding
        "let app = \".+\""
       ("let app = \"" + release.NugetVersion + "\"")
        System.Text.Encoding.UTF8
        (Path.combine clientPath "Version.fs")
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
    runTool "docker" args __SOURCE_DIRECTORY__

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
    runTool "docker" pushArgs __SOURCE_DIRECTORY__
)

Target.create "ClusterAuth" (fun _ ->
    let clusterName = Environment.environVarOrDefault "SAFE_CLUSTER" "safe-cluster"
    let authArgs = sprintf "container clusters get-credentials %s" clusterName
    runTool (gcloudTool ()) authArgs __SOURCE_DIRECTORY__
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
        let clientId = try Environment.environVar "clientId" |> Guid.Parse with _ -> failwith "Invalid Client ID. This should be the Client ID of an application registered in Azure with permission to create resources in your subscription."
        let tenantId =
            try Environment.environVarOrNone "tenantId" |> Option.map Guid.Parse
            with _ -> failwith "Invalid TenantId ID. This should be the Tenant ID of an application registered in Azure with permission to create resources in your subscription."

        Trace.tracefn "Deploying template '%s' to resource group '%s' in subscription '%O'..." armTemplate resourceGroupName subscriptionId
        subscriptionId
        |> authenticateDevice Trace.trace { ClientId = clientId; TenantId = tenantId }
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
    runTool (gcloudTool ()) "app deploy --quiet" __SOURCE_DIRECTORY__
)
//#endif

//#if (deploy == "iis")
Target.create "Bundle" (fun _ ->
    let serverDir = Path.combine deployDir "Server"
    let clientDir = Path.combine deployDir "Client"
    let publicDir = Path.combine clientDir "public"
    let publishArgs = sprintf "publish -c Release -o \"%s\"" serverDir
    runDotNet publishArgs serverPath

//#if (server == "suave")
    // read and transform web.config, removing aspNetCore generated info
    let config = Path.combine serverDir "web.config"
    let mutable xmlDoc = new System.Xml.XmlDocument()
    xmlDoc.LoadXml(File.readAsString config)
    for node in xmlDoc.SelectNodes("/configuration/system.webServer/aspNetCore") do
        ignore (node.ParentNode.RemoveChild(node))
    for node in xmlDoc.SelectNodes("/configuration/system.webServer/handlers/add[@name='aspNetCore']") do
        ignore (node.ParentNode.RemoveChild(node))
    xmlDoc.Save(config)
//#endif

    Shell.copyDir publicDir clientDeployPath FileFilter.allFiles
)

//#endif

//#if (deploy == "heroku")
Target.create "Configure" (fun args ->
    let gitTool = platformTool "git" "git.exe"
    let herokuTool = platformTool "heroku" "heroku.cmd"
    let arguments =  ("apps:create"::args.Context.Arguments) |> String.concat " "
    let output = runToolWithOutput herokuTool arguments __SOURCE_DIRECTORY__
    let app = (output.Split '|').[0]
    printfn "app created in %s" (app.Trim())
    let appName = app.[8..(app.IndexOf(".")-1)]
    runTool gitTool "init" __SOURCE_DIRECTORY__
    let gitCmd = sprintf "git:remote --app %s" appName
    runTool herokuTool gitCmd __SOURCE_DIRECTORY__
    runTool herokuTool "buildpacks:set -i 1 https://github.com/heroku/heroku-buildpack-nodejs" __SOURCE_DIRECTORY__
    runTool herokuTool "buildpacks:set -i 2 https://github.com/SAFE-Stack/SAFE-buildpack" __SOURCE_DIRECTORY__
    runTool gitTool "add ." __SOURCE_DIRECTORY__
    runTool gitTool "commit -m initial" __SOURCE_DIRECTORY__
)

Target.create "Bundle" (fun _ ->
    let serverDir = Path.combine deployDir "Server"
    let clientDir = Path.combine deployDir "Client"
    let publicDir = Path.combine clientDir "public"

    let publishArgs = sprintf "publish -c Release -o \"%s\" --runtime linux-x64" serverDir
    runDotNet publishArgs serverPath

    Shell.copyDir publicDir clientDeployPath FileFilter.allFiles

    let procFile = "web: cd ./deploy/Server/ && ./Server"
    File.writeNew "Procfile" [procFile]
)

Target.create "Deploy" (fun _ ->
    let gitTool = platformTool "git" "git.exe"
    runTool gitTool "push heroku master" __SOURCE_DIRECTORY__
    let herokuTool = platformTool "heroku" "heroku.cmd"
    runTool herokuTool "open" __SOURCE_DIRECTORY__
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
//#elseif (deploy == "iis" || deploy == "heroku")
    ==> "Bundle"
//#endif

//#if (deploy == "gcp-appengine")
"Bundle"
    ==> "Deploy"
//#endif

"Clean"
    ==> "InstallClient"
    ==> "Run"

//#if (deploy == "heroku")
Target.runOrDefaultWithArguments "Bundle"
//#else
Target.runOrDefaultWithArguments "Build"
//#endif
