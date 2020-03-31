module Server.Tests

open Expecto

let serverTests = testList "Server" [
    test "one is one" {
        Expect.equal 1 1 "One is one"
    }
]

[<EntryPoint>]
let main argv = runTests defaultConfig serverTests