module ExpectoTemplate
open Expecto

let config =
  { defaultConfig with parallel = false }

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly config argv