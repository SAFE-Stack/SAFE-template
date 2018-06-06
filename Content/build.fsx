#r @"packages/build/FAKE/tools/FakeLib.dll"
//#if (deploy == "azure")
#r "netstandard"
#I "packages/build/Microsoft.Rest.ClientRuntime.Azure/lib/net452"
#load ".paket/load/netcoreapp2.1/Build/build.group.fsx"
#load @"paket-files/build/CompositionalIT/fshelpers/src/FsHelpers/ArmHelper/ArmHelper.fs"

open Cit.Helpers.Arm
open Cit.Helpers.Arm.Parameters
open Microsoft.Azure.Management.ResourceManager.Fluent.Core
//#endif
open System

// TODO: remove Fake
open Fake

open Fake.Core
open Fake.DotNet
open Fake.IO

let serverPath = Path.getFullName "./src/Server"
let clientPath = Path.getFullName "./src/Client"
let deployDir = Path.getFullName "./deploy"

let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool
    match Process.tryFindFileOnPath tool with Some t -> t | _ -> failwithf "%s not found" tool

let nodeTool = platformTool "node" "node.exe"
//#if (js-deps == "npm")
let npmTool = platformTool "npm" "npm.cmd"
//#else
let yarnTool = platformTool "yarn" "yarn.cmd"
//#endif

let dotnetcliVersion = DotNet.getSDKVersionFromGlobalJson()
let mutable dotnetCli = "dotnet"

let run cmd args workingDir =
    let result =
        Process.execSimple (fun info ->
            { info with
                FileName = cmd
                WorkingDirectory = workingDir
                Arguments = args })
            TimeSpan.MaxValue
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
//#if (js-deps == "npm")
    printfn "Npm version:"
    run npmTool "--version"  __SOURCE_DIRECTORY__
    run npmTool "install" __SOURCE_DIRECTORY__
//#else
    printfn "Yarn version:"
    run yarnTool "--version" __SOURCE_DIRECTORY__
    run yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
//#endif
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

//#if (deploy == "docker")
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

//#endif
//#if (deploy == "azure")
Target "Bundle" (fun () ->
    run dotnetCli (sprintf "publish %s -c release -o %s" serverPath deployDir) __SOURCE_DIRECTORY__
    CopyDir (deployDir </> "public") (clientPath </> "public") allFiles
)

type ArmOutput =
    { WebAppName : ParameterValue<string>
      WebAppPassword : ParameterValue<string> }
let mutable deploymentOutputs : ArmOutput option = None

Target "ArmTemplate" (fun _ ->
    let environment = getBuildParamOrDefault "environment" (Guid.NewGuid().ToString().ToLower().Split '-' |> Array.head)
    let armTemplate = @"arm-template.json"
    let resourceGroupName = "safe-" + environment

    let authCtx =
        // You can safely replace these with your own subscription and client IDs hard-coded into this script.
        let subscriptionId = try getBuildParam "subscriptionId" |> Guid.Parse with _ -> failwith "Invalid Subscription ID. This should be your Azure Subscription ID."
        let clientId = try getBuildParam "clientId" |> Guid.Parse with _ -> failwith "Invalid Client ID. This should be the Client ID of a Native application registered in Azure with permission to create resources in your subscription."

        tracefn "Deploying template '%s' to resource group '%s' in subscription '%O'..." armTemplate resourceGroupName subscriptionId
        subscriptionId
        |> authenticateDevice trace { ClientId = clientId; TenantId = None }
        |> Async.RunSynchronously

    let deployment =
        let location = getBuildParamOrDefault "location" Region.EuropeWest.Name
        let pricingTier = getBuildParamOrDefault "pricingTier" "F1"
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
        | DeploymentInProgress (state, operations) -> tracefn "State is %s, completed %d operations." state operations
        | DeploymentError (statusCode, message) -> traceError <| sprintf "DEPLOYMENT ERROR: %s - '%s'" statusCode message
        | DeploymentCompleted d -> deploymentOutputs <- d)
)

Target "AppService" (fun _ ->
    let zipFile = "deploy.zip"
    IO.File.Delete zipFile
    Zip deployDir zipFile !!(deployDir + @"\**\**")

    let appName = deploymentOutputs.Value.WebAppName.value
    let appPassword = deploymentOutputs.Value.WebAppPassword.value

    let destinationUri = sprintf "https://%s.scm.azurewebsites.net/api/zipdeploy" appName
    let client = new Net.WebClient(Credentials = Net.NetworkCredential("$" + appName, appPassword))
    tracefn "Uploading %s to %s" zipFile destinationUri
    client.UploadData(destinationUri, IO.File.ReadAllBytes zipFile) |> ignore)

//#endif
"Clean"
    ==> "InstallDotNetCore"
    ==> "InstallClient"
    ==> "Build"
//#if (deploy == "docker")
    ==> "Bundle"
    ==> "Docker"
//#elseif (deploy == "azure")
    ==> "Bundle"
    ==> "ArmTemplate"
    ==> "AppService"
//#endif

"InstallClient"
    ==> "RestoreServer"
    ==> "Run"

RunTargetOrDefault "Build"
