open Fake.Core
open Fake.IO
open Farmer
open Farmer.Builders

let execContext = Context.FakeExecutionContext.Create false "build.fsx" [ ]
Context.setExecutionContext (Context.RuntimeContext.Fake execContext)

let sharedPath = Path.getFullName "src/Shared"
let serverPath = Path.getFullName "src/Server"
let clientPath = Path.getFullName "src/Client"
let deployDir = Path.getFullName "deploy"
let sharedTestsPath = Path.getFullName "tests/Shared"
let serverTestsPath = Path.getFullName "tests/Server"

let run exe arg dir =
    CreateProcess.fromRawCommandLine exe arg
    |> CreateProcess.withWorkingDirectory dir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

Target.create "Clean" (fun _ -> Shell.cleanDir deployDir)

Target.create "InstallClient" (fun _ -> run "npm" "install" ".")

Target.create "Bundle" (fun _ ->
    run "dotnet" (sprintf "publish -c Release -o \"%s\"" deployDir) serverPath
    run "dotnet" "fable --run webpack -p" clientPath
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
    run "dotnet" "build" sharedPath
    [ async { run "dotnet" "watch run" serverPath }
      async { run "dotnet" "fable watch --run webpack-dev-server" clientPath } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "RunTests" (fun _ ->
    run "dotnet" "build" sharedTestsPath
    [ async { run "dotnet" "watch run" serverTestsPath }
      async { run "dotnet" "fable watch --run webpack-dev-server --config ../../webpack.tests.config.js" "tests/Client" } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "Format" (fun _ ->
    run "dotnet" "fantomas . -r" "src"
)

open Fake.Core.TargetOperators

let dependencies = [
    "Clean"
        ==> "InstallClient"
        ==> "Bundle"
        ==> "Azure"

    "Clean"
        ==> "InstallClient"
        ==> "Run"

    "Clean"
        ==> "InstallClient"
        ==> "RunTests"
]

[<EntryPoint>]
let main args =
    try
        match args with
        | [| target |] -> Target.runOrDefault target
        | _ -> Target.runOrDefault "Run"
        0
    with e ->
        printfn "%A" e
        1
