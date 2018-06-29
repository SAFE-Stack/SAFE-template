namespace SAFE.Build

open System

open Fake.Core
open Fake.DotNet
open Fake.IO

open Cit.Helpers.Arm.Parameters

[<RequireQualifiedAccess>]
module SAFE =
    [<AutoOpen>]
    module private Internal =
        let serverPath = Path.getFullName "./src/Server"
        let clientPath = Path.getFullName "./src/Client"
        let deployPath = Path.getFullName "./deploy"

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

    let restoreClient () =
        printfn "Node version:"
        runTool nodeTool "--version" __SOURCE_DIRECTORY__
        printfn "Yarn version:"
        runTool yarnTool "--version" __SOURCE_DIRECTORY__
        runTool yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
        runDotNet "restore" clientPath

    let restoreServer () =
        runDotNet "restore" serverPath

    let buildClient () =
        runDotNet "fable webpack -- -p" clientPath

    let buildServer () = 
        runDotNet "build" serverPath

    let runServer = async {
        runDotNet "watch run" serverPath
    }

    let runClient = async {
        runDotNet "fable webpack-dev-server" clientPath
    }

    let runBrowser = async {
        Threading.Thread.Sleep 5000
        openBrowser "http://localhost:8080"
    }

    let runInParallel tasks =
        tasks
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore

    module Azure =
                
        type ArmOutput =
            { WebAppName : ParameterValue<string>
              WebAppPassword : ParameterValue<string> }
        
        let mutable private deploymentOutputs : ArmOutput option = None

        let bundle () =
            runDotNet (sprintf "publish %s -c release -o %s" serverPath deployPath) __SOURCE_DIRECTORY__
            Shell.copyDir (Path.combine deployPath "public") (Path.combine clientPath "public") FileFilter.allFiles

        