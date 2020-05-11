module Shared.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open Shared

let shared = testList "Shared" [
    testCase "Initial counter value is 42" <| fun _ ->
        let expected = 42
        let actual = Counter.initial.Value
        Expect.equal actual expected "Should be 42"
]