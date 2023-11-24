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
open Polly

let dotnet =
    match Environment.GetEnvironmentVariable "DOTNET_PATH" with
    | null -> "dotnet"
    | x -> x

let npm =
    // copied from Fake.JavaScript.Npm
    ProcessUtils.tryFindFileOnPath "npm"
    |> function
        | Some npm when File.Exists npm -> npm
        | _ ->
            match Environment.isWindows with
            | true -> "./packages/Npm.js/tools/npm.cmd"
            | _ -> "/usr/bin/npm"

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
    |> CreateProcess.redirectOutputIfNotRedirected
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
            try
                let! l =
                    proc.StandardOutput.ReadLineAsync()
                    |> Async.AwaitTask
                    |> asyncWithTimeout (TimeSpan.FromSeconds 30.)
                line <- l
            with :? TimeoutException ->
                failwith "Timeout occurred while waiting for line"
    } |> asyncWithTimeout timeout

let waitAndRetry seconds times (func: unit -> 'result) =
    Policy
        .Handle<Exception>()
        .WaitAndRetry(
            retryCount = times,
            sleepDurationProvider = (fun _ _ -> TimeSpan.FromSeconds seconds))
        .Execute(func)

let get (url: string) =
    use client = new HttpClient ()
    client.GetStringAsync url |> Async.AwaitTask |> Async.RunSynchronously

let childrenPids pid =
    let pgrep =
        if Environment.isWindows then
            CreateProcess.fromRawCommand "wmic" ["process"; "where"; sprintf "(ParentProcessId=%i)" pid; "get"; "ProcessId" ]
        else
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


let path = __SOURCE_DIRECTORY__ </> ".." </> "Content"

let testTemplateBuild templateType = testCase $"{templateType}" <| fun () ->
    let dir = if templateType = Normal then path </> "default" else path </> "minimal"

    run dotnet "tool restore" dir

    if templateType = Minimal then
        // run build on Shared to avoid race condition between Client and Server
        run dotnet "build" (dir </> "src" </> "Shared")

    if templateType = Normal then
        run dotnet "run" (dir </> "tests" </> "Server")

    let proc =
        if templateType = Normal then
            start dotnet "run" dir
        else
            run npm "install" dir
            start dotnet "fable watch --run vite" (dir </> "src" </> "Client" )

    let extraProc =
        if templateType = Normal then None
        else
            let proc = start dotnet "run" (dir </> "src" </> "Server")
            let wait = waitForStdOut proc "Now listening on:"
            Some (proc, wait)

    let stdOutPhrase = "ready in"
    let htmlSearchPhrase = """<title>SAFE Template</title>"""
    //vite will not serve up from root
    let clientUrl = "http://localhost:8080/index.html"
    let serverUrl, searchPhrase =
        match templateType with
        | Normal -> "http://localhost:5000/api/ITodosApi/getTodos", "Create new SAFE project" // JSON should contain a todo with such description
        | Minimal -> "http://localhost:5000/api/hello", "Hello from SAFE!"
    try
        let timeout = TimeSpan.FromMinutes 5.
        waitForStdOut proc stdOutPhrase timeout |> Async.RunSynchronously
        logger.info(
            eventX "Requesting `{url}`"
            >> setField "url" clientUrl)
        let response = waitAndRetry 3 5 (fun () -> get clientUrl)
        Expect.stringContains response htmlSearchPhrase
            (sprintf "html fragment not found for %A" templateType)
        extraProc |> Option.iter (fun (_, wait) -> Async.RunSynchronously (wait timeout))
        logger.info(
            eventX "Requesting `{url}`"
            >> setField "url" serverUrl)
        let response = get serverUrl
        Expect.stringContains response searchPhrase
            (sprintf "plaintext fragment not found for %A at %s" templateType serverUrl)
        logger.info(
            eventX "Run target for `{type}` run successfully"
            >> setField "type" templateType)
        if templateType = Normal then
            run dotnet "run -- bundle" dir
            logger.info(
                eventX "Bundle target for `{type}` run successfully"
                >> setField "type" templateType)
    finally
        killProcessTree proc.Id
        extraProc |> Option.map (fun (p,_) -> p.Id) |> Option.iter killProcessTree

