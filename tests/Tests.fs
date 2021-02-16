module SAFE.Tests

open System
open System.Diagnostics
open System.IO
open System.Net.Http

open Expecto
open Expecto.Logging
open Expecto.Logging.Message

open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators

let dotnet =
    match Environment.GetEnvironmentVariable "DOTNET_PATH" with
    | null -> "dotnet"
    | x -> x
let execParams exe arg dir : ExecParams =
    { Program = exe
      WorkingDir = dir
      CommandLine = arg
      Args = [] }

let logger = Log.create "SAFE"
let run exe arg dir =
    logger.info(
        eventX "Running `{exe} {arg}` in `{dir}`"
        >> setField "exe" exe
        >> setField "arg" arg
        >> setField "dir" dir)

    CreateProcess.fromRawCommandLine exe arg
    |> CreateProcess.withWorkingDirectory dir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

open System.Threading.Tasks

let start exe arg dir =
    logger.info(
        eventX "Starting `{exe} {arg}` in `{dir}`"
        >> setField "exe" exe
        >> setField "arg" arg
        >> setField "dir" dir)

    let psi =
        { ProcStartInfo.Create() with
            FileName = exe
            Arguments = arg
            WorkingDirectory = dir
            RedirectStandardOutput = true
            RedirectStandardInput = true
            UseShellExecute = false }.AsStartInfo

    Process.Start psi

let asyncWithTimeout (timeout: TimeSpan) action =
  async {
    let! child = Async.StartChild( action, int timeout.TotalMilliseconds )
    return! child
  }

let waitForStdOut (proc : Process) (stdOutPhrase : string) timeout =
    async {
        let mutable line = ""
        while line <> null && line.Contains stdOutPhrase |> not do
            let! l =
                proc.StandardOutput.ReadLineAsync()
                |> Async.AwaitTask
            line <- l
            printfn "--> %s" line
    } |> asyncWithTimeout timeout

let get (url: string) =
    use client = new HttpClient ()
    client.GetStringAsync url |> Async.AwaitTask |> Async.RunSynchronously

// works just on unix (`pgrep`) for now
let childrenPids pid =
    let pgrep =
        CreateProcess.fromRawCommand "pgrep" ["-P"; string pid]
        |> CreateProcess.redirectOutput
        |> Proc.run

    pgrep.Result.Output.Split ([|'\n'|])
    |> Array.choose
        (fun x ->
            match System.Int32.TryParse x with
            | true, y -> Some y
            | _ -> None)

let killProcessTree (pid: int) =
    let rec getProcessTree (pid: int) =
        [ for childPid in childrenPids pid do
            yield! getProcessTree childPid
          pid ]

    for pid in getProcessTree pid do
        let proc =
            try
                Process.GetProcessById pid |> Some
            // The process specified by the processId parameter is not running. The identifier might be expired.
            with :? ArgumentException ->
                logger.warn(
                        eventX "Can't kill process {pid}: process is not running"
                        >> setField "pid" pid)
                None

        match proc with
        | Some proc when not proc.HasExited ->
            logger.info(
                eventX "Killing process {pid}"
                >> setField "pid" pid)
            try
                proc.Kill ()
            with e ->
                logger.warn(
                    eventX "Failed to kill process {pid}: {msg}"
                    >> setField "pid" pid
                    >> setField "msg" e.Message)
        | _ ->
            ()

type TemplateType = Normal | Minimal


let testTemplateBuild templateType =
    let args = match templateType with Normal -> "" | Minimal -> " -m"
    let uid = Guid.NewGuid().ToString("n")
    let dir = Path.GetTempPath() </> uid
    Directory.create dir

    run dotnet (sprintf "new SAFE %s" args) dir

    if templateType = Minimal then
        // run build on Shared to avoid race condition between Client and Server
        run dotnet "build" (dir </> "src" </> "Shared")

    run dotnet "tool restore" dir
    let proc =
        if templateType = Normal then
            start dotnet "run" dir
        else
            run "npm" "install" dir
            start dotnet "fable watch src/Client --run webpack-dev-server" dir

    let extraProc =
        if templateType = Normal then None
        else start dotnet "run" (dir </> "src" </> "Server") |> Some

    let stdOutPhrase = ": Compiled successfully."
    let htmlSearchPhrase = """<title>SAFE Template</title>"""
    try
        let timeout = TimeSpan.FromMinutes 5.
        waitForStdOut proc stdOutPhrase timeout |> Async.RunSynchronously
        let response = get "http://localhost:8080"
        Expect.stringContains response htmlSearchPhrase
            (sprintf "html fragment not found for '%s'" args)
    finally
        killProcessTree proc.Id
        extraProc |> Option.map (fun p -> p.Id) |> Option.iter killProcessTree


    logger.info(
        eventX "Deleting `{dir}`"
        >> setField "dir" dir)
    Directory.delete dir

let tests =
    testList "Project created from template" [
        for build in [ Minimal; Normal ] do
            test (sprintf "%O template should build properly" build) {
                testTemplateBuild build
            }
    ]

