module ExpectoTemplate
open Expecto

let config = defaultConfig

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly config argv