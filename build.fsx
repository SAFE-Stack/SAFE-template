#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#endif

open System

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Tools

let templatePath = "./Content/.template.config/template.json"
let versionFilePath = "./Content/src/Client/Version.fs"
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
)

Target.create "BuildWebPackConfig" (fun _ ->
    let srcDir = "paket-files/fable-compiler/webpack-config-template/webpack.config.js"
    let destDir = "Content/src/Client/webpack.config.js"
    Shell.copyFile destDir srcDir

    let devServerProxy =
        """{
        // redirect requests that start with /api/ to the server on port 8085
        '/api/**': {
            target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
               changeOrigin: true
           },
        // redirect websocket requests that start with /socket/ to the server on the port 8085
        '/socket/**': {
            target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
            ws: true
           }
       }"""

    let quote = sprintf "'%s'"

    let replacements =
        [
            "indexHtmlTemplate", quote "./index.html"
            "fsharpEntry", quote "./Client.fsproj"
            "cssEntry", quote "./style.scss"
            "devServerProxy", devServerProxy
        ]

    for (key, value) in replacements do
        Shell.regexReplaceInFileWithEncoding
           (sprintf "    %s: .+," key)
           (sprintf "    %s: %s," key value)
            System.Text.Encoding.UTF8
            destDir
)

Target.create "Pack" (fun _ ->
    Shell.regexReplaceInFileWithEncoding
        "  \"name\": .+,"
       ("  \"name\": \"" + templateName + " v" + release.NugetVersion + "\",")
        System.Text.Encoding.UTF8
        templatePath
    Shell.regexReplaceInFileWithEncoding
        "let template = \".+\""
       ("let template = \"" + release.NugetVersion + "\"")
        System.Text.Encoding.UTF8
        versionFilePath
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
    let args=
      let nupkgFileName = sprintf "SAFE.Template.%s.nupkg" release.NugetVersion
      let fullPathToNupkg = System.IO.Path.Combine(nupkgDir, nupkgFileName)
      sprintf "-i \"%s\"" fullPathToNupkg
    let result = DotNet.exec (fun x -> { x with DotNetCliPath = "dotnet" }) "new" args
    if not result.OK then failwithf "`dotnet %s` failed with %O" args result
)

let psi exe arg dir (x: ProcStartInfo) : ProcStartInfo =
    { x with
        FileName = exe
        Arguments = arg
        WorkingDirectory = dir }

let run exe arg dir =
    let result = Process.execWithResult (psi exe arg dir) TimeSpan.MaxValue
    if not result.OK then (failwithf "`%s %s` failed: %A" exe arg result.Errors)

type Communication =
    | Remoting
    | Bridge


type BuildPaketDependencies =
    { Azure : bool }

    with override x.ToString () =
            if x.Azure then "azure" else "noazure"

type ClientPaketDependencies =
    { Communication : Communication option
      Fulma : bool
      Streams : bool }

    with override x.ToString () =
            let communication =
                x.Communication
                |> Option.map (function
                    | Remoting -> "remoting"
                    | Bridge -> "bridge")
                |> Option.defaultValue "nocommunication"
            let fulma = if x.Fulma then "fulma" else "nofulma"
            let streams = if x.Streams then "streams" else "nostreams"
            sprintf "%s-%s-%s" communication fulma streams

type ServerPaketDependencies =
    { Communication : Communication option
      Azure : bool }

    with override x.ToString () =
            let communication =
                x.Communication
                |> Option.map (function
                    | Remoting -> "remoting"
                    | Bridge -> "bridge")
                |> Option.defaultValue "nocommunication"
            let azure = if x.Azure then "azure" else "noazure"
            sprintf "%s-%s" communication azure

Target.create "Tests" (fun _ ->
    let cmd = "run"
    let args = "--project tests/tests.fsproj"
    let result = DotNet.exec (fun x -> { x with DotNetCliPath = "dotnet" }) cmd args
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

Target.runOrDefaultWithArguments "Install"
