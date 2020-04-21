module Tests

#if FABLE_COMPILER
open Fable.Mocha
let all = testList "All" [ Shared.Tests.sharedTests; Client.Tests.clientTests ]
[<EntryPoint>] let main args = Mocha.runTests all
#else
open Expecto
let all = testList "All" [ Shared.Tests.sharedTests; Server.Tests.serverTests ]
let config = { defaultConfig with verbosity = Logging.LogLevel.Debug }
[<EntryPoint>] let main args = runTests config all
#endif