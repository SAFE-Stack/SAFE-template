module ExpectoTemplate

open Expecto

let config =
    let writeResults = TestResults.writeNUnitSummary ("TestResults.xml", "SAFE Tests")
    { defaultConfig.appendSummaryHandler writeResults with
          // Disabling parallel run to avoid port conflicts
          ``parallel`` = false }

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly config argv
