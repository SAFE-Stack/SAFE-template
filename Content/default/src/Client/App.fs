module App

open Elmish
open Elmish.React

open Fable.Core.JsInterop

importSideEffects "./index.css"

//-:cnd:noEmit
#if DEBUG
open Elmish.HMR
#endif

//+:cnd:noEmit
Program.mkProgram Index.init Index.update Index.view
//-:cnd:noEmit
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "elmish-app"

//+:cnd:noEmit
|> Program.run