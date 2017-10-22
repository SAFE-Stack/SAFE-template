module App

open Elmish
open Elmish.React

open Fable.Helpers.React

type Model =
  { X : int }

type Msg =
| Increment
| Decrement

let init () =
  { X = 0 }, Cmd.ofMsg Increment

let update msg model =
  match msg,model with
  | Increment, { X = x } when x >= 100 ->
    { model with X = x - 1 }, Cmd.ofMsg Decrement
  | Increment, _ ->
    { model with X = model.X + 1 }, Cmd.ofMsg Increment
  | Decrement, { X = x } when x <= 0 ->
    { model with X = x + 1 }, Cmd.ofMsg Increment
  | Decrement, _ ->
    { model with X = model.X - 1 }, Cmd.ofMsg Decrement

let view model dispath =
  div []
   [ h1 [] [ str (sprintf "Counter: %d" model.X) ] ]

Program.mkProgram init update view
|> Program.withReact "elmish-app"
|> Program.run