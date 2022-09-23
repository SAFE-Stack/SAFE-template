module ExpectoTemplate

open Expecto
open SAFE.Tests

let tests = testList "All" [
    testTemplateBuild Normal
    testTemplateBuild Minimal
]

[<EntryPoint>]
let main args =
    let config =
        { defaultConfig with runInParallel = false }
    runTests config tests
