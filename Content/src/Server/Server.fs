module SAFE.App.Server

(*#if (minimal)
open FSharp.Control.Tasks.V2
open Giraffe
#else*)
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
//#endif
open Saturn
open SAFE.App.Shared

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

(*#if (minimal)
let webApp =
    router {
        get Routes.todos (json (storage.GetTodos()))
        post Routes.todos (fun next ctx ->
            task {
                let! todo = ctx.BindModelAsync<_>()
                match storage.AddTodo todo with
                | Ok () -> return! json todo next ctx
                | Error e -> return! RequestErrors.BAD_REQUEST e next ctx
            })
    }
#else*)
let todosApi =
    { getTodos = async { return storage.GetTodos() }
      addTodo =
        fun todo -> async {
            match storage.AddTodo todo with
            | Ok () -> return todo
            | Error e -> return failwith e
        } }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler
//#endif

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
(*#if (minimal)
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
#else
#endif*)
        use_gzip
    }

run app