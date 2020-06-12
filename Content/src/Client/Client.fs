module Client

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Shared
(*#if (minimal)
open Thoth.Fetch
#else*)
open Fable.Remoting.Client
//#endif

type Model =
    { Todos: Todo list
      Input: string }

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo

(*#if (minimal)
let getTodos() = Fetch.get<unit, Todo list> Routes.todos
let addTodo(todo) = Fetch.post<Todo, Todo> (Routes.todos, todo)
#else*)
let todosApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>
//#endif

let init(): Model * Cmd<Msg> =
    let model =
        { Todos = []
          Input = "" }
(*#if (minimal)
    let cmd = Cmd.OfPromise.perform getTodos () GotTodos
#else*)
    let cmd = Cmd.OfAsync.perform (fun _ -> todosApi.getTodos) () GotTodos
//#endif
    model, cmd

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | GotTodos todos ->
        { model with Todos = todos }, Cmd.none
    | SetInput value ->
        { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input
(*#if (minimal)
        let cmd = Cmd.OfPromise.perform addTodo todo AddedTodo
#else*)
        let cmd = Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo
//#endif
        { model with Input = "" }, cmd
    | AddedTodo todo ->
        { model with Todos = model.Todos @ [ todo ] }, Cmd.none

(*#if minimal
let view model dispatch =
    div [ Style [ TextAlign TextAlignOptions.Center; Padding 40 ] ] [
        div [ Style [ Display DisplayOptions.InlineBlock ] ] [
            img [ Src "favicon.png" ]
            h1 [] [ str "minimal" ]
            ol [ Style [ MarginRight 20 ] ] [
                for todo in model.Todos ->
                    li [] [ str todo.Description ]
            ]
            input
                [ Value model.Input
                  Placeholder "What needs to be done?"
                  OnChange (fun e -> SetInput e.Value |> dispatch) ]
            button
                [ Disabled (Todo.isValid model.Input |> not)
                  OnClick (fun _ -> dispatch AddTodo) ]
                [ str "Add" ]
        ]
    ]
#else*)
open Fulma

let navBrand =
    Navbar.Brand.div [ ] [
        Navbar.Item.a [
            Navbar.Item.Props [ Href "https://safe-stack.github.io/" ]
            Navbar.Item.IsActive true
        ] [
            img [ Src "https://safe-stack.github.io/images/safe_top.png"
                  Alt "Logo" ]
        ]
    ]

let containerBox (model : Model) (dispatch : Msg -> unit) =
    Box.box' [ ]
        [ Content.content [ ]
            [ Content.Ol.ol [ ]
                [ for todo in model.Todos ->
                    li [ ] [ str todo.Description ] ] ]
          Field.div [ Field.IsGrouped ]
            [ Control.p [ Control.IsExpanded ]
                [ Input.text
                    [ Input.Value model.Input
                      Input.Placeholder "What needs to be done?"
                      Input.OnChange (fun x -> SetInput x.Value |> dispatch) ] ]
              Control.p [ ] [
                Button.a [
                    Button.Color IsPrimary
                    Button.Disabled (Todo.isValid model.Input |> not)
                    Button.OnClick (fun _ -> dispatch AddTodo) ]
                  [ str "Add" ] ] ] ]

let view (model : Model) (dispatch : Msg -> unit) =
    Hero.hero [
        Hero.Color IsPrimary
        Hero.IsFullHeight
        Hero.Props [
            Style [
                Background """linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url("https://unsplash.it/1200/900?random") no-repeat center center fixed"""
                BackgroundSize "cover"
            ]
        ]
    ] [
        Hero.head [ ] [
            Navbar.navbar [ ] [
                Container.container [ ] [ navBrand ]
            ]
        ]

        Hero.body [ ] [
            Container.container [ ] [
                Column.column [
                    Column.Width (Screen.All, Column.Is6)
                    Column.Offset (Screen.All, Column.Is3)
                ] [
                    Heading.p [ Heading.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [ str "SAFE.App" ]
                    containerBox model dispatch
                ]
            ]
        ]
    ]
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
