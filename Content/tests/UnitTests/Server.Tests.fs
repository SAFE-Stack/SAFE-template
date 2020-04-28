module Server.Tests

open Expecto

let serverTests = testList "Server" [
    testCase "one is one" <| fun () ->
        Expect.equal (Server.serverAdd 1 2) 3 "One is one"
]