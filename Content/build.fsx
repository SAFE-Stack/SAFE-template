#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"
#r "netstandard"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Farmer
open Farmer.Builders

Target.initEnvironment ()

let serverPath = Path.getFullName "./src/Server"
let clientPath = Path.getFullName "./src/Client"
let deployDir = Path.getFullName "./deploy"

let npm args workingDir =
    let npmPath =
        match ProcessUtils.tryFindFileOnPath "npm" with
        | Some path -> path
        | None ->
            "npm was not found in path. Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
            |> failwith

    let arguments = args |> String.split ' ' |> Arguments.OfArgs

    Command.RawCommand (npmPath, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let dotnet cmd workingDir =
    let result = DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

Target.create "Clean" (fun _ -> Shell.cleanDir deployDir)

Target.create "InstallClient" (fun _ -> npm "install" clientPath)

Target.create "Bundle" (fun _ ->
    dotnet (sprintf "publish -c Release -o \"%s\"" deployDir) serverPath
    npm "run build" clientPath
)

Target.create "Azure" (fun _ ->
    let web = webApp {
        name "SAFE.App"
        zip_deploy "deploy"
    }
    let deployment = arm {
        location Location.WestEurope
        add_resource web
    }

    deployment
    |> Deploy.execute "SAFE.App" Deploy.NoParameters
    |> ignore
)

Target.create "Run" (fun _ ->
    [ async { dotnet "watch run" serverPath }
      async { npm "run start" clientPath } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

open Fake.Core.TargetOperators

"Clean"
    ==> "InstallClient"
    ==> "Bundle"
    ==> "Azure"

"Clean"
    ==> "InstallClient"
    ==> "Run"

Target.runOrDefaultWithArguments "Bundle"
