module Client.Tests

open Fable.Mocha

let clientTests = testList "Client" [
    testCase "Counter is none" <| fun _ ->
        let model, msg = Client.init ()
        Expect.equal true model.Counter.IsSome "Counter is some"
]