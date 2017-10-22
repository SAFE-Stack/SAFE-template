#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.ReleaseNotesHelper

let nupkgDir = FullName "./nupkg"

let release = LoadReleaseNotes "RELEASE_NOTES.md"

Target "Clean" (fun () ->
  CleanDirs [
    nupkgDir
  ]
)

Target "Pack" (fun () ->
  let rn = release.Notes |> String.concat "\n"
  DotNetCli.Pack ( fun args ->
    { args with
        OutputPath = nupkgDir
        AdditionalArgs =
          [
            sprintf "/p:PackageVersion=%s" release.NugetVersion
            sprintf "/p:PackageReleaseNotes=\"%s\"" rn
          ]
    }
  )
)

"Clean"
  ==> "Pack"

RunTargetOrDefault "Pack"