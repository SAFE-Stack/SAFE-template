namespace SAFE.Build

open System

open Fake.Core
open Fake.DotNet
open Fake.IO

[<RequireQualifiedAccess>]
module FakeTargets =

    [<AutoOpen>]
    module private Internal =
        let serverPath = Path.getFullName "./src/Server"
        let clientPath = Path.getFullName "./src/Client"
        let deployDir = Path.getFullName "./deploy"

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

    let create () =
        Target.create "Clean" (fun _ ->
            Shell.cleanDirs [deployDir]
        )

        Target.create "InstallClient" (fun _ ->
            printfn "Node version:"
            runTool nodeTool "--version" __SOURCE_DIRECTORY__
            printfn "Yarn version:"
            runTool yarnTool "--version" __SOURCE_DIRECTORY__
            runTool yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
            runDotNet "restore" clientPath
        )

        Target.create "RestoreServer" (fun _ ->
            runDotNet "restore" serverPath
        )

        Target.create "Build" (fun _ ->
            runDotNet "build" serverPath
            runDotNet "fable webpack -- -p" clientPath
        )

        Target.create "Run" (fun _ ->
            let server = async {
                runDotNet "watch run" serverPath
            }
            let client = async {
                runDotNet "fable webpack-dev-server" clientPath
            }
            let browser = async {
                Threading.Thread.Sleep 5000
                openBrowser "http://localhost:8080"
            }

            [ server; client; browser ]
            |> Async.Parallel
            |> Async.RunSynchronously
            |> ignore
        )
