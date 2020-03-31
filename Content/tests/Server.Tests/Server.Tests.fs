module Server.Tests

open Expecto

let serverTests = testList "Server" [
    test "one is one" {
        Expect.equal 1 1 "One is one"
    }
]

let allTest = testList "All" [ Shared.Tests.sharedTests; serverTests ]

[<EntryPoint>]
let main argv = runTests defaultConfig serverTests