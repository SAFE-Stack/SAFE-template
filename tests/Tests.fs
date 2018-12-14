module SAFE.Tests

open System
open System.Diagnostics
open System.IO
open System.Net.Http

open Expecto
open Expecto.Logging
open Expecto.Logging.Message

open FsCheck

open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators

let dotnet =
    match Environment.GetEnvironmentVariable "DOTNET_PATH" with
    | null -> "dotnet"
    | x -> x

let fake =
    match Environment.GetEnvironmentVariable "FAKE_PATH" with
    | null -> "fake"
    | x -> x

let maxTests =
    match Environment.GetEnvironmentVariable "MAX_TESTS" with
    | null -> 20
    | x ->
        match System.Int32.TryParse x with
        | true, n -> n
        | _ -> 20

let execParams exe arg dir : ExecParams =
    { Program = exe
      WorkingDir = dir
      CommandLine = arg
      Args = [] }

let logger = Log.create "SAFE"

type TemplateArgs =
    { Server : string option
      Deploy : string option
      Layout : string option
      JsDeps : string option
      Communication : string option
      Pattern : string option }

    override args.ToString () =
        let optArg (name, value) =
            value
            |> Option.map (sprintf "--%s %s" name)

        [ "server", args.Server
          "deploy", args.Deploy
          "layout", args.Layout
          "js-deps", args.JsDeps
          "communication", args.Communication
          "pattern", args.Pattern ]
        |> List.map optArg
        |> List.choose id
        |> String.concat " "

let serverGen =
    Gen.elements [
        None
        Some "giraffe"
        Some "suave"
    ]

let deployGen =
    Gen.elements [
        None
        Some "docker"
        Some "azure"
    ]

let layoutGen =
    Gen.elements [
        None
        Some "fulma-basic"
        Some "fulma-admin"
        Some "fulma-cover"
        Some "fulma-hero"
        Some "fulma-landing"
        Some "fulma-login"
    ]

let jsDepsGen =
    Gen.elements [
        None
        Some "npm"
    ]

let communicationGen =
    Gen.elements [
        None
        Some "remoting"
    ]

let patternGen =
    Gen.elements [
        None
        Some "reaction"
    ]

type TemplateArgsArb () =
    static member Arb () : Arbitrary<TemplateArgs> =
        let generator : Gen<TemplateArgs> =
            gen {
                let! server = serverGen
                let! deploy = deployGen
                let! layout = layoutGen
                let! jsDeps = jsDepsGen
                let! communication = communicationGen
                let! pattern = patternGen
                return
                    { Server = server
                      Deploy = deploy
                      Layout = layout
                      JsDeps = jsDeps
                      Communication = communication
                      Pattern = pattern }
            }

        let shrinker (x : TemplateArgs) : seq<TemplateArgs> =
            seq {
                match x.Server with
                | Some _ -> yield { x with Server = None }
                | _ -> ()
                match x.Deploy with
                | Some _ -> yield { x with Deploy = None }
                | _ -> ()
                match x.Layout with
                | Some _ -> yield { x with Layout = None }
                | _ -> ()
                match x.JsDeps with
                | Some _ -> yield { x with JsDeps = None }
                | _ -> ()
                match x.Communication with
                | Some _ -> yield { x with Communication = None }
                | _ -> ()
                match x.Pattern with
                | Some _ -> yield { x with Pattern = None }
                | _ -> ()
            }

        Arb.fromGenShrink (generator, shrinker)


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

    if readTask.Wait timeout then
        ()
    else
        failwithf "Timeout!"

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
          yield pid ]

    for pid in getProcessTree pid do
        logger.info(
            eventX "killing process {pid}"
            >> setField "pid" pid)
        (Process.GetProcessById pid).Kill ()

let fsCheckConfig =
    { FsCheckConfig.defaultConfig with
        arbitrary = [typeof<TemplateArgsArb>]
        maxTest = maxTests }

[<Tests>]
let tests =
    testList "Project created from template" [
        testPropertyWithConfig fsCheckConfig "Project should build properly" (fun (x : TemplateArgs) ->
            let newSAFEArgs = x.ToString()
            let uid = Guid.NewGuid().ToString("n")
            let dir = Path.GetTempPath() </> uid
            Directory.create dir

            run dotnet (sprintf "new SAFE %s" newSAFEArgs) dir

            Expect.isTrue (File.exists (dir </> "paket.lock")) (sprintf "paket.lock not present for '%s'" newSAFEArgs)

            // see if `fake build` succeeds
            run fake "build" dir

            // see if `fake build -t run` succeeds and webpack serves the index page
            let stdOutPhrase = ": Compiled successfully."
            let htmlSearchPhrase = """<title>SAFE Template</title>"""
            let timeout = TimeSpan.FromMinutes 5.
            let proc = start fake "build -t run" dir
            try
                waitForStdOut proc stdOutPhrase timeout
            finally
                killProcessTree proc.Id
            let response = get "http://localhost:8080"
            Expect.stringContains response htmlSearchPhrase (sprintf "html fragment not found for '%s'" newSAFEArgs)

            logger.info(
                eventX "Deleting `{dir}`"
                >> setField "dir" dir)
            Directory.delete dir
        )
    ]
