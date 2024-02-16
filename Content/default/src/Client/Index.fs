module Index

open Elmish
open SAFE
open Shared

type Model = {
    Todos: Deferred<Todo list>
    Input: string
}

type Msg =
    | SetInput of string
    | GetTodos of AsyncOperation<unit, Todo list>
    | AddTodo of AsyncOperation<unit, Todo>

let todosApi = Api.makeProxy<ITodosApi> ()

let init () =
    let model = { Todos = NotStarted; Input = "" }
    model, Cmd.ofMsg (GetTodos(Start()))

let update msg model =
    match msg with
    | SetInput value -> { model with Input = value }, Cmd.none
    | GetTodos msg ->
        match msg with
        | Start() ->
            let cmd = Cmd.OfAsync.perform todosApi.getTodos () (Finished >> GetTodos)
            { model with Todos = InProgress }, cmd
        | Finished todos -> { model with Todos = Resolved todos }, Cmd.none
    | AddTodo msg ->
        match msg with
        | Start() ->
            let todo = Todo.create model.Input
            let cmd = Cmd.OfAsync.perform todosApi.addTodo todo (Finished >> AddTodo)
            { model with Input = "" }, cmd
        | Finished todo ->
            {
                model with
                    Todos = model.Todos.Map(fun todos -> todos @ [ todo ])
            },
            Cmd.none

open Feliz

let private todoAction model dispatch =
    Html.div [
        prop.className "flex flex-col sm:flex-row mt-4 gap-4"
        prop.children [
            Html.input [
                prop.className
                    "shadow appearance-none border rounded w-full py-2 px-3 outline-none focus:ring-2 ring-teal-300 text-grey-darker"
                prop.value model.Input
                prop.placeholder "What needs to be done?"
                prop.autoFocus true
                prop.onChange (SetInput >> dispatch)
                prop.onKeyPress (fun ev ->
                    if ev.key = "Enter" then
                        dispatch (AddTodo(Start())))
            ]
            Html.button [
                prop.className
                    "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
                prop.disabled (Todo.isValid model.Input |> not)
                prop.onClick (fun _ -> dispatch (AddTodo(Start())))
                prop.text "Add"
            ]
        ]
    ]

let private todoList model dispatch =
    Html.div [
        prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
        prop.children [
            Html.ol [
                prop.className "list-decimal ml-6"
                prop.children [
                    match model.Todos with
                    | NotStarted -> Html.text "Not Started."
                    | InProgress -> Html.text "Loading..."
                    | Resolved todos ->
                        for todo in todos do
                            Html.li [ prop.className "my-1"; prop.text todo.Description ]
                ]
            ]

            todoAction model dispatch
        ]
    ]

let view model dispatch =
    Html.section [
        prop.className "h-screen w-screen"
        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]

        prop.children [
            Html.a [
                prop.href "https://safe-stack.github.io/"
                prop.className "absolute block ml-12 h-12 w-12 bg-teal-300 hover:cursor-pointer hover:bg-teal-400"
                prop.children [ Html.img [ prop.src "/favicon.png"; prop.alt "Logo" ] ]
            ]

            Html.div [
                prop.className "flex flex-col items-center justify-center h-full"
                prop.children [
                    Html.h1 [
                        prop.className "text-center text-5xl font-bold text-white mb-3 rounded-md p-4"
                        prop.text "SAFE.App"
                    ]
                    todoList model dispatch
                ]
            ]
        ]
    ]