module Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let allTests =
    testList "All"
        [
            Shared.Tests.sharedTests
#if FABLE_COMPILER
            Client.Tests.clientTests
#else
            Server.Tests.serverTests
#endif
        ]

[<EntryPoint>]
let main (args: string[]) =
#if FABLE_COMPILER
    Mocha.runTests allTests
#else
    let config = { defaultConfig with verbosity = Logging.LogLevel.Debug }
    runTests config allTests
#endif