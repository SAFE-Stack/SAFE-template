module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared

module Storage =
    let todos = ResizeArray ()
    let addTodo (todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

Storage.addTodo (Todo.create "Create new SAFE project") |> ignore
Storage.addTodo (Todo.create "Write your app") |> ignore
Storage.addTodo (Todo.create "Ship it !!!") |> ignore

let todosApi =
    {
        getTodos = fun () -> async {
            return List.ofSeq Storage.todos
        }
        addTodo = fun todo -> async {
            return
                match Storage.addTodo todo with
                | Ok () -> todo
                | Error e -> failwith e
        }
    }

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app