module App

open Elmish
open Elmish.React

open Fable.Helpers.React.Props
module R = Fable.Helpers.React
open Fable.PowerPack.Fetch

type Model = int option

type Msg = Increment | Decrement | Init of Result<int, exn>

let init () = 
  let model = None
  let cmd = 
    Cmd.ofPromise 
      (fetchAs<int> "/api/init") 
      [] 
      (Ok >> Init) 
      (Error >> Init)
  model, cmd

let update msg (model : Model) =
  let model' =
    match model,  msg with
    | Some x, Increment -> Some (x + 1)
    | Some x, Decrement -> Some (x - 1)
    | None, Init (Ok x) -> Some x
    | _ -> None
  model', Cmd.none

let view model dispatch =
  R.div []
      [ R.h1 [] [ R.str "SAFE Template" ]
        R.p  [] [ R.str "The initial counter is fetched from server" ]
        R.p  [] [ R.str "Press buttons to manipulate counter:" ]
        R.button [ OnClick (fun _ -> dispatch Decrement) ] [ R.str "-" ]
        R.div [] [ R.str (match model with 
                          | Some x -> string x 
                          | None -> "Loading...") ]
        R.button [ OnClick (fun _ -> dispatch Increment) ] [ R.str "+" ] ]

//-:cnd:noEmit
#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
//+:cnd:noEmit
|> Program.run
