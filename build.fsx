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

Target.create "BuildWebPackConfig" (fun _ ->
    let srcDir = "paket-files/fable-compiler/webpack-config-template/webpack.config.js"
    let destDir = "Content/webpack.config.js"
    Shell.copyFile destDir srcDir

    let devServerProxy =
        """{
        // redirect requests that start with /api/* to the server on port 8085
        '/api/*': {
            target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
               changeOrigin: true
           }
       }"""

    let quote = sprintf "\"%s\""

    let replacements =
        [
            "indexHtmlTemplate", quote "./src/Client/index.html"
            "fsharpEntry", quote "./src/Client/Client.fsproj"
            "cssEntry", quote "./src/Client/style.sass"
            "outputDir", quote "./src/Client/deploy"
            "assetsDir", quote "./src/Client/public"
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
      Fulma : bool
      Reaction : bool }

    with override x.ToString () =
            let remoting = if x.Remoting then "remoting" else "noremoting"
            let fulma = if x.Fulma then "fulma" else "nofulma"
            let reaction = if x.Reaction then "reaction" else "noreaction"
            sprintf "%s-%s-%s" remoting fulma reaction

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

type CombinedPaketDependencies =
    { Azure : bool
      Remoting : bool
      Fulma : bool
      Server : ServerPaketDependency
      Reaction : bool }

    member x.ToBuild : BuildPaketDependencies =
            { Azure = x.Azure }

    member x.ToClient : ClientPaketDependencies =
        { Remoting = x.Remoting
          Fulma = x.Fulma
          Reaction = x.Reaction }

    member x.ToServer : ServerPaketDependencies =
        { Server = x.Server
          Remoting = x.Remoting
          Azure = x.Azure }

    override x.ToString () =
        let remoting = if x.Remoting then Some "--communication remoting" else None
        let azure = if x.Azure then Some "--deploy azure" else None
        let fulma = if not x.Fulma then Some "--layout none" else None
        let server = if x.Server <> Saturn then Some (sprintf "--server %O" x.Server) else None
        let reaction = if x.Reaction then Some "--pattern reaction" else None

        [ remoting
          azure
          fulma
          server
          reaction ]
        |> List.choose id
        |> String.concat " "

let configs =
    [ for azure in [ false; true ] do
      for fulma in [ false; true ] do
      for remoting in [ false; true ] do
      for server in [ Saturn; Giraffe; Suave ] do
      for reaction in [ false; true ] do
      yield
          { Azure = azure
            Fulma = fulma
            Server = server
            Remoting = remoting
            Reaction = reaction }
    ]

let specific f =
    List.map (fun x -> f x, string x)
    >> List.groupBy fst
    >> List.map (snd >> List.head)

let specificConfigs =
    [ "Build", configs |> specific (fun x -> string x.ToBuild)
      "Client", configs |> specific (fun x -> string x.ToClient)
      "Server", configs |> specific (fun x -> string x.ToServer) ]
    |> Map.ofList

let fullLockFileName build client server =
    sprintf "paket_%O_%O_%O.lock" build client server

let runPaket args wd =
    if Environment.isUnix then
        run "mono" (".paket/paket.exe " + args) wd
    else
        run ".paket/paket.exe" args wd

Target.create "BuildPaketLockFiles" (fun _ ->
    for config in configs do
        let contents =
            [
                "Content" </> "src" </> "Build" </> sprintf "paket_%O.lock" config.ToBuild
                "Content" </> "src" </> "Client" </> sprintf "paket_%O.lock" config.ToClient
                "Content" </> "src" </> "Server" </> sprintf "paket_%O.lock" config.ToServer
            ]
            |> List.map File.read
            |> Seq.concat

        let lockFileName = fullLockFileName config.ToBuild config.ToClient config.ToServer

        File.writeWithEncoding (Text.UTF8Encoding(false)) false ("Content" </> lockFileName) contents
)

Target.create "RemovePaketLockFiles" (fun _ ->
    for config in configs do
        let lockFileName = fullLockFileName config.ToBuild config.ToClient config.ToServer
        File.delete ("Content" </> lockFileName)
)

Target.create "GenJsonConditions" (fun _ ->
    for config in configs do
        let lockFileName = fullLockFileName config.ToBuild config.ToClient config.ToServer
        let server = string config.Server
        let azureOperator = if config.Azure then "==" else "!="
        let remoting = config.Remoting
        let layoutOperator = if config.Fulma then "!=" else "=="
        let reaction = config.Reaction
        let template =
            sprintf """                    {
                        "include": "%s",
                        "condition": "(server == \"%s\" && remoting == %b && deploy %s \"azure\" && layout %s \"none\" && reaction == %b)",
                        "rename": { "%s": "paket.lock" }
                    },"""
                 lockFileName
                 server
                 remoting
                 azureOperator
                 layoutOperator
                 reaction
                 lockFileName

        printfn "%s" template
)

Target.create "GenPaketLockFiles" (fun _ ->
    let baseDir = "gen-paket-lock-files"
    Directory.delete baseDir
    Directory.create baseDir

    for config in configs do
        let dirName = baseDir </> "tmp"
        Directory.delete dirName
        Directory.create dirName
        let arg = string config

        run "dotnet" (sprintf "new SAFE %s" arg) dirName

        let lockFile = dirName </> "paket.lock"

        if not (File.exists lockFile) then
            printfn "'paket.lock' doesn't exist for args '%s', installing..." arg
            runPaket "install" dirName

        let lines = File.readAsString lockFile
        Directory.delete dirName
        Directory.create dirName
        let delimeter = "GROUP "
        let groups =
            lines
            |> String.splitStr delimeter
            |> List.filter (String.isNullOrWhiteSpace >> not)
            |> List.map (fun group -> group.Substring(0, group.IndexOf Environment.NewLine), delimeter + group)
        for (groupName, group) in groups do
            let dirName = baseDir </> groupName
            Directory.create dirName
            let lockFileSuffix =
                match groupName with
                | "Build" -> string config.ToBuild
                | "Client" -> string config.ToClient
                | "Server" -> string config.ToServer
                | _ -> failwithf "Unhandled name '%s'" groupName
            let fileName = sprintf "paket_%s.lock" lockFileSuffix
            let filePath = dirName </> fileName
            if not (File.exists filePath) then
                File.writeString false filePath group
                Shell.copyFile ("Content" </> "src" </> groupName </> fileName) (dirName </> fileName)
            else
                printfn "'%s' already exists" filePath
)

Target.create "UpdatePaketLockFiles" (fun x ->
    let baseDir = "gen-paket-lock-files"
    Directory.delete baseDir
    Directory.create baseDir

    let groupNames =
        match x.Context.Arguments with
        | [ ] -> specificConfigs |> Seq.map (fun kv -> kv.Key) |> Seq.toList
        | xs -> xs

    for groupName in groupNames do
        let configs =
            match Map.tryFind groupName specificConfigs with
            | Some x -> x |> List.indexed
            | None -> failwithf "unknown group: '%s'" groupName

        printfn "Group name: %s, all configs: %A" groupName configs

        for (index, (configAbbr, safeArgs)) in configs do
            let dirName = baseDir </> "tmp"
            Directory.delete dirName
            Directory.create dirName
            printfn "Updating lock file %d of %d" (index + 1) configs.Length
            run "dotnet" (sprintf "new SAFE %s" safeArgs) dirName

            let lockFile = dirName </> "paket.lock"

            if not (File.exists lockFile) then
                failwithf "'paket.lock' doesn't exist for args '%s'" safeArgs

            runPaket (sprintf "update -g %s" groupName) dirName

            let lines = File.readAsString lockFile
            Directory.delete dirName
            Directory.create dirName
            let delimeter = "GROUP "
            let (groupName, group) =
                lines
                |> String.splitStr delimeter
                |> List.filter (String.isNullOrWhiteSpace >> not)
                |> List.map (fun group -> group.Substring(0, group.IndexOf Environment.NewLine), delimeter + group)
                |> List.filter (fst >> ((=) groupName))
                |> List.head
            let dirName = baseDir </> groupName
            Directory.create dirName
            let fileName = sprintf "paket_%s.lock" configAbbr
            let filePath = dirName </> fileName
            if not (File.exists filePath) then
                File.writeString false filePath group
                Shell.copyFile ("Content" </> "src" </> groupName </> fileName) (dirName </> fileName)
            else
                printfn "'%s' already exists" filePath
)

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
    ==> "BuildWebPackConfig"
    ==> "BuildPaketLockFiles"
    ==> "Pack"
    ==> "RemovePaketLockFiles"
    ==> "Install"
    ==> "Tests"
    ==> "Push"
    ==> "Release"

"Install"
    ==> "GenPaketLockFiles"

"Install"
    ==> "UpdatePaketLockFiles"

Target.runOrDefaultWithArguments "Install"