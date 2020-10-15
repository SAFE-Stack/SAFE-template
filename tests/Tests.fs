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

let maxTests =
    match Environment.GetEnvironmentVariable "MAX_TESTS" with
    | null -> 15
    | x ->
        match System.Int32.TryParse x with
        | true, n -> n
        | _ -> 15

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

let waitForStdOut (proc : Process) (stdOutPhrase : string) (timeout : TimeSpan) =
    let readTask =
        Task.Factory.StartNew (Func<_>(fun _ ->
            let mutable line = ""
            while line <> null && line.Contains stdOutPhrase |> not do
                line <- proc.StandardOutput.ReadLine()
                printfn "--> %s" line
        ))

    readTask.Wait timeout

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

    let proc =
        if templateType = Normal then
            run dotnet "tool restore" dir
            // see if `dotnet fake build` succeeds
            run dotnet ("fake build") dir
            start dotnet "fake build -t run" dir
        else
            run "npm" "install" (dir </> "src" </> "Client")
            start "npm" "run start" (dir </> "src" </> "Client")

    let extraProc =
        if templateType = Normal then None
        else start dotnet "run" (dir </> "src" </> "Server") |> Some

    // see if `dotnet fake build -t run` succeeds and webpack serves the index page
    let stdOutPhrase = ": Compiled successfully."
    let htmlSearchPhrase = """<title>SAFE Template</title>"""
    let timeout = TimeSpan.FromMinutes 5.
    try
        let waitResult = waitForStdOut proc stdOutPhrase timeout
        if waitResult then
            let response = get "http://localhost:8080"
            Expect.stringContains response htmlSearchPhrase
                (sprintf "html fragment not found for '%s'" args)
        else
            raise (Expecto.FailedException (sprintf "`dotnet fake build -t run` timeout for '%s'" args))
    finally
        killProcessTree proc.Id
        extraProc |> Option.map (fun p -> p.Id) |> Option.iter killProcessTree


    logger.info(
        eventX "Deleting `{dir}`"
        >> setField "dir" dir)
    Directory.delete dir

[<Tests>]
let tests =
    testList "Project created from template" [
        for build in [ Normal; Minimal ] do
            test (sprintf "%O template should build properly" build) { testTemplateBuild build }
    ]
