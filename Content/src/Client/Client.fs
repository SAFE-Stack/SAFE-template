module Client

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
open Fulma.Elements.Form
open Fulma.Components
open Fulma.BulmaClasses
#endif

#if (Fulma == "admin" || Fulma == "cover" || Fulma == "hero" || Fulma == "landing" || Fulma == "login")
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

let init () : Model * Cmd<Msg> =
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

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
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
#if     (Server == "suave")
      "Suave", "http://suave.io"
#elseif (Server == "giraffe")
      "Giraffe", "https://github.com/giraffe-fsharp/Giraffe"
#elseif (Server == "saturn")
      "Saturn", "https://saturnframework.github.io/docs/"
#endif
      "Fable", "http://fable.io"
      "Elmish", "https://fable-elmish.github.io/"
#if (Fulma != "none")
      "Fulma", "https://mangelmaxime.github.io/Fulma" 
#endif
#if (Fulma == "admin" || Fulma == "cover" || Fulma == "hero" || Fulma == "landing" || Fulma == "login")
      "Bulma\u00A0Templates", "https://dansup.github.io/bulma-templates/"
#endif
#if (Remoting)
      "Fable.Remoting", "https://zaid-ajaj.github.io/Fable.Remoting/"
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
let view (model : Model) (dispatch : Msg -> unit) =
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
  Button.button
    [ Button.IsFullwidth
      Button.Color IsPrimary
      Button.OnClick onClick ] 
    [ str txt ]

let view (model : Model) (dispatch : Msg -> unit) =
  div []
    [ Navbar.navbar [ Navbar.Color IsPrimary ]
        [ Navbar.Item.div [ ]
            [ Heading.h2 [ ]
                [ str "SAFE Template" ] ] ]

      Container.container []
        [ Content.content [ Content.CustomClass Bulma.Properties.Alignment.HasTextCentered ] 
            [ Heading.h3 [] [ str ("Press buttons to manipulate counter: " + show model) ] ]
          Columns.columns [] 
            [ Column.column [] [ button "-" (fun _ -> dispatch Decrement) ]
              Column.column [] [ button "+" (fun _ -> dispatch Increment) ] ] ]
    
      Footer.footer [ ]
        [ Content.content [ Content.CustomClass Bulma.Properties.Alignment.HasTextCentered ]
            [ safeComponents ] ] ]
#elseif (Fulma == "admin")
let navBrand =
  Navbar.navbar [ Navbar.Color IsWhite ]
    [ Container.container [ ]
        [ Navbar.Brand.div [ ]
            [ Navbar.Item.a [ Navbar.Item.CustomClass "brand-text" ]
                [ str "SAFE Admin" ]
              Navbar.burger [ ] 
                [ span [ ] [ ]
                  span [ ] [ ]
                  span [ ] [ ] ] ]
          Navbar.menu [ ]
            [ Navbar.Start.div [ ]
                [ Navbar.Item.a [ ]
                    [ str "Home" ]
                  Navbar.Item.a [ ]
                    [ str "Orders" ]
                  Navbar.Item.a [ ]
                    [ str "Payments" ]
                  Navbar.Item.a [ ]
                    [ str "Exceptions" ] ] ] ] ]

let menu =
  Menu.menu [ ]
    [ Menu.label [ ]
        [ str "General" ]
      Menu.list [ ]
        [ Menu.item [ ]
            [ str "Dashboard" ]
          Menu.item [ ]
            [ str "Customers" ] ]
      Menu.label [ ]
        [ str "Administration" ]
      Menu.list [ ]
        [ Menu.item [ ]
            [ str "Team Settings" ]
          li [ ]
            [ a [ ]
                [ str "Manage Your Team" ]
              Menu.list [ ]
                [ Menu.item [ ]
                    [ str "Members" ]
                  Menu.item [ ]
                    [ str "Plugins" ]
                  Menu.item [ ]
                    [ str "Add a member" ] ] ]
          Menu.item [ ]
            [ str "Invitations" ]
          Menu.item [ ]
            [ str "Cloud Storage Environment Settings" ]
          Menu.item [ ]
            [ str "Authentication" ] ]
      Menu.label [ ]
        [ str "Transactions" ]
      Menu.list [ ]
        [ Menu.item [ ]
            [ str "Payments" ]
          Menu.item [ ]
            [ str "Transfers" ]
          Menu.item [ ]
            [ str "Balance" ] ] ]

let breadcrump =
  Breadcrumb.breadcrumb [ ]
    [ Breadcrumb.item [ ]
        [ a [ ] [ str "Bulma" ] ]
      Breadcrumb.item [ ]
        [ a [ ] [ str "Templates" ] ]
      Breadcrumb.item [ ]
        [ a [ ] [ str "Examples" ] ]
      Breadcrumb.item [ Breadcrumb.Item.IsActive true ]
        [ a [ ] [ str "Admin" ] ] ]

let hero =
  Hero.hero [ Hero.CustomClass "is-info welcome is-small" ]
    [ Hero.body [ ]
        [ Container.container [ ]
            [ h1 [ Class "title" ]
                [ str "Hello, Admin." ]
              h2 [ Class "subtitle" ]
                [ safeComponents ] ] ] ] 

let info =
  section [ Class "info-tiles" ]
    [ Tile.ancestor [ Tile.CustomClass Alignment.HasTextCentered ]
        [ Tile.parent [ ]
            [ article [ Class "tile is-child box" ]
                [ p [ Class "title" ] [ str "439k" ]
                  p [ Class "subtitle" ] [ str "Users" ] ] ]
          Tile.parent [ ]
            [ article [ Class "tile is-child box" ]
                [ p [ Class "title" ] [ str "59k" ]
                  p [ Class "subtitle" ] [ str "Products" ] ] ]
          Tile.parent [ ]
            [ article [ Class "tile is-child box" ]
                [ p [ Class "title" ] [ str "3.4k" ]
                  p [ Class "subtitle" ] [ str "Open Orders" ] ] ]
          Tile.parent [ ]
            [ article [ Class "tile is-child box" ]
                [ p [ Class "title" ] [ str "19" ]
                  p [ Class "subtitle" ] [ str "Exceptions" ] ] ] ] ]

let counter (model : Model) (dispatch : Msg -> unit) =
  Form.Field.div [ Form.Field.IsGrouped ] 
    [ Form.Control.p [ Form.Control.CustomClass "is-expanded"] 
        [ Form.Input.text
            [ Form.Input.Disabled true
              Form.Input.Value (show model) ] ]
      Form.Control.p [ ]
        [ Button.a 
            [ Button.Color IsInfo
              Button.OnClick (fun _ -> dispatch Increment) ]
            [ str "+" ] ]
      Form.Control.p [ ]
        [ Button.a 
            [ Button.Color IsInfo
              Button.OnClick (fun _ -> dispatch Decrement) ]
            [ str "-" ] ] ]

let columns (model : Model) (dispatch : Msg -> unit) =
  Columns.columns [ ]
    [ Column.column [ Column.Width (Column.All, Column.Is6) ]
        [ Card.card [ CustomClass "events-card" ]
            [ Card.header [ ]
                [ Card.Header.title [ ]
                    [ str "Events" ]
                  Card.Header.icon [ ]
                    [ Icon.faIcon [ ]
                        [ Fa.icon Fa.I.AngleDown ] ] ]
              div [ Class "card-table" ]
                [ Content.content [ ]
                    [ Table.table 
                        [ Table.IsFullwidth
                          Table.IsStripped ]
                        [ tbody [ ]
                            [ for _ in 1..10 ->
                                tr [ ]
                                  [ td [ Style [ Width "5%" ] ]
                                      [ Icon.faIcon
                                          [ ]
                                          [ Fa.icon Fa.I.BellO ] ]
                                    td [ ]
                                      [ str "Lorem ipsum dolor aire" ]
                                    td [ ]
                                      [ Button.a 
                                          [ Button.Size IsSmall
                                            Button.Color IsPrimary ]
                                          [ str "Action" ] ] ] ] ] ] ]
              Card.footer [ ]
                [ Card.Footer.item [ ]
                    [ str "View All" ] ] ] ]
      Column.column [ Column.Width (Column.All, Column.Is6) ]
        [ Card.card [ ]
            [ Card.header [ ]
                [ Card.Header.title [ ]
                    [ str "Inventory Search" ]
                  Card.Header.icon [ ]
                    [ Icon.faIcon [ ]
                        [ Fa.icon Fa.I.AngleDown ] ] ]
              Card.content [ ]
                [ Content.content [ ]
                    [ Control.div 
                        [ Control.HasIconLeft
                          Control.HasIconRight ]
                        [ Input.text 
                            [ Input.Size IsLarge ]
                          Icon.faIcon 
                            [ Icon.Size IsMedium
                              Icon.IsLeft ]
                            [ Fa.icon Fa.I.Search ]
                          Icon.faIcon 
                            [ Icon.Size IsMedium
                              Icon.IsRight ]
                            [ Fa.icon Fa.I.Check ] ] ] ] ]
          Card.card [ ]
            [ Card.header [ ]
                [ Card.Header.title [ ]
                    [ str "Counter" ]
                  Card.Header.icon [ ]
                    [ Icon.faIcon [ ]
                        [ Fa.icon Fa.I.AngleDown ] ] ]
              Card.content [ ]
                [ Content.content [ ]
                    [ counter model dispatch ] ] ] ] ]

let view (model : Model) (dispatch : Msg -> unit) =
  div [ ]
    [ navBrand
      Container.container [ ]
        [ Columns.columns [ ]
            [ Column.column [ Column.Width (Column.All, Column.Is3) ]
                [ menu ]
              Column.column [ Column.Width (Column.All, Column.Is9) ]
                [ breadcrump
                  hero
                  info
                  columns model dispatch ] ] ] ]
#elseif (Fulma == "cover")
let navBrand =
  Navbar.Brand.div [ ] 
    [ Navbar.Item.a 
        [ Navbar.Item.Props 
            [ Href "https://safe-stack.github.io/"
              Style [ BackgroundColor "#00d1b2" ] ] ] 
        [ img [ Src "https://safe-stack.github.io/images/safe_top.png"
                Alt "Logo" ] ] 
      Navbar.burger [ ] 
        [ span [ ] [ ]
          span [ ] [ ]
          span [ ] [ ] ] ]

let navMenu =
  Navbar.menu [ ]
    [ Navbar.End.div [ ] 
        [ Navbar.Item.a [ ] 
            [ str "Home" ] 
          Navbar.Item.a [ ]
            [ str "Examples" ]
          Navbar.Item.a [ ]
            [ str "Documentation" ]
          Navbar.Item.div [ ]
            [ Button.a 
                [ Button.Size IsSmall
                  Button.Props [ Href "https://github.com/SAFE-Stack/SAFE-template" ] ] 
                [ Icon.faIcon [ ] 
                    [ Fa.icon Fa.I.Github; Fa.fw ]
                  span [ ] [ str "View Source" ] ] ] ] ]

let containerBox (model : Model) (dispatch : Msg -> unit) =
  Box.box' [ ]
    [ Form.Field.div [ Form.Field.IsGrouped ] 
        [ Form.Control.p [ Form.Control.CustomClass "is-expanded"] 
            [ Form.Input.text
                [ Form.Input.Disabled true
                  Form.Input.Value (show model) ] ]
          Form.Control.p [ ]
            [ Button.a 
                [ Button.Color IsPrimary
                  Button.OnClick (fun _ -> dispatch Increment) ]
                [ str "+" ] ]
          Form.Control.p [ ]
            [ Button.a 
                [ Button.Color IsPrimary
                  Button.OnClick (fun _ -> dispatch Decrement) ]
                [ str "-" ] ] ] ]

let view (model : Model) (dispatch : Msg -> unit) =
  Hero.hero 
    [ Hero.IsFullHeight
      Hero.IsBold ]
    [ Hero.head [ ]
        [ Navbar.navbar [  ]
            [ Container.container [ ]
                [ navBrand
                  navMenu ] ] ]
      Hero.body [ ]
        [ Container.container 
            [ Container.CustomClass Alignment.HasTextCentered ]
            [ Columns.columns [ Columns.IsVCentered ]
                [ Column.column 
                    [ Column.Width (Column.All, Column.Is5) ]
                    [ Image.image [ Image.Is4by3 ]
                        [ img [ Src "http://placehold.it/800x600" ] ] ]
                  Column.column 
                   [ Column.Width (Column.All, Column.Is5)
                     Column.Offset (Column.All, Column.Is1) ]
                   [ Heading.h1 [ Heading.Is2 ] 
                       [ str "Superhero Scaffolding" ]
                     Heading.h2 
                       [ Heading.IsSubtitle
                         Heading.Is4 ] 
                       [ safeComponents ]
                     containerBox model dispatch ] ] ] ]
      Hero.foot [ ]
        [ Container.container [ ]
            [ Tabs.tabs [ Tabs.IsCentered ]
                [ ul [ ]
                    [ li [ ]
                        [ a [ ]
                            [ str "And this at the bottom" ] ] ] ] ] ] ]
#elseif (Fulma == "hero")
let navBrand =
  Navbar.Brand.div [ ] 
    [ Navbar.Item.a 
        [ Navbar.Item.Props [ Href "https://safe-stack.github.io/" ] ] 
        [ img [ Src "https://safe-stack.github.io/images/safe_top.png"
                Alt "Logo" ] ] 
      Navbar.burger [ ] 
        [ span [ ] [ ]
          span [ ] [ ]
          span [ ] [ ] ] ]

let navMenu =
  Navbar.menu [ ]
    [ Navbar.End.div [ ] 
        [ Navbar.Item.a [ ] 
            [ str "Home" ] 
          Navbar.Item.a [ ]
            [ str "Examples" ]
          Navbar.Item.a [ ]
            [ str "Documentation" ]
          Navbar.Item.div [ ]
            [ Button.a 
                [ Button.Color IsWhite
                  Button.IsOutlined
                  Button.Size IsSmall
                  Button.Props [ Href "https://github.com/SAFE-Stack/SAFE-template" ] ] 
                [ Icon.faIcon [ ] 
                    [ Fa.icon Fa.I.Github; Fa.fw ]
                  span [ ] [ str "View Source" ] ] ] ] ]

let buttonBox (model : Model) (dispatch : Msg -> unit) =
  Box.box' [ CustomClass "cta" ]
    [ Level.level [ ]
        [ Level.item [ ]
            [ Button.a 
                [ Button.Color IsPrimary
                  Button.OnClick (fun _ -> dispatch Increment) ]
                [ str "+" ] ]

          Level.item [ ]
            [ p [ ] [ str (show model) ] ]

          Level.item [ ]
            [ Button.a 
                [ Button.Color IsPrimary
                  Button.OnClick (fun _ -> dispatch Decrement) ]
                [ str "-" ] ] ] ]

let card icon heading body =
  Column.column [ Column.Width (Column.All, Column.Is4) ]
    [ Card.card [ ]
        [ div
            [ ClassName (Card.Classes.Image + " " + Alignment.HasTextCentered) ]
            [ i [ ClassName ("fa fa-" + icon) ] [ ] ]
          Card.content [ ]
            [ Content.content [ ]
                [ h4 [ ] [ str heading ]
                  p [ ] [ str body ]
                  p [ ]
                    [ a [ Href "#" ]
                        [ str "Learn more" ] ] ] ] ] ]

let features = 
  Columns.columns [ Columns.CustomClass "features" ]
    [ card "paw" "Tristique senectus et netus et." "Purus semper eget duis at tellus at urna condimentum mattis. Non blandit massa enim nec. Integer enim neque volutpat ac tincidunt vitae semper quis. Accumsan tortor posuere ac ut consequat semper viverra nam."
      card "id-card-o" "Tempor orci dapibus ultrices in." "Ut venenatis tellus in metus vulputate. Amet consectetur adipiscing elit pellentesque. Sed arcu non odio euismod lacinia at quis risus. Faucibus turpis in eu mi bibendum neque egestas cmonsu songue. Phasellus vestibulum lorem sed risus."
      card "rocket" "Leo integer malesuada nunc vel risus." "Imperdiet dui accumsan sit amet nulla facilisi morbi. Fusce ut placerat orci nulla pellentesque dignissim enim. Libero id faucibus nisl tincidunt eget nullam. Commodo viverra maecenas accumsan lacus vel facilisis." ]

let intro =
  Column.column 
    [ Column.CustomClass "intro"
      Column.Width (Column.All, Column.Is8)
      Column.Offset (Column.All, Column.Is2) ]
    [ h2 [ ClassName "title" ] [ str "Perfect for developers or designers!" ]
      br [ ]
      p [ ClassName "subtitle"] [ str "Vel fringilla est ullamcorper eget nulla facilisi. Nulla facilisi nullam vehicula ipsum a. Neque egestas congue quisque egestas diam in arcu cursus." ] ]

let tile title subtitle content =
  article [ ClassName "tile is-child notification is-white" ]
    [ yield p [ ClassName "title" ] [ str title ]
      yield p [ ClassName "subtitle" ] [ str subtitle ]
      match content with
      | Some c -> yield c
      | None -> () ]

let content txts =
  Content.content [ ]
    [ for txt in txts -> p [ ] [ str txt ] ]

module Tiles =
  let hello = tile "Hello World" "What is up?" None

  let foo = tile "Foo" "Bar" None

  let third = 
    tile 
      "Third column"
      "With some content"
      (Some (content ["Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin ornare magna eros, eu pellentesque tortor vestibulum ut. Maecenas non massa sem. Etiam finibus odio quis feugiat facilisis."]))

  let verticalTop = tile "Vertical tiles" "Top box" None
  
  let verticalBottom = tile "Vertical tiles" "Bottom box" None

  let middle = 
    tile 
      "Middle box" 
      "With an image"
      (Some (figure [ ClassName "image is-4by3" ] [ img [ Src "http://bulma.io/images/placeholders/640x480.png"] ]))

  let wide =
    tile
      "Wide column"
      "Aligned with the right column"
      (Some (content ["Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin ornare magna eros, eu pellentesque tortor vestibulum ut. Maecenas non massa sem. Etiam finibus odio quis feugiat facilisis."]))

  let tall =
    tile
      "Tall column"
      "With even more content"
      (Some (content 
              ["Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam semper diam at erat pulvinar, at pulvinar felis blandit. Vestibulum volutpat tellus diam, consequat gravida libero rhoncus ut. Morbi maximus, leo sit amet vehicula eleifend, nunc dui porta orci, quis semper odio felis ut quam."
               "Suspendisse varius ligula in molestie lacinia. Maecenas varius eget ligula a sagittis. Pellentesque interdum, nisl nec interdum maximus, augue diam porttitor lorem, et sollicitudin felis neque sit amet erat. Maecenas imperdiet felis nisi, fringilla luctus felis hendrerit sit amet. Aenean vitae gravida diam, finibus dignissim turpis. Sed eget varius ligula, at volutpat tortor."
               "Integer sollicitudin, tortor a mattis commodo, velit urna rhoncus erat, vitae congue lectus dolor consequat libero. Donec leo ligula, maximus et pellentesque sed, gravida a metus. Cras ullamcorper a nunc ac porta. Aliquam ut aliquet lacus, quis faucibus libero. Quisque non semper leo."]))

  let side = 
    tile
      "Side column"
      "With some content"
      (Some (content ["Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin ornare magna eros, eu pellentesque tortor vestibulum ut. Maecenas non massa sem. Etiam finibus odio quis feugiat facilisis."]))

  let main =
    tile
      "Main column"
      "With some content"
      (Some (content ["Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin ornare magna eros, eu pellentesque tortor vestibulum ut. Maecenas non massa sem. Etiam finibus odio quis feugiat facilisis."]))


let sandbox =
  div [ ClassName "sandbox" ]
    [ Tile.ancestor [ ]
        [ Tile.parent [ ] 
            [ Tiles.hello ]
          Tile.parent [ ] 
            [ Tiles.foo ]
          Tile.parent [  ] 
            [ Tiles.third ] ]
      Tile.ancestor [ ]
        [ Tile.tile [ Tile.IsVertical; Tile.Size Tile.Is8 ]
            [ Tile.tile [ ] 
                [ Tile.parent [ Tile.IsVertical ] 
                    [ Tiles.verticalTop
                      Tiles.verticalBottom ]
                  Tile.parent [ ] 
                    [ Tiles.middle ] ]
              Tile.parent [ ]
                [ Tiles.wide ] ]
          Tile.parent [ ]
            [ Tiles.tall ] ]
      Tile.ancestor [ ]
        [ Tile.parent [ ]
            [ Tiles.side ]
          Tile.parent [ Tile.Size Tile.Is8 ]
            [ Tiles.main ] ] ]

let footerContainer =
  Container.container [ ]
    [ Content.content [ Content.CustomClass Alignment.HasTextCentered ] 
        [ p [ ] 
            [ safeComponents ]
          p [ ]
            [ a [ Href "https://github.com/SAFE-Stack/SAFE-template" ]
                [ Icon.faIcon [ ] 
                    [ Fa.icon Fa.I.Github; Fa.fw ] ] ] ] ]

let view (model : Model) (dispatch : Msg -> unit) =
  div [ ]
    [ Hero.hero 
        [ Hero.Color IsPrimary
          Hero.IsMedium
          Hero.IsBold ]
        [ Hero.head [ ]
            [ Navbar.navbar [ ]
                [ Container.container [ ] 
                    [ navBrand
                      navMenu ] ] ]
          Hero.body [ ]
            [ Container.container [ Container.CustomClass Alignment.HasTextCentered ]
                [ h1 [ ClassName "title" ] 
                    [ str "SAFE Template" ]
                  div [ ClassName "subtitle" ]
                    [ safeComponents ] ] ] ]
      
      buttonBox model dispatch
      
      Container.container [ ]
        [ features
          intro
          sandbox ]

      footer [ ClassName "footer" ]
        [ footerContainer ] ]
#elseif (Fulma == "landing")
let navBrand =
  Navbar.Brand.div [ ] 
    [ Navbar.Item.a 
        [ Navbar.Item.Props [ Href "https://safe-stack.github.io/" ]
          Navbar.Item.IsActive true ] 
        [ img [ Src "https://safe-stack.github.io/images/safe_top.png"
                Alt "Logo" ] ] 
      Navbar.burger [ ] 
        [ span [ ] [ ]
          span [ ] [ ]
          span [ ] [ ] ] ]

let navMenu =
  Navbar.menu [ ]
    [ Navbar.End.div [ ] 
        [ Navbar.Item.a [ ] 
            [ str "Home" ] 
          Navbar.Item.a [ ]
            [ str "Examples" ]
          Navbar.Item.a [ ]
            [ str "Documentation" ]
          Navbar.Item.div [ ]
            [ Button.a 
                [ Button.Color IsWhite
                  Button.IsOutlined
                  Button.Size IsSmall
                  Button.Props [ Href "https://github.com/SAFE-Stack/SAFE-template" ] ] 
                [ Icon.faIcon [ ] 
                    [ Fa.icon Fa.I.Github; Fa.fw ]
                  span [ ] [ str "View Source" ] ] ] ] ]

let containerBox (model : Model) (dispatch : Msg -> unit) =
  Box.box' [ ]
    [ Form.Field.div [ Form.Field.IsGrouped ] 
        [ Form.Control.p [ Form.Control.CustomClass "is-expanded"] 
            [ Form.Input.text
                [ Form.Input.Disabled true
                  Form.Input.Value (show model) ] ]
          Form.Control.p [ ]
            [ Button.a 
                [ Button.Color IsPrimary
                  Button.OnClick (fun _ -> dispatch Increment) ]
                [ str "+" ] ]
          Form.Control.p [ ]
            [ Button.a 
                [ Button.Color IsPrimary
                  Button.OnClick (fun _ -> dispatch Decrement) ]
                [ str "-" ] ] ] ]

let view (model : Model) (dispatch : Msg -> unit) =
  Hero.hero [ Hero.Color IsPrimary; Hero.IsFullHeight ] 
    [ Hero.head [ ] 
        [ Navbar.navbar [ ]
            [ Container.container [ ]
                [ navBrand
                  navMenu ] ] ]
      
      Hero.body [ ] 
        [ Container.container [ Container.CustomClass Alignment.HasTextCentered ]
            [ Column.column 
                [ Column.Width (Column.All, Column.Is6)
                  Column.Offset (Column.All, Column.Is3) ]
                [ h1 [ ClassName "title" ] 
                    [ str "SAFE Template" ]
                  div [ ClassName "subtitle" ]
                    [ safeComponents ]
                  containerBox model dispatch ] ] ] ]
#else

let counter (model : Model) (dispatch : Msg -> unit) =
  Form.Field.div [ Form.Field.IsGrouped ] 
    [ Form.Control.p [ Form.Control.CustomClass "is-expanded"] 
        [ Form.Input.text
            [ Form.Input.Disabled true
              Form.Input.Value (show model) ] ]
      Form.Control.p [ ]
        [ Button.a 
            [ Button.Color IsInfo
              Button.OnClick (fun _ -> dispatch Increment) ]
            [ str "+" ] ]
      Form.Control.p [ ]
        [ Button.a 
            [ Button.Color IsInfo
              Button.OnClick (fun _ -> dispatch Decrement) ]
            [ str "-" ] ] ]

let column (model : Model) (dispatch : Msg -> unit) =
  Column.column 
    [ Column.Width (Column.All, Column.Is4)
      Column.Offset (Column.All, Column.Is4) ]
    [ Heading.h3
        [ Heading.CustomClass "title has-text-grey" ]
        [ str "Login" ]
      p [ Class "subtitle has-text-grey" ]
        [ str "Please login to proceed." ]
      Box.box' [ ]
        [ figure [ Class "avatar" ]
            [ img [ Src "https://placehold.it/128x128" ] ]
          form [ ]
            [ Field.div [ ]
                [ Control.div [ ]
                    [ Input.email 
                        [ Input.Size IsLarge
                          Input.Placeholder "Your Email"
                          Input.Props [ AutoFocus true ] ] ] ]
              Field.div [ ]
                [ Control.div [ ]
                    [ Input.password 
                        [ Input.Size IsLarge
                          Input.Placeholder "Your Password" ] ] ]
              counter model dispatch
              Field.div [ ]
                [ Checkbox.checkbox [ ]
                    [ input [ Type "checkbox" ]
                      str "Remember me" ] ]
              Button.button 
                [ Button.Color IsInfo
                  Button.IsFullwidth
                  Button.CustomClass "is-large is-block" ]
                [ str "Login" ] ] ]
      p [ Class "has-text-grey" ]
        [ a [ ] [ str "Sign Up" ]
          str "\u00A0·\u00A0"
          a [ ] [ str "Forgot Password" ]
          str "\u00A0·\u00A0"
          a [ ] [ str "Need Help?" ] ]
      br [ ]
      p [ Class "has-text-grey" ] 
        [ safeComponents ] ]

let view (model : Model) (dispatch : Msg -> unit) =
  Hero.hero 
    [ Hero.Color IsSuccess 
      Hero.IsFullHeight ]
    [ Hero.body [ ]
        [ Container.container 
            [ Container.CustomClass Alignment.HasTextCentered ]
            [ column model dispatch ] ] ]

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
