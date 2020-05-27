module Client

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Shared
open Thoth.Fetch

type Model = Counter option

type Msg =
    | Increment
    | Decrement
    | InitialCountLoaded of Counter

let initialCounter() = Fetch.fetchAs<_, Counter> "/api/init"

let init() =
    let initialModel = None
    let loadCountCmd = Cmd.OfPromise.perform initialCounter () InitialCountLoaded
    initialModel, loadCountCmd

let update msg model =
    match msg, model with
    | Increment, Some counter ->
        let nextModel = Some { Value = counter.Value + 1 }
        nextModel, Cmd.none
    | Decrement, Some counter ->
        let nextModel = Some { counter with Value = counter.Value - 1 }
        nextModel, Cmd.none
    | InitialCountLoaded initialCount, _ ->
        let nextModel = Some initialCount
        nextModel, Cmd.none
    | _ ->
        model, Cmd.none

let show =
    function
    | Some counter -> string counter.Value
    | None -> "Loading..."

let view model dispatch =
    div [ Style [ TextAlign TextAlignOptions.Center; Padding 40 ] ] [
        img [ Src "favicon.png" ]
        h1 [] [ str "SAFE Template" ]
        h2 [] [ str (show model) ]
        button [ Style [ Margin 5; Padding 10 ]; OnClick(fun _ -> dispatch Decrement) ] [
            str "-"
        ]
        button [ Style [ Margin 5; Padding 10 ]; OnClick(fun _ -> dispatch Increment) ] [
            str "+"
        ]
    ]

//-:cnd:noEmit
#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

//+:cnd:noEmit
Program.mkProgram init update view
//-:cnd:noEmit
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
//+:cnd:noEmit
|> Program.run