module App

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch

open Shared

#if (Fulma)
open Fulma
open Fulma.Layouts
open Fulma.Elements
open Fulma.Components
open Fulma.BulmaClasses
#endif

type Model = Counter option

type Msg =
| Increment
| Decrement
| Init of Result<Counter, exn>

let init () = 
  let model = None
  let cmd =
#if Remoting
    let routeBuilder typeName methodName = 
      sprintf "/api/%s/%s" typeName methodName
    let api = Fable.Remoting.Client.Proxy.createWithBuilder<Init> routeBuilder
    Cmd.ofAsync 
      api.getCounter
      () 
      (Ok >> Init)
      (Error >> Init)
#else
    Cmd.ofPromise 
      (fetchAs<int> "/api/init") 
      [] 
      (Ok >> Init) 
      (Error >> Init)
#endif
  model, cmd

let update msg (model : Model) =
  let model' =
    match model,  msg with
    | Some x, Increment -> Some (x + 1)
    | Some x, Decrement -> Some (x - 1)
    | None, Init (Ok x) -> Some x
    | _ -> None
  model', Cmd.none

let safeComponents =
  let intersperse sep ls =
    List.foldBack (fun x -> function
      | [] -> [x]
      | xs -> x::sep::xs) ls []

  let components =
    [ 
      "Suave.IO", "http://suave.io" 
      "Fable"   , "http://fable.io"
      "Elmish"  , "https://fable-elmish.github.io/"
#if (Fulma)
      "Fulma"   , "https://mangelmaxime.github.io/Fulma" 
#endif
#if (Remoting)
      "Fable.Remoting", "https://github.com/Zaid-Ajaj/Fable.Remoting"
#endif
    ]
    |> List.map (fun (desc,link) -> a [ Href link ] [ str desc ] )
    |> intersperse (str ", ")
    |> span [ ]

  p [ ]
    [ strong [] [ str "SAFE Template" ]
      str " powered by: "
      components ]

let show = function
| Some x -> string x
| None -> "Loading..."

#if (Fulma)
let button txt onClick = 
  Button.button_btn
    [ Button.isFullWidth
      Button.isPrimary
      Button.onClick onClick ] 
    [ str txt ]
#endif

let view model dispatch =
#if (Fulma)
  div []
    [ Navbar.navbar [ Navbar.customClass "is-primary" ]
        [ Navbar.item_div [ ]
            [ Heading.h2 [ ]
                [ str "SAFE Template" ] ] ]

      Container.container []
        [ Content.content [ Content.customClass Bulma.Level.Item.HasTextCentered ] 
            [ Heading.h3 [] [ str ("Press buttons to manipulate counter: " + show model) ] ]
          Columns.columns [] 
            [ Column.column [] [ button "-" (fun _ -> dispatch Decrement) ]
              Column.column [] [ button "+" (fun _ -> dispatch Increment) ] ] ]
    
      Footer.footer [ ]
        [ Content.content [ Content.customClass Bulma.Level.Item.HasTextCentered ]
            [ safeComponents ] ] ]
#else
  div []
    [ h1 [] [ str "SAFE Template" ]
      p  [] [ str "The initial counter is fetched from server" ]
      p  [] [ str "Press buttons to manipulate counter:" ]
      button [ OnClick (fun _ -> dispatch Decrement) ] [ str "-" ]
      div [] [ str (show model) ]
      button [ OnClick (fun _ -> dispatch Increment) ] [ str "+" ]
      safeComponents ]
#endif
  
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
