#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.ReleaseNotesHelper

let templatePath = "./Content/.template.config/template.json"
let templateName = "SAFE-Stack Web App"
let nupkgDir = FullName "./nupkg"

let release = LoadReleaseNotes "RELEASE_NOTES.md"

let formattedRN =
    release.Notes
    |> List.map (sprintf "* %s")
    |> String.concat "\n"

Target "Clean" (fun () ->
    CleanDirs [ nupkgDir ]
    Git.CommandHelper.directRunGitCommandAndFail "./Content" "clean -fxd"
)

Target "Pack" (fun () ->
    RegexReplaceInFileWithEncoding
        "  \"name\": .+,"
       ("  \"name\": \"" + templateName + " v" + release.NugetVersion + "\",")
        System.Text.Encoding.UTF8
        templatePath
    DotNetCli.Pack ( fun args ->
        { args with
                OutputPath = nupkgDir
                AdditionalArgs =
                    [
                        sprintf "/p:PackageVersion=%s" release.NugetVersion
                        sprintf "/p:PackageReleaseNotes=\"%s\"" formattedRN
                    ]
        }
    )
)

Target "Push" (fun () ->
    Paket.Push ( fun args ->
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

    Git.Staging.StageAll ""
    Git.Commit.Commit "" commitMsg
    Git.Branches.pushBranch "" remoteGit "master"

    Git.Branches.tag "" tagName
    Git.Branches.pushTag "" remoteGit tagName
)

Target "Release" DoNothing

"Clean"
    ==> "Pack"
    ==> "Push"
    ==> "Release"

RunTargetOrDefault "Pack"