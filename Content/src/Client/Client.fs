module SAFE.App.Client

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open SAFE.App.Shared
(*#if (minimal)
open Thoth.Fetch
#else*)
open Fable.Remoting.Client
//#endif

// The model holds state that you want to keep track of while the application is running
// In this case, we are keeping track of list of Todos and Input value
// The Input denotes value for a new todo to be added
type Model =
    { Todos: Todo list
      Input: string }

// The Msg type defines what events/actions can occur while the application is running
// The state of the application changes only in reaction to these events
type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo

// Below functions send HTTP requests to server
(*#if (minimal)
let getTodos() = Fetch.get<unit, Todo list> Routes.todos
let addTodo(todo) = Fetch.post<Todo, Todo> (Routes.todos, todo)
#else*)
let todosApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>
//#endif

// Init function defines initial state (model) and command (side effect) of the application
// Todos are empty - they will be fetched from server using `Cmd` over promise
// Input is also empty
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

// The update function computes the next state of the application
// It does so based on the current state and the incoming message
// It can also run side-effects (encoded as commands) like calling the server via HTTP
// These commands in turn, can dispatch messages to which the update function will react
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

// View takes current model and generates React elements
// It can also use `dispatch` to trigger Msg from UI elements

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

// In development mode open namepaces for debugging and hot-module replacement
//-:cnd:noEmit
#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

// Following is the entry point for the application - in F# we don't need `main`
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
