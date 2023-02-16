namespace Shared
open System
open FSharp.Core
open EventSourcing
open Todos
open TodoEvents
open Shared
open FSharpPlus

module Commands =
    type Command =
        | AddTodo of Todo
        | RemoveTodo of Guid

        interface Executable<Todos, Event> with
            member this.Execute (x: Todos) =
                match this with
                | AddTodo t ->
                    match
                        Cache.memoize (fun x -> x.AddTodo t) (x, [Event.TodoAdded t]) with
                        | Ok _ -> [Event.TodoAdded t] |> Ok
                        | Error x -> x |> Error
                | RemoveTodo g ->
                    match
                        Cache.memoize (fun x -> x.RemoveTodo g) (x, [Event.TodoRemoved g]) with
                        | Ok _ -> [Event.TodoRemoved g] |> Ok
                        | Error x -> x |> Error

                // how we can deal with commands returning multiple events
                // | Add2Todos (t1, t2) ->
                //     let events = [Event.TodoAdded t1; TodoAdded t2]
                //     let evolved = fun () -> events |> evolve x
                //     match
                //         Cache.memoize (fun _ -> evolved()) (x, [Event.TodoAdded t1; TodoAdded t2] ) with
                //         | Ok _ -> events |>  Ok
                //         | Error x -> x |> Error