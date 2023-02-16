namespace Shared
open System
open System.Runtime.CompilerServices

open FSharp.Core

open EventSourcing

open FSharpPlus
open FSharpPlus.Data

module Cache =
    // caching events is fine with global vars whereas caching aggregate required embed them in a singleton (not sure why)
    let dic = Collections.Generic.Dictionary<'H * List<Processable<'H>>, Result<'H, string>>()
    let queue = Collections.Generic.Queue<'H * List<Processable<'H>>>()
    [<MethodImpl(MethodImplOptions.Synchronized)>]
    let tryAddToDictionary (arg, res) =
        try
            dic.Add(arg, res)
            queue.Enqueue arg
            if (queue.Count > Conf.cacheSize) then
                let removed = queue.Dequeue()
                dic.Remove removed |> ignore
            ()
        with :? _ as e -> printf "warning: cache is doing something wrong %A\n" e

    let memoize (f: 'H -> Result<'H, string>) (arg: 'H * List<Processable<'H>>) =
        if (dic.ContainsKey arg) then
            dic.[arg]
        else
            let res = arg |> fst |> f
            tryAddToDictionary(arg, res)
            res

module Todos =
    open Shared
    open Utils

    type Todos =
        {
            todos: List<Todo>
        }
        with
            static member Zero =
                {
                    todos = []
                }
            member this.AddTodo (t: Todo) =
                ceResult {
                    let! mustNotExist =
                        this.todos
                        |> List.exists (fun x -> x.Description = t.Description)
                        |> not
                        |> boolToResult
                    let result =
                        {
                            this with
                                todos = t::this.todos
                        }
                    return result
                }
            member this.RemoveTodo (id: Guid) =
                ceResult {
                    let! mustExist =
                        this.todos
                        |> List.exists (fun x -> x.Id = id)
                        |> boolToResult
                    let result =
                        {
                            this with
                                todos = this.todos |> List.filter (fun x -> x.Id <> id)
                        }
                    return result
                }
            member this.GetTodos() = this.todos

module TodoEvents =
    open Todos
    type Event =
        | TodoAdded of Todo
        | TodoRemoved of Guid
            interface Processable<Todos> with
                member this.Process (x: Todos ) =
                    match this with
                    | TodoAdded (t: Todo) ->
                        Cache.memoize (fun (x: Todos) -> x.AddTodo t) (x, [this])
                    | TodoRemoved (g: Guid) ->
                        Cache.memoize (fun (x: Todos) -> x.RemoveTodo g) (x, [this])
