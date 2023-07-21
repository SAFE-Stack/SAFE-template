module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Model = { Todos: Todo list; Input: string }

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let init () : Model * Cmd<Msg> =
    let model = { Todos = []; Input = "" }

    let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | GotTodos todos -> { model with Todos = todos }, Cmd.none
    | SetInput value -> { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input

        let cmd = Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

        { model with Input = "" }, cmd
    | AddedTodo todo ->
        { model with
            Todos = model.Todos @ [ todo ] },
        Cmd.none

open Feliz

let private todoAction (model: Model) (dispatch: Dispatch<Msg>) =
    Html.div
        [ prop.className "flex mt-4"
          prop.children
              [ Html.input
                    [ prop.className "shadow appearance-none border rounded w-full py-2 px-3 mr-4 text-grey-darker"
                      prop.value model.Input
                      prop.placeholder "What needs to be done?"
                      prop.autoFocus true
                      prop.onChange (SetInput >> dispatch)
                      prop.onKeyPress (fun ev ->
                          if ev.key = "Enter" then
                              dispatch AddTodo) ]
                Html.button
                    [ prop.className
                          "flex-no-shrink p-2 rounded bg-teal-300 hover:bg-teal disabled:opacity-30     disabled:cursor-not-allowed"
                      prop.disabled (Todo.isValid model.Input |> not)
                      prop.onClick (fun _ -> dispatch AddTodo)
                      prop.text "Add" ] ] ]

let private todoList (model: Model) (dispatch: Dispatch<Msg>) =
    Html.div
        [ prop.className "bg-white rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
          prop.children
              [ Html.ol
                    [ prop.className "list-decimal ml-6"
                      prop.children
                          [ for todo in model.Todos do
                                Html.li [ prop.className "my-1"; prop.text todo.Description ] ] ]

                todoAction model dispatch ] ]

let view (model: Model) (dispatch: Dispatch<Msg>) =
    Html.section
        [ prop.className "h-screen w-screen"
          prop.style
              [ style.backgroundSize "cover"
                style.backgroundImageUrl "https://unsplash.it/1200/900?random"
                style.backgroundPosition "no-repeat center center fixed" ]

          prop.children
              [ Html.a
                    [ prop.href "https://safe-stack.github.io/"
                      prop.className "absolute block ml-32 h-12 w-12 bg-teal-300 hover:cursor-pointer hover:bg-teal-400"
                      prop.children [ Html.img [ prop.src "/favicon.png"; prop.alt "Logo" ] ] ]

                Html.div
                    [ prop.className "flex flex-col items-center justify-center h-full"
                      prop.children
                          [ Html.h1
                                [ prop.className
                                      "text-center text-5xl text-white mb-3 bg-gray-900 bg-opacity-25 rounded-md p-4"
                                  prop.text "SAFE.App" ]
                            todoList model dispatch ] ] ] ]