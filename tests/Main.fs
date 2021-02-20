module ExpectoTemplate

open SAFE.Tests

[<EntryPoint>]
let main argv =
    try
        for build in [ Normal; Minimal ] do
            testTemplateBuild build
        0
    with e ->
        printfn "%O" e
        1
