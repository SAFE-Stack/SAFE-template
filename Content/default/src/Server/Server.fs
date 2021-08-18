module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Saturn
open Shared

type Storage () as this =
    let todos = ResizeArray()
    do
        this.AddTodo (Todo.create "Create new SAFE project") |> ignore
        this.AddTodo (Todo.create "Write your app") |> ignore
        this.AddTodo (Todo.create "Ship it !!!") |> ignore

    member _.GetTodos () =
        List.ofSeq todos
    member _.AddTodo (todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

let todosApi (context:HttpContext) =
    let storage = context.GetService<Storage>()
    {
        getTodos = fun () -> async {
            return storage.GetTodos()
        }
        addTodo = fun todo -> async {
            match storage.AddTodo todo with
            | Ok () -> return todo
            | Error e -> return failwith e
        }
    }

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromContext todosApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        service_config (fun services -> services.AddSingleton<Storage>())
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
