module ExpectoTemplate

open Expecto

let config =
    let writeResults = TestResults.writeNUnitSummary ("TestResults.xml", "SAFE Tests")
    { defaultConfig.appendSummaryHandler writeResults with
          // Disabling parallel run due to yarn concurrency issue when
          // installing packages in parallel
          // https://github.com/yarnpkg/yarn/issues/2629
          ``parallel`` = false }

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly config argv
