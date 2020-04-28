module Client.Tests

open Fable.Mocha

let client = testList "Client" [
    testCase "Initially counter is some" <| fun _ ->
        let model, _ = Client.init ()
        Expect.equal true model.Counter.IsSome "Counter is some"
]

let all =
    testList "All"
        [
#if FABLE_COMPILER // The preprocessor directive makes editor happy
            Shared.Tests.shared
#endif
            client
        ]

[<EntryPoint>]
let main _ = Mocha.runTests all