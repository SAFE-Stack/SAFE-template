#r @"packages/FAKE/tools/FakeLib.dll"

open Fake

let nupkgDir = "./nupkg"

Target "Clean" DoNothing

Target "Pack" (fun () ->
  DotNetCli.Pack ( fun args ->
    args
  )
)

"Clean"
  ==> "Pack"

RunTargetOrDefault "Pack"