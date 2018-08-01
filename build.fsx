#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open System

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Tools

let templatePath = "./Content/.template.config/template.json"
let templateProj = "SAFE.Template.proj"
let templateName = "SAFE-Stack Web App"
let nupkgDir = Path.getFullName "./nupkg"

let release = ReleaseNotes.load "RELEASE_NOTES.md"

let formattedRN =
    release.Notes
    |> List.map (sprintf "* %s")
    |> String.concat "\n"

Target.create "Clean" (fun _ ->
    Shell.cleanDirs [ nupkgDir ]
    Git.CommandHelper.directRunGitCommandAndFail "./Content" "clean -fxd"
)

Target.create "Pack" (fun _ ->
    Shell.regexReplaceInFileWithEncoding
        "  \"name\": .+,"
       ("  \"name\": \"" + templateName + " v" + release.NugetVersion + "\",")
        System.Text.Encoding.UTF8
        templatePath
    DotNet.pack
        (fun args ->
            { args with
                    OutputPath = Some nupkgDir
                    Common =
                        { args.Common with
                            CustomParams =
                                Some (sprintf "/p:PackageVersion=%s /p:PackageReleaseNotes=\"%s\""
                                        release.NugetVersion
                                        formattedRN) }
            })
        templateProj
)

Target.create "Install" (fun _ ->
    let args = sprintf "-i %s/SAFE.Template.%s.nupkg" nupkgDir release.NugetVersion
    let result = DotNet.exec (fun x -> { x with DotNetCliPath = "dotnet" }) "new" args
    if not result.OK then failwithf "`dotnet %s` failed" args
)

let psi exe arg dir (x: ProcStartInfo) : ProcStartInfo =
    { x with
        FileName = exe
        Arguments = arg
        WorkingDirectory = dir }

let run exe arg dir =
    let result = Process.execWithResult (psi exe arg dir) TimeSpan.MaxValue
    if not result.OK then (failwithf "`%s %s` failed: %A" exe arg result.Errors)

type BuildPaketDependencies =
    { Azure : bool }

    with override x.ToString () =
            if x.Azure then "azure" else "noazure"

type ClientPaketDependencies =
    { Remoting : bool
      Fulma : bool }

    with override x.ToString () =
            let remoting = if x.Remoting then "remoting" else "noremoting"
            let fulma = if x.Fulma then "fulma" else "nofulma"
            sprintf "%s-%s" remoting fulma

type ServerPaketDependency = Saturn | Giraffe | Suave

    with override x.ToString () =
            match x with
            | Saturn -> "saturn"
            | Giraffe -> "giraffe"
            | Suave -> "suave"

type ServerPaketDependencies =
    { Server : ServerPaketDependency
      Remoting : bool
      Azure : bool }

    with override x.ToString () =
            let server = string x.Server
            let remoting = if x.Remoting then "remoting" else "noremoting"
            let azure = if x.Azure then "azure" else "noazure"
            sprintf "%s-%s-%s" server remoting azure

let buildGroups =
    [ { Azure = false } ]

let clientGroups =
    [ { Remoting = false
        Fulma = true } ]

let serverGroups =
    [ { Server = Saturn
        Remoting = false
        Azure = false } ]

Target.create "BuildPaketLockFiles" (fun _ ->

    for buildGroup in buildGroups do
    for clientGroup in clientGroups do
    for serverGroup in serverGroups do
        let contents =
            [
                "Content" </> "src" </> "Build" </> sprintf "paket_%A.lock" buildGroup
                "Content" </> "src" </> "Client" </> sprintf "paket_%A.lock" clientGroup
                "Content" </> "src" </> "Server" </> sprintf "paket_%A.lock" serverGroup
            ]
            |> List.map File.read
            |> Seq.concat

        let lockName = sprintf "paket_%A_%A_%A.lock" buildGroup clientGroup serverGroup

        File.writeWithEncoding (Text.UTF8Encoding(false)) false ("Content" </> lockName) contents
)

Target.create "GenPaketLockFiles" (fun _ ->
    let baseDir = "gen-paket-lock-files"
    Directory.delete baseDir
    Directory.create baseDir
    for server in [ "saturn" ] do
        let dirName = baseDir </> server
        Directory.create dirName
        run "dotnet" (sprintf "new SAFE --server %s" server) dirName

        run "mono" ".paket/paket.exe update --group Build" dirName

        let lockFile = dirName </> "paket.lock"
        let lines = File.readAsString lockFile
        Directory.delete dirName
        Directory.create dirName
        let delimeter = "GROUP "
        let groups =
            lines
            |> String.splitStr delimeter
            |> List.filter (String.isNullOrWhiteSpace >> not)
            |> List.map (fun group -> group.Substring(0, group.IndexOf Environment.NewLine), delimeter + group)
        for (name, group) in groups do
            let fileName = sprintf "paket-%s.lock" name
            File.writeString false (dirName </> fileName) group
            Shell.copyFile ("Content" </> "src" </> name </> "paket-default.lock") (dirName </> fileName)
)

Target.create "Tests" (fun _ ->
    let cmd = "run"
    let args = "--project tests/tests.fsproj"
    let result = DotNet.exec id cmd args
    if not result.OK then failwithf "`dotnet %s %s` failed" cmd args
)

Target.create "Push" (fun _ ->
    Paket.push ( fun args ->
        { args with
                PublishUrl = "https://www.nuget.org"
                WorkingDir = nupkgDir
        }
    )

    let remoteGit = "upstream"
    let commitMsg = sprintf "Bumping version to %O" release.NugetVersion
    let tagName = string release.NugetVersion

    Git.Branches.checkout "" false "master"
    Git.CommandHelper.directRunGitCommand "" "fetch origin" |> ignore
    Git.CommandHelper.directRunGitCommand "" "fetch origin --tags" |> ignore

    Git.Staging.stageAll ""
    Git.Commit.exec "" commitMsg
    Git.Branches.pushBranch "" remoteGit "master"

    Git.Branches.tag "" tagName
    Git.Branches.pushTag "" remoteGit tagName
)

Target.create "Release" ignore

open Fake.Core.TargetOperators

"Clean"
    ==> "Pack"
    ==> "Install"
    ==> "Tests"
    ==> "Push"
    ==> "Release"

Target.runOrDefault "Pack"