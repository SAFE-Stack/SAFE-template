module App

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch

open Shared

#if (Fulma != "none")
open Fulma
open Fulma.Layouts
open Fulma.Elements
open Fulma.Components
open Fulma.BulmaClasses
#endif

#if (Fulma == "landing")
open Fulma.BulmaClasses.Bulma
open Fulma.BulmaClasses.Bulma.Properties
open Fulma.Extra.FontAwesome
#endif

type Model = Counter option

type Msg =
| Increment
| Decrement
| Init of Result<Counter, exn>


#if (Remoting)
module Server = 

  open Shared
  open Fable.Remoting.Client
  
  /// A proxy you can use to talk to server directly
  let api : ICounterProtocol = 
    Proxy.createWithBuilder<ICounterProtocol> Route.builder
    
#endif

let init () = 
  let model = None
  let cmd =
#if Remoting
    Cmd.ofAsync 
      Server.api.getInitCounter
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
#if (Server == "suave")
      "Suave.IO", "http://suave.io"
#else
      "Giraffe", "https://github.com/giraffe-fsharp/Giraffe"
#endif
      "Fable", "http://fable.io"
      "Elmish", "https://fable-elmish.github.io/"
#if (Fulma != "none")
      "Fulma", "https://mangelmaxime.github.io/Fulma" 
#endif
#if (Fulma == "landing")
      "Bulma\u00A0Templates", "https://dansup.github.io/bulma-templates/"
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

#if (Fulma == "none")
let view model dispatch =
  div []
    [ h1 [] [ str "SAFE Template" ]
      p  [] [ str "The initial counter is fetched from server" ]
      p  [] [ str "Press buttons to manipulate counter:" ]
      button [ OnClick (fun _ -> dispatch Decrement) ] [ str "-" ]
      div [] [ str (show model) ]
      button [ OnClick (fun _ -> dispatch Increment) ] [ str "+" ]
      safeComponents ]
#elseif (Fulma == "basic")
let button txt onClick = 
  Button.button_btn
    [ Button.isFullWidth
      Button.isPrimary
      Button.onClick onClick ] 
    [ str txt ]

let view model dispatch =
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
let navBrand =
  Navbar.brand_div [ ] 
    [ Navbar.item_a 
        [ Navbar.Item.props [ Href "https://safe-stack.github.io/" ]
          Navbar.Item.isActive ] 
        [ img [ Src "https://safe-stack.github.io/images/safe_top.png"
                Alt "Logo" ] ] 
      Navbar.burger [ ] 
        [ span [ ] [ ]
          span [ ] [ ]
          span [ ] [ ] ] ]

let navMenu =
  Navbar.menu [ ]
    [ Navbar.end_div [ ] 
        [ Navbar.item_a [ ] 
            [ str "Home" ] 
          Navbar.item_a [ ]
            [ str "Examples" ]
          Navbar.item_a [ ]
            [ str "Documentation" ]
          Navbar.item_div [ ]
            [ Button.button_a 
                [ Button.isWhite
                  Button.isOutlined
                  Button.isSmall
                  Button.props [ Href "https://github.com/SAFE-Stack/SAFE-template" ] ] 
                [ Icon.faIcon [ ] 
                    [ Fa.icon Fa.I.Github; Fa.fw ]
                  span [ ] [ str "View Source" ] ] ] ] ]

let containerBox model dispatch =
  Box.box' [ ]
    [ Form.Field.field_div [ Form.Field.isGrouped ] 
        [ Form.Control.control_p [ Form.Control.customClass "is-expanded"] 
            [ Form.Input.input
                [ Form.Input.typeIsNumber
                  Form.Input.disabled true
                  Form.Input.value (show model) ] ]
          Form.Control.control_p [ ]
            [ Button.button_a 
                [ Button.isPrimary
                  Button.onClick (fun _ -> dispatch Increment) ]
                [ str "+" ] ]
          Form.Control.control_p [ ]
            [ Button.button_a 
                [ Button.isPrimary
                  Button.onClick (fun _ -> dispatch Decrement) ]
                [ str "-" ] ] ] ]

let view model dispatch =
  Hero.hero [ Hero.isPrimary; Hero.isFullHeight ] 
    [ Hero.head [ ] 
        [ Navbar.navbar [ ]
            [ Container.container [ ]
                [ navBrand
                  navMenu ] ] ]
      
      Hero.body [ ] 
        [ Container.container [ Container.customClass Alignment.HasTextCentered ]
            [ Column.column 
                [ Column.Width.is6
                  Column.Offset.is3 ]
                [ h1 [ ClassName "title" ] 
                    [ str "SAFE Template" ]
                  div [ ClassName "subtitle" ]
                    [ safeComponents ]
                  containerBox model dispatch ] ] ] ]
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
