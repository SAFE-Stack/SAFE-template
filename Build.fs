open System

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.Tools

let execContext = Context.FakeExecutionContext.Create false "build.fsx" [ ]
Context.setExecutionContext (Context.RuntimeContext.Fake execContext)

let skipTests = Environment.hasEnvironVar "yolo"
let release = ReleaseNotes.load "RELEASE_NOTES.md"

let templatePath = "./Content/.template.config/template.json"
let templateProj = "SAFE.Template.proj"
let templateName = "SAFE-Stack Web App"
let nupkgDir = Path.getFullName "./nupkg"
let nupkgPath = System.IO.Path.Combine(nupkgDir, $"SAFE.Template.%s{release.NugetVersion}.nupkg")

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
    let unInstallArgs = $"uninstall SAFE.Template"
    DotNet.exec (fun x -> { x with DotNetCliPath = "dotnet" }) "new" unInstallArgs
    |> ignore // Allow this to fail as the template might not be installed

    let installArgs = $"install \"%s{nupkgPath}\""
    DotNet.exec (fun x -> { x with DotNetCliPath = "dotnet" }) "new" installArgs
    |> fun result -> if not result.OK then failwith $"`dotnet new %s{installArgs}` failed with %O{result}"
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
    let args = "--project tests/Tests.fsproj"
    let result = DotNet.exec (fun x -> { x with DotNetCliPath = "dotnet" }) cmd args
    if not result.OK then failwithf "`dotnet %s %s` failed" cmd args
)

Target.create "Push" (fun _ ->
    let args = $"push %s{nupkgPath} --url https://www.nuget.org"
    run "dotnet" $"paket push %s{nupkgPath} --url https://www.nuget.org" "."

    let commitMsg = sprintf "Bumping version to %O" release.NugetVersion
    let tagName = string release.NugetVersion

    Git.Branches.checkout "" false "master"
    Git.CommandHelper.directRunGitCommand "" "fetch origin" |> ignore
    Git.CommandHelper.directRunGitCommand "" "fetch origin --tags" |> ignore

    Git.Staging.stageAll ""
    Git.Commit.exec "" commitMsg
    Git.Branches.pushBranch "" "origin" "master"

    Git.Branches.tag "" tagName
    Git.Branches.pushTag "" "origin" tagName
)

Target.create "Release" ignore

open Fake.Core.TargetOperators

"Clean"
    ==> "Pack"
    ==> "Install"
    =?> ("Tests", not skipTests)
    ==> "Push"
    ==> "Release"
|> ignore

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
