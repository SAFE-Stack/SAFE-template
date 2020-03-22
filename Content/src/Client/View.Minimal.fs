module View

open Fable.React
open Fable.React.Props

open State

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

    footer [ Style [ Margin 30 ] ]
        [ str "Version "
          strong [] [ str Version.app ]
          str " powered by: "
          components ]

let show =
    function
    | { Counter = Some counter } -> string counter.Value
    | { Counter = None } -> "Loading..."

let view (model: Model) (dispatch: Msg -> unit) =
    div
        [ Style
            [ TextAlign TextAlignOptions.Center
              Padding 40 ] ]
        [ img [ Src "favicon.png" ]
          h1 [] [ str "SAFE Template" ]
          h2 [] [ str (show model) ]
          button
              [ Style
                  [ Margin 5
                    Padding 10 ]
                OnClick(fun _ -> dispatch Decrement) ] [ str "-" ]
          button
              [ Style
                  [ Margin 5
                    Padding 10 ]
                OnClick(fun _ -> dispatch Increment) ] [ str "+" ]
          safeComponents ]
