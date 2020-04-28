module Server.Tests

open Expecto

let server = testList "Server" [
    testCase "one is one" <| fun _ ->
        Expect.equal (Server.serverAdd 1 2) 3 "One is one"
]

let all =
    testList "All"
        [
            Shared.Tests.shared
            server
        ]

[<EntryPoint>]
let main _ = runTests defaultConfig all