module SAFE.App.Shared.Tests

//-:cnd:noEmit
#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif
//+:cnd:noEmit

open SAFE.App.Shared

let shared = testList "Shared" [
    testCase "Empty string is not a valid description" <| fun _ ->
        let expected = false
        let actual = Todo.isValid ""
        Expect.equal actual expected "Should be false"
]