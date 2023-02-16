module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open BackEnd
open Shared

let todosApi =
    {
        getTodos =
            fun () ->
                async
                    {
                        return
                            match (App.getAllTodos()) with
                            | Ok l -> l
                            | Error x -> failwith x
                    }
        addTodo =
            fun todo ->
                async {
                    return
                        match (App.addTodo todo) with
                        | Ok _ -> todo
                        | Error x -> failwith x
                }
        removeTodo =
            fun guid ->
                async {
                    return
                        match (App.removeTodo guid) with
                        | Ok _ -> guid
                        | Error x -> failwith x
                }
        getAverageTime =
            fun () ->
                async
                    {
                        return
                            match (BackEnd.App.AverageTime()) with
                            | Ok t -> t
                            | Error x -> failwith x
                    }
    }

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler

let app =
    application {
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

[<EntryPoint>]
let main _ =
    run app
    0