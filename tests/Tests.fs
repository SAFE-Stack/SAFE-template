module SAFE.Tests

open System
open System.IO

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

let psi exe arg dir (x: ProcStartInfo) : ProcStartInfo =
    { x with
        FileName = exe
        Arguments = arg
        WorkingDirectory = dir }

let logger = Log.create "SAFE"

type TemplateArgs =
    { Server : string option
      Deploy : string option
      Layout : string option
      JsDeps : string option
      Remoting : bool }

    override args.ToString () =
        let optArg (name, value) =
            value
            |> Option.map (sprintf "--%s %s" name)
            |> Option.defaultValue ""

        let remoting = if args.Remoting then "--remoting" else ""

        [ "server", args.Server
          "deploy", args.Deploy
          "layout", args.Layout
          "js-deps", args.JsDeps ]
        |> List.map optArg
        |> fun x -> List.append x [remoting]
        |> List.map String.trim
        |> String.concat " "

let serverGen = Gen.elements [
    None
    Some "giraffe"
    Some "suave"
]

let deployGen = Gen.elements [
    None
    Some "docker"
    // https://travis-ci.org/SAFE-Stack/SAFE-template/builds/409831921?utm_source=github_status&utm_medium=notification
    // Some "azure"
]

let layoutGen = Gen.elements [
    None
    Some "fulma-basic"
    Some "fulma-admin"
    Some "fulma-cover"
    Some "fulma-hero"
    Some "fulma-landing"
    Some "fulma-login"
]

let jsDepsGen = Gen.elements [
    None
    Some "npm"
]

type TemplateArgsArb () =
    static member Arb () : Arbitrary<TemplateArgs> =
        let generator : Gen<TemplateArgs> =
            gen {
                let! server = serverGen
                let! deploy = deployGen
                let! layout = layoutGen
                let! jsDeps = jsDepsGen
                let! remoting = Gen.elements [false; true]
                return
                    { Server = server
                      Deploy = deploy
                      Layout = layout
                      JsDeps = jsDeps
                      Remoting = remoting }
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
                if x.Remoting then
                    yield { x with Remoting = false }
            }

        Arb.fromGenShrink (generator, shrinker)


let run exe arg dir =
    logger.info(
        eventX "Running `{exe} {arg}` in `{dir}`"
        >> setField "exe" exe
        >> setField "arg" arg
        >> setField "dir" dir)

    let result = Process.execWithResult (psi exe arg dir) TimeSpan.MaxValue
    Expect.isTrue (result.OK) (sprintf "`%s %s` failed: %A" exe arg result.Errors)

let fsCheckConfig =
    { FsCheckConfig.defaultConfig with
        arbitrary = [typeof<TemplateArgsArb>]
        maxTest = 10 }

[<Tests>]
let tests =
  testList "Project created from template" [
    testPropertyWithConfig fsCheckConfig "Project should build properly" (fun (x : TemplateArgs) ->
        let newSAFEArgs = x.ToString()
        let uid = Guid.NewGuid().ToString("n")
        let dir = Path.GetTempPath() </> uid
        Directory.create dir

        run dotnet (sprintf "new SAFE %s" newSAFEArgs) dir

        run fake "build" dir

        logger.info(
            eventX "Deleting `{dir}`"
            >> setField "dir" dir)
        Directory.delete dir
    )
  ]
