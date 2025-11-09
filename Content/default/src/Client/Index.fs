module Index

open Elmish
open SAFE
open Shared

type Model = {
    Todos: RemoteData<Todo list>
    Input: string
}

type Msg =
    | SetInput of string
    | LoadTodos of ApiCall<unit, Todo list>
    | SaveTodo of ApiCall<string, Todo list>

let todosApi = Api.makeProxy<ITodosApi> ()

let init () =
    let initialModel = { Todos = NotStarted; Input = "" }
    let initialCmd = LoadTodos(Start()) |> Cmd.ofMsg

    initialModel, initialCmd

let update msg model =
    match msg with
    | SetInput value -> { model with Input = value }, Cmd.none
    | LoadTodos msg ->
        match msg with
        | Start() ->
            let loadTodosCmd = Cmd.OfAsync.perform todosApi.getTodos () (Finished >> LoadTodos)

            { model with Todos = model.Todos.StartLoading() }, loadTodosCmd
        | Finished todos -> { model with Todos = Loaded todos }, Cmd.none
    | SaveTodo msg ->
        match msg with
        | Start todoText ->
            let saveTodoCmd =
                let todo = Todo.create todoText
                Cmd.OfAsync.perform todosApi.addTodo todo (Finished >> SaveTodo)

            { model with Input = "" }, saveTodoCmd
        | Finished todos ->
            {
                model with
                    Todos = RemoteData.Loaded todos
            },
            Cmd.none

open Feliz
open Feliz.DaisyUI

module ViewComponents =

    let todoAction model dispatch =
        Html.div [
            prop.className "flex flex-col sm:flex-row mt-4 gap-4"
            prop.children [
                Daisy.input [
                    prop.className
                        "shadow-sm appearance-none border rounded-sm w-full py-2 px-3 outline-hidden focus:ring-2 ring-teal-300 text-grey-darker text-sm sm:text-base"
                    prop.value model.Input
                    prop.placeholder "What needs to be done?"
                    prop.autoFocus true
                    prop.onChange (SetInput >> dispatch)
                    prop.onKeyPress (fun ev ->
                        if ev.key = "Enter" then
                            dispatch (SaveTodo(Start model.Input)))
                ]
                Daisy.button.button [
                    prop.className
                        "flex-no-shrink p-2 px-12 rounded-sm bg-teal-600 outline-hidden focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed text-sm sm:text-base"
                    prop.disabled (Todo.isValid model.Input |> not)
                    prop.onClick (fun _ -> dispatch (SaveTodo(Start model.Input)))
                    prop.text "Add"
                ]
            ]
        ]

    let todoList model dispatch =
        Html.div [
            prop.className "p-2 sm:p-4 w-full"
            prop.children [
                Daisy.list [
                    prop.className "list-decimal ml-4 sm:ml-6"
                    prop.children [
                        match model.Todos with
                        | NotStarted -> Html.text "Not Started."
                        | Loading None -> Html.text "Loading..."
                        | Loading (Some todos)
                        | Loaded todos ->
                            for todo in todos do
                                Html.li [
                                    prop.className "my-1 text-black text-base sm:text-lg break-words"
                                    prop.text todo.Description
                                ]
                    ]
                ]

                todoAction model dispatch
            ]
        ]

let view model dispatch =
    Html.div [
        prop.className "h-screen w-screen relative overflow-hidden"
        prop.children [
            // Meta viewport tag for proper mobile scaling
            Html.meta [
                prop.name "viewport"
                prop.content "width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"
            ]
            
            // Background image and content card
            Daisy.hero [
                prop.className "absolute inset-0 bg-cover bg-center bg-fixed bg-no-repeatbg-white/20 backdrop-blur-xs"
                prop.style [
                    style.backgroundImageUrl "https://unsplash.it/1200/900?random"
                ]
                prop.children [
                    Daisy.heroContent [
                        Daisy.card [
                            prop.className "bg-white/20 backdrop-blur-lg p-4 sm:p-8 rounded-xl shadow-lg border border-white/30 mx-4 sm:mx-0 max-w-full sm:max-w-2xl"
                            prop.children [
                                Html.h1 [
                                    prop.className "text-center text-3xl sm:text-5xl font-bold mb-3 p-2 sm:p-4"
                                    prop.text "SAFE.App"
                                ]
                                ViewComponents.todoList model dispatch
                            ]
                        ]
                    ]
                ]
            ]

            // Top-left logo link - positioned absolutely without blocking overlay
            Html.a [
                prop.href "https://safe-stack.github.io/"
                prop.className "absolute top-4 left-4 sm:top-6 sm:left-12 z-20 block h-10 w-10 sm:h-12 sm:w-12 bg-teal-300 hover:cursor-pointer hover:bg-teal-400"
                prop.children [
                    Html.img [ prop.src "/favicon.png"; prop.alt "Logo" ]
                ]
            ]
        ]
    ]
