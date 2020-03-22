module View

open Elmish.React
open Fable.React
open Fable.React.Props
open Fetch.Types

let view (model: Model) (dispatch: Msg -> unit) =
    div
        [ Style
            [ TextAlign TextAlignOptions.Center
              Padding 40 ] ]
        [ img [ Src "favicon.png" ]
          h1 [] [ str "SAFE Template" ] ]