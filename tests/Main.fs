module ExpectoTemplate
open Expecto

#nowarn "46" // The identifier 'parallel' is reserved for future use by F#

let config =
    { defaultConfig with
        parallel = false }

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly config argv