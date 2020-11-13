#r "nuget: System.Reactive"
#r "nuget: Fake.Core.Target,5.20.3"
#r "nuget: Fake.DotNet.Cli"
#r "nuget: Fake.IO.FileSystem"
#r "nuget: Farmer"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Farmer
open Farmer.Builders

let execContext = Context.FakeExecutionContext.Create false "build.fsx" [ ]
Context.setExecutionContext (Context.RuntimeContext.Fake execContext)

let sharedPath = Path.getFullName "./src/Shared"
let serverPath = Path.getFullName "./src/Server"
let deployDir = Path.getFullName "./deploy"
let sharedTestsPath = Path.getFullName "./tests/Shared"
let serverTestsPath = Path.getFullName "./tests/Server"

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


Target.create "ToolRestore" (fun _ -> dotnet "tool restore" ".")

Target.create "Clean" (fun _ -> Shell.cleanDir deployDir)

Target.create "InstallClient" (fun _ -> npm "install" ".")

Target.create "Bundle" (fun _ ->
    dotnet (sprintf "publish -c Release -o \"%s\"" deployDir) serverPath
    dotnet "fable --run webpack -p" "src/Client"
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
    dotnet "build" sharedPath
    [ async { dotnet "watch run" serverPath }
      async { dotnet "fable watch --run webpack-dev-server" "src/Client" } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "RunTests" (fun _ ->
    dotnet "build" sharedTestsPath
    [ async { dotnet "watch run" serverTestsPath }
      async { dotnet "fable watch --run webpack-dev-server --config ../../webpack.tests.config.js" "tests/Client" } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

open Fake.Core.TargetOperators

"ToolRestore"
    ==> "Clean"
    ==> "InstallClient"
    ==> "Bundle"
    ==> "Azure"

"Clean"
    ==> "InstallClient"
    ==> "Run"

"Clean"
    ==> "InstallClient"
    ==> "RunTests"

match fsi.CommandLineArgs |> Array.skip 1 with
| [|"-t"; target|] -> target
| [||] -> "Bundle"
| args -> failwithf "Invalid arguments %A" args
|> Target.runOrDefaultWithArguments

