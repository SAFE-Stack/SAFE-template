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

(*//#if minimal
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
#else*)
open Fulma

let safeComponents =
    let components =
        span []
            [ a [ Href "https://github.com/SAFE-Stack/SAFE-template" ]
                  [ str "SAFE  "
                    str Version.template ]
              str ", "
              a [ Href "https://saturnframework.github.io" ] [ str "Saturn" ]
              str ", "
              a [ Href "http://fable.io" ] [ str "Fable" ]
              str ", "
              a [ Href "https://elmish.github.io" ] [ str "Elmish" ] ]

    footer [ ]
        [ str "Version "
          strong [] [ str Version.app ]
          str " powered by: "
          components ]

let navBrand =
    Navbar.Brand.div [ ]
        [ Navbar.Item.a
            [ Navbar.Item.Props [ Href "https://safe-stack.github.io/" ]
              Navbar.Item.IsActive true ]
            [ img [ Src "https://safe-stack.github.io/images/safe_top.png"
                    Alt "Logo" ] ] ]

let containerBox (model : Model) (dispatch : Msg -> unit) =
    Box.box' [ ]
        [ Field.div [ Field.IsGrouped ]
            [ Control.p [ Control.IsExpanded ]
                [ Input.text
                    [ Input.Disabled true
                      Input.Value (show model) ] ]
              Control.p [ ]
                [ Button.a
                    [ Button.Color IsPrimary
                      Button.OnClick (fun _ -> dispatch Increment) ]
                    [ str "+" ] ]
              Control.p [ ]
                [ Button.a
                    [ Button.Color IsPrimary
                      Button.OnClick (fun _ -> dispatch Decrement) ]
                    [ str "-" ] ] ] ]

let view (model : Model) (dispatch : Msg -> unit) =
    Hero.hero
        [ Hero.Color IsPrimary
          Hero.IsFullHeight
          Hero.Props
            [ Style
                [ Background """linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url("https://unsplash.it/1200/900?random") no-repeat center center fixed"""
                  BackgroundSize "cover" ] ] ]
        [ Hero.head [ ]
            [ Navbar.navbar [ ]
                [ Container.container [ ]
                    [ navBrand ] ] ]

          Hero.body [ ]
            [ Container.container [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                [ Column.column
                    [ Column.Width (Screen.All, Column.Is6)
                      Column.Offset (Screen.All, Column.Is3) ]
                    [ Heading.p [ ]
                        [ str "SAFE Template" ]
                      Heading.p [ Heading.IsSubtitle ]
                        [ safeComponents ]
                      containerBox model dispatch ] ] ] ]
(*#endif*)

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
