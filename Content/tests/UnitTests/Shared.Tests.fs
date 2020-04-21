module Shared.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let sharedTests = testList "Shared tests" [
    testCase "Ice breaker" <| fun _ ->
        Expect.equal 2 2 "Two is two"
]