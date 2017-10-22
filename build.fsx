#r @"packages/FAKE/tools/FakeLib.dll"

open Fake

Target "Generate" DoNothing

RunTargetOrDefault "Generate"