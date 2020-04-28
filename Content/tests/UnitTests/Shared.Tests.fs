module Tests.Shared

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let sharedTests = testList "Shared" [
    testCase "Add" <| fun _ ->
        Expect.equal (Shared.Calculator.add 1 1) 2 "Two is two"
    testCase "Add2" <| fun _ ->
        Expect.equal (Shared.Calculator.add 1 2) 3 "Two is two"
]