#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.ReleaseNotesHelper

let nupkgDir = FullName "./nupkg"

let release = LoadReleaseNotes "RELEASE_NOTES.md"

let formattedRN =
  release.Notes
  |> List.map (sprintf "* %s")
  |> String.concat "\n"

Target "Clean" (fun () ->
  CleanDirs [
    nupkgDir
  ]
)

Target "Pack" (fun () ->
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
)

"Clean"
  ==> "Pack"
  ==> "Push"

RunTargetOrDefault "Pack"