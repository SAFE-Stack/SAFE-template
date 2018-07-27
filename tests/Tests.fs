module SAFE.Tests

open System
open System.IO

open Expecto
open Expecto.Logging
open Expecto.Logging.Message

open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators

let psi exe arg dir (x: ProcStartInfo) : ProcStartInfo =
    { x with
        FileName = exe
        Arguments = arg
        WorkingDirectory = dir }

let logger = Log.create "SAFE"

let serverOpts =
    [ "saturn"
      "giraffe"
      "suave"
    ]

let deployOpts =
    [ "none"
    ]

let layoutOpts =
    [ "fulma-basic"
    ]

let remotingOpts =
    [ ""
    ]

let jsDepsOpts =
    [ "yarn"
    ]

let run exe arg dir =
    logger.info(
        eventX "Running `{exe} {arg}` in `{dir}`"
        >> setField "exe" exe
        >> setField "arg" arg
        >> setField "dir" dir)

    let result = Process.execWithResult (psi exe arg dir) TimeSpan.MaxValue
    Expect.isTrue (result.OK) (sprintf "`%s %s` failed: %A" exe arg result.Errors)

[<Tests>]
let tests =
  testList "Project created from template" [
    for server in serverOpts do
    for deploy in deployOpts do
    for layout in layoutOpts do
    for remoting in remotingOpts do
    for jsDeps in jsDepsOpts do

        let newSAFEArgs =
            sprintf
                "--server %s --deploy %s --layout %s --js-deps %s %s"
                server
                deploy
                layout
                jsDeps
                remoting

        yield testCase (sprintf "Template created with args `%s` should build properly" newSAFEArgs) (fun () ->
            let uid = Guid.NewGuid().ToString("n")
            let dir = Path.GetTempPath() </> uid
            Directory.create dir

            let dotnet = "/usr/local/share/dotnet/dotnet"
            run dotnet (sprintf "new SAFE %s" newSAFEArgs) dir

            run "fake" "build" dir

            logger.info(
                eventX "Deleting `{dir}`"
                >> setField "dir" dir)
            Directory.delete dir
        )
  ]
