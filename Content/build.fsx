#r @"packages/build/FAKE/tools/FakeLib.dll"

open System

open Fake

let serverPath = "./src/Server" |> FullName
let clientPath = "./src/Client" |> FullName
let deployDir = "./deploy" |> FullName

let platformTool tool winTool =
  let tool = if isUnix then tool else winTool
  tool
  |> ProcessHelper.tryFindFileOnPath
  |> function Some t -> t | _ -> failwithf "%s not found" tool

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

  [ server; client; browser]
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

  !! "src/Client/public/**/*.*" |> CopyFiles publicDir

  !! "src/Client/index.html"
  ++ "src/Client/landing.css"
  |> CopyFiles clientDir 
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

"Clean"
  ==> "InstallDotNetCore"
  ==> "InstallClient"
  ==> "Build"
#if (Docker)
  ==> "Bundle"
  ==> "Docker"
#endif

"InstallClient"
  ==> "RestoreServer"
  ==> "Run"

RunTargetOrDefault "Build"