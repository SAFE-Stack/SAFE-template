module Tests.Main

#if FABLE_COMPILER
open Fable.Mocha
let all = testList "All" [ Tests.Shared.sharedTests; Client.Tests.clientTests ]
[<EntryPoint>] let main args = Mocha.runTests all
#else
open Expecto
let all = testList "All" [ Tests.Shared.sharedTests; Server.Tests.serverTests ]
[<EntryPoint>] let main args = runTests defaultConfig all
#endif