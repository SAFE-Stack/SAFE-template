module SAFE.Tests

open System
open System.IO

open Expecto
open Expecto.Logging
open Expecto.Logging.Message


open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators

let options workDir (x: DotNet.Options) : DotNet.Options =
    { x with
        DotNetCliPath = "/usr/local/share/dotnet/dotnet"
        WorkingDirectory = workDir }

let psi workDir (x: ProcStartInfo) : ProcStartInfo =
    { x with
        FileName = "fake"
        WorkingDirectory = workDir
        Arguments = "build" }

let logger = Log.create "SAFE"

let layoutArgs = ["none"; "azure"; "docker"]
let deployArgs = ["none"; "azure"; "docker"]

[<Tests>]
let tests =
  testList "Project created from template" [
    testCase "universe exists (╭ರᴥ•́)" <| fun _ ->
      let subject = true
      Expect.isTrue subject "I compute, therefore I am."

    testCase "when true is not (should fail)" <| fun _ ->
      let subject = false
      Expect.isTrue subject "I should fail because the subject is false"
    (*
    for deploy in deployArgs ->
        let args = sprintf "SAFE --deploy %s" deploy
        testCase (sprintf "should build properly `%s`" args) (fun () ->
            let uid = Guid.NewGuid().ToString("n")
            let dir = Path.GetTempPath() </> uid
            Directory.create dir

            let cmd = "new"

            printfn "Running `dotnet %s %s` in `%s`" cmd args dir
            let result = DotNet.exec (options dir) cmd args
            Expect.isTrue (result.OK) (sprintf "`dotnet %s %s` failed" cmd args)

            printfn "Running `fake build` in `%s`" dir
            let result = Process.execWithResult (psi dir) TimeSpan.MaxValue
            Expect.isTrue (result.OK) "`fake build` failed"

            printfn "Deleting `%s`" dir
            Directory.delete dir
        )
        *)
  ]
