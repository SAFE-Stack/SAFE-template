module Bridge

open Shared
open Elmish
/// Elmish.Bridge model for keeping the server-side state
type Model = { SendTime : bool }

/// The server messages that Elmish can receive
type Msg =
    | Tick
    | Remote of ServerMsg

/// Elmish init function with a channel for sending client messages
/// Returns a new state and commands
let init clientDispatch () =
    clientDispatch (GetTime System.DateTime.Now)
    { SendTime = true }, Cmd.none

/// Elmish update function with a channel for sending client messages
/// Returns a new state and commands
let update clientDispatch msg model =
    match msg with
    | Tick ->
        if model.SendTime then
            clientDispatch (GetTime System.DateTime.Now)
        model, Cmd.none
    | Remote Start ->
        { model with SendTime = true }, Cmd.none
    | Remote Pause ->
        { model with SendTime = false }, Cmd.none

/// Elmish subscription for sending a tick every second
let timer _ =
    let sub dispatch =
        async {
            while true do
                do! Async.Sleep 1000
                dispatch Tick
        } |> Async.Start
    Cmd.ofSub sub
