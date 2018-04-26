#r @"packages/build/FAKE/tools/FakeLib.dll"
#if (Azure)
#r "netstandard"
#I "packages/build/Microsoft.Rest.ClientRuntime.Azure/lib/net452"
#load ".paket/load/netcoreapp2.1/Build/build.group.fsx"
#load @"paket-files\build\CompositionalIT\fshelpers\src\FsHelpers\ArmHelper\ArmHelper.fs"

open Cit.Helpers.Arm
open Cit.Helpers.Arm.Parameters
open Microsoft.Azure.Management.ResourceManager.Fluent.Core
#endif
open System
open Fake

let serverPath = "./src/Server" |> FullName
let clientPath = "./src/Client" |> FullName
let deployDir = "./deploy" |> FullName

let platformTool tool winTool =
  let tool = if isUnix then tool else winTool
  match tryFindFileOnPath tool with Some t -> t | _ -> failwithf "%s not found" tool

let nodeTool = platformTool "node" "node.exe"
#if (NPM)
let npmTool = platformTool "npm" "npm.cmd"
#else
let yarnTool = platformTool "yarn" "yarn.cmd"
#endif

let dotnetcliVersion = DotNetCli.GetDotNetSDKVersionFromGlobalJson()
let mutable dotnetCli = "dotnet"

let run cmd args workingDir =
  let result =
    ExecProcess (fun info ->
      info.FileName <- cmd
      info.WorkingDirectory <- workingDir
      info.Arguments <- args) TimeSpan.MaxValue
  if result <> 0 then failwithf "'%s %s' failed" cmd args

Target "Clean" (fun _ ->
  CleanDirs [deployDir]
)

Target "InstallDotNetCore" (fun _ ->
  dotnetCli <- DotNetCli.InstallDotNetSDK dotnetcliVersion
)

Target "InstallClient" (fun _ ->
  printfn "Node version:"
  run nodeTool "--version" __SOURCE_DIRECTORY__
#if (NPM)
  printfn "Npm version:"
  run npmTool "--version"  __SOURCE_DIRECTORY__
  run npmTool "install" __SOURCE_DIRECTORY__
#else
  printfn "Yarn version:"
  run yarnTool "--version" __SOURCE_DIRECTORY__
  run yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
#endif
  run dotnetCli "restore" clientPath
)

Target "RestoreServer" (fun () ->
  run dotnetCli "restore" serverPath
)

Target "Build" (fun () ->
  run dotnetCli "build" serverPath
  run dotnetCli "fable webpack -- -p" clientPath
)

Target "Run" (fun () ->
  let server = async {
    run dotnetCli "watch run" serverPath
  }
  let client = async {
    run dotnetCli "fable webpack-dev-server" clientPath
  }
  let browser = async {
    Threading.Thread.Sleep 5000
    Diagnostics.Process.Start "http://localhost:8080" |> ignore
  }

  [ server; client; browser ]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

#if (Docker)
Target "Bundle" (fun _ ->
  let serverDir = deployDir </> "Server"
  let clientDir = deployDir </> "Client"
  let publicDir = clientDir </> "public"

  let publishArgs = sprintf "publish -c Release -o \"%s\"" serverDir
  run dotnetCli publishArgs serverPath

  CopyDir publicDir "src/Client/public" allFiles
)

let dockerUser = "safe-template"
let dockerImageName = "safe-template"

let dockerFullName = sprintf "%s/%s" dockerUser dockerImageName

Target "Docker" (fun _ ->
  let buildArgs = sprintf "build -t %s ." dockerFullName
  run "docker" buildArgs "."

  let tagArgs = sprintf "tag %s %s" dockerFullName dockerFullName
  run "docker" tagArgs "."
)

#endif
#if (Azure)
Target "Publish" (fun () ->
  run yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
  run "dotnet" (sprintf "publish %s -c release -o %s" serverPath deployDir) __SOURCE_DIRECTORY__
  run "dotnet" "restore" clientPath
  run "dotnet" "fable webpack -- -p" clientPath
  CopyDir (deployDir </> "public") (clientPath </> "public") allFiles
)

type ArmOutput =
  { WebAppName : ParameterValue<string>
    WebAppPassword : ParameterValue<string> }
let environment = getBuildParamOrDefault "environment" (Guid.NewGuid().ToString().ToLower().Split '-' |> Array.head)
let subscriptionId = getBuildParam "subscriptionId"
let clientId = getBuildParam "clientId"

let mutable deploymentOutputs : ArmOutput option = None

Target "DeployArmTemplate" (fun _ ->
  let armTemplate = @"arm-template.json"
  let resourceGroupName = "safe-" + environment
  let subscriptionId = Guid.Parse subscriptionId
  let clientId = Guid.Parse clientId

  tracefn "Deploying template '%s' to resource group '%s' in subscription '%O'..." armTemplate resourceGroupName subscriptionId

  let authCtx =
    subscriptionId
    |> authenticateDevice trace { ClientId = clientId; TenantId = None }
    |> Async.RunSynchronously

  let deployment =
     { DeploymentName = "fake-deploy"
       ResourceGroup = New(resourceGroupName, Region.EuropeWest)
       ArmTemplate = IO.File.ReadAllText armTemplate
       Parameters = Simple [ "environment", ArmString environment ]
       DeploymentMode = Incremental }

  deployment
  |> deployWithProgress authCtx
  |> Seq.iter(function
    | DeploymentInProgress (state, operations) -> tracefn "State is %s, completed %d operations." state operations
    | DeploymentError (statusCode, message) -> traceError <| sprintf "DEPLOYMENT ERROR: %s - '%s'" statusCode message
    | DeploymentCompleted d -> deploymentOutputs <- d)
)

Target "DeployWebApp" (fun _ ->
  let zipFile = "deploy.zip"
  IO.File.Delete zipFile
  Zip deployDir zipFile !!(deployDir + @"\**\**")
  
  let appName = deploymentOutputs.Value.WebAppName.value
  let appPassword = deploymentOutputs.Value.WebAppPassword.value
  let destinationUri = sprintf "https://%s.scm.azurewebsites.net/api/zipdeploy" appName
  tracefn "Uploading %s to %s" zipFile destinationUri
  let client = new Net.WebClient(Credentials = Net.NetworkCredential("$" + appName, appPassword))
  client.UploadData(destinationUri, IO.File.ReadAllBytes zipFile) |> ignore
)

"Publish" ==> "DeployWebApp"
"DeployArmTemplate" ==> "DeployWebApp"

#endif
"Clean"
  ==> "InstallDotNetCore"
  ==> "InstallClient"
  ==> "Build"
#if (Docker)
  ==> "Bundle"
  ==> "Docker"
#endif

#if (Azure)
"Clean" ==> "Publish" ==> "DeployWebApp"
"DeployArmTemplate" ==> "DeployWebApp"

#endif
"InstallClient"
  ==> "RestoreServer"
  ==> "Run"

RunTargetOrDefault "Build"