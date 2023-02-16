module Index

open Elmish
open Fable.Remoting.Client
open Shared
open System

type Model = { Todos: Todo list; Input: string; Average: int }

type Msg =
    | GotTodos of Todo list
    | GotAverage of int
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo
    | RemoveTodo of Guid
    | RemovedTodo of Guid


let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let init () : Model * Cmd<Msg> =
    let model = { Todos = []; Input = ""; Average = 0 }

    let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | GotTodos todos ->
        let cmd = Cmd.OfAsync.perform todosApi.getAverageTime () GotAverage
        { model with Todos = todos }, cmd
    | GotAverage average -> {model with Average = average}, Cmd.none
    | SetInput value -> { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input
        let cmd = Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo
        { model with Input = "" }, cmd
    | AddedTodo _ ->
        let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos
        model, cmd
    | RemoveTodo guid ->
        let cmd = Cmd.OfAsync.perform todosApi.removeTodo guid RemovedTodo
        model, cmd
    | RemovedTodo _ ->
        let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos
        model, cmd

open Feliz
open Feliz.Bulma

let navBrand =
    Bulma.navbarBrand.div [
        Bulma.navbarItem.a [
            prop.href "https://safe-stack.github.io/"
            navbarItem.isActive
            prop.children [
                Html.img [
                    prop.src "/favicon.png"
                    prop.alt "Logo"
                ]
            ]
        ]
    ]

let containerBox (model: Model) (dispatch: Msg -> unit) =
    Bulma.box [
        Bulma.content [
            Html.ol [
                for todo in model.Todos do
                    Html.li [ prop.text todo.Description ]
                    Html.button [
                        prop.classes ["button"; "is-danger is-small is-outlined"]
                        prop.text "x"
                        prop.onClick (fun _ -> dispatch (RemoveTodo todo.Id))
                    ]
            ]
        ]
        Bulma.field.div [
            field.isGrouped
            prop.children [
                Bulma.control.p [
                    control.isExpanded
                    prop.children [
                        Bulma.input.text [
                            prop.value model.Input
                            prop.placeholder "What needs to be done?"
                            prop.onChange (fun x -> SetInput x |> dispatch)
                        ]
                        Bulma.box [
                            prop.text (sprintf "average: %d" (model.Average))
                        ]
                    ]
                ]
                Bulma.control.p [
                    Bulma.button.a [
                        color.isPrimary
                        prop.disabled (Todo.isValid model.Input |> not)
                        prop.onClick (fun _ -> dispatch AddTodo)
                        prop.text "Add"
                    ]
                ]
            ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.hero [
        hero.isFullHeight
        color.isPrimary
        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]
        prop.children [
            Bulma.heroHead [
                Bulma.navbar [
                    Bulma.container [ navBrand ]
                ]
            ]
            Bulma.heroBody [
                Bulma.container [
                    Bulma.column [
                        column.is6
                        column.isOffset3
                        prop.children [
                            Bulma.title [
                                text.hasTextCentered
                                prop.text "safeTodoEvents"
                            ]
                            containerBox model dispatch
                        ]
                    ]
                ]
            ]
        ]
    ]