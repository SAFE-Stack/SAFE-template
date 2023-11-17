open System

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.Tools

let execContext = Context.FakeExecutionContext.Create false "build.fsx" [ ]
Context.setExecutionContext (Context.RuntimeContext.Fake execContext)

let templatePath = "./Content/.template.config/template.json"
let templateProj = "SAFE.Template.proj"
let templateName = "SAFE-Stack Web App"
let nupkgDir = Path.getFullName "./nupkg"

let skipTests = Environment.hasEnvironVar "yolo"
let release = ReleaseNotes.load "RELEASE_NOTES.md"

let formattedRN =
    release.Notes
    |> List.map (sprintf "* %s")
    |> String.concat "\n"

Target.create "Clean" (fun _ ->
    Shell.cleanDirs [ nupkgDir ]
)

let msBuildParams msBuildParameter: MSBuild.CliArguments = { msBuildParameter with DisableInternalBinLog = true }

Target.create "Pack" (fun _ ->
    Shell.regexReplaceInFileWithEncoding
        "  \"name\": .+,"
       ("  \"name\": \"" + templateName + " v" + release.NugetVersion + "\",")
        Text.Encoding.UTF8
        templatePath
    DotNet.pack
        (fun args ->
            { args with
                    OutputPath = Some nupkgDir
                    MSBuildParams = msBuildParams args.MSBuildParams
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
      sprintf "install \"%s\"" fullPathToNupkg
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
    =?> ("Tests", not skipTests)
    ==> "Push"
    ==> "Release"

[<EntryPoint>]
let main args =
    try
        match args with
        | [| target |] -> Target.runOrDefault target
        | _ -> Target.runOrDefault "Install"
        0
    with e ->
        printfn "%A" e
        1