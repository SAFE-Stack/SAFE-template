module Server

open System.IO

open FSharp.Control.Tasks.V2
open Giraffe
open Saturn

open Shared

let getEnvVar (name: string) (defaultValue: string) =
    System.Environment.GetEnvironmentVariable name
    |> function
    | null
    | "" -> defaultValue
    | x -> x

let publicPath = getEnvVar "public_path" "../Client/public" |> Path.GetFullPath
let port = getEnvVar "PORT" "8085" |> uint16

type Storage () =
    let todos = ResizeArray<_>()

    member __.GetTodos () =
        List.ofSeq todos

    member __.AddTodo (todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok ()
        else Error "Invalid todo"

let storage = Storage()

storage.AddTodo(Todo.create "Create new project from SAFE template") |> ignore
storage.AddTodo(Todo.create "Customize to your own needs") |> ignore
storage.AddTodo(Todo.create "Profit !!!") |> ignore

let webApp =
    router {
        get Routes.todos (fun next ctx ->
            task {
                let todos = storage.GetTodos()
                return! json todos next ctx
            })
        post Routes.todos (fun next ctx ->
            task {
                let! todo = ctx.BindModelAsync<_>()
                match storage.AddTodo todo with
                | Ok () -> return! json todo next ctx
                | Error e -> return! RequestErrors.BAD_REQUEST e next ctx
            }) }

let app =
    application {
        url ("http://0.0.0.0:" + port.ToString() + "/")
        use_router webApp
        memory_cache
        use_static publicPath
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
        use_gzip
    }

run app
