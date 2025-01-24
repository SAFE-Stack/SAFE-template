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
let defaultTemplatePath = "./Content/default"
let templateProj = "SAFE.Template.proj"
let templateName = "SAFE-Stack Web App"
let version = Environment.environVarOrDefault "VERSION" ""
let nupkgDir = Path.getFullName "./nupkg"
let nupkgPath = System.IO.Path.Combine(nupkgDir, $"SAFE.Template.%s{version}.nupkg")

let result = DotNet.exec (fun x -> { x with DotNetCliPath = "dotnet" }) "tool" "restore"

if not result.OK then failwithf "`dotnet %s %s` failed" "tool" "restore"

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
        ("  \"name\": \"" + templateName + " v" + version + "\",")
        Text.Encoding.UTF8
        templatePath
    let releaseNotesUrl = Environment.environVarOrDefault "RELEASE_NOTES_URL" ""

    DotNet.pack
        (fun args ->
            { args with
                    Configuration = DotNet.BuildConfiguration.Release
                    OutputPath = Some nupkgDir
                    MSBuildParams = msBuildParams args.MSBuildParams
                    Common =
                        { args.Common with
                            CustomParams =
                                Some (sprintf "/p:PackageVersion=%s /p:PackageReleaseNotes=\"%s\""
                                        version
                                        releaseNotesUrl) }
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

Target.create "TemplateExecutionTests" (fun _ ->
    let cmd = "run"
    let args = "--project tests/Tests.fsproj"
    let result = DotNet.exec (fun x -> { x with DotNetCliPath = "dotnet" }) cmd args
    if not result.OK then failwithf "`dotnet %s %s` failed" cmd args
)

Target.create "DefaultTemplateTests" (fun _ ->
    let cmd = "run"
    let args = "RunTestsHeadless --project Build.fsproj"
    let result = DotNet.exec (fun x -> { x with DotNetCliPath = "dotnet"; WorkingDirectory = defaultTemplatePath}) cmd args
    if not result.OK then failwithf "`dotnet %s %s` failed" cmd args
)

Target.create "Release" (fun _ ->
    let nugetApiKey = Environment.environVarOrFail "NUGET_API_KEY"
    let nugetArgs = $"""push "*.nupkg" --api-key {nugetApiKey} --source https://api.nuget.org/v3/index.json"""
    let result = DotNet.exec (fun x -> { x with DotNetCliPath = "dotnet"; WorkingDirectory = nupkgDir }) "nuget" nugetArgs
    if not result.OK then failwithf "`dotnet %s` failed" "nuget push")

Target.create "UpdateReleaseNotes" (fun _ ->
    let file = "RELEASE_NOTES.md"
    let version = Environment.environVarOrFail "VERSION"
    let body = Environment.environVarOrFail "RELEASE_NOTES_BODY"

    let existingContent = System.IO.File.ReadAllText file
    let releaseNote =
        $"""#### {version} - {DateTime.Now.ToString("dd.MM.yy")}

{body}

{existingContent}
        """

    System.IO.File.WriteAllText("RELEASE_NOTES.md", releaseNote)
    )

open Fake.Core.TargetOperators

"Clean"
    =?> ("TemplateExecutionTests", not skipTests)
    =?> ("DefaultTemplateTests", not skipTests)
    ==> "Pack"
    ==> "Install"
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
