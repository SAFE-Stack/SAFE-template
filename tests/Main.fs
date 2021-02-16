module ExpectoTemplate

open Expecto

let config =
    { defaultConfig with
          // Disabling parallel run to avoid port conflicts
          runInParallel = false }

[<EntryPoint>]
let main argv =
    Tests.runTests config SAFE.Tests.tests
