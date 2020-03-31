module Tests

open Fable.Mocha

let clientTests = testList "App tests" [
    testCase "Increment and Decrement work" <| fun _ ->
        Expect.equal 1 1 "One is one"
]

[<EntryPoint>]
let main (args: string[]) = Mocha.runTests clientTests