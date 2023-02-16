
namespace BackEnd

open Shared
open FSharpPlus
open System

module Todos =
    open Utils

    type Model =
        abstract member AddTodo: Todo -> Result<Model, string>
        abstract member RemoveTodo: Guid -> Result<Model, string>

    let ceResult = CeResultBuilder()

    type Stat =
        {
            timeAdded: Map<Guid, DateTime>
            timeRemoved: Map<Guid, DateTime>
        }
        static member Zero =
            {
                timeAdded = [] |> Map.ofList
                timeRemoved = [] |> Map.ofList
            }

        member private this.Purge() =
            let olds =
                this.timeRemoved
                |> Map.toList
                |>>
                    fun (x, y)
                        ->
                            if (DateTime.Now.Subtract(y).CompareTo(TimeSpan(10,0,0,0))>0) then
                                x |> Some
                            else
                                None
                |>
                List.fold
                    (fun acc x ->
                        if (x.IsSome) then (x.Value::acc) else acc
                    )
                    []
            let timeAddedPurged =
                this.timeAdded
                |> Map.toList
                |> List.filter
                    (fun (x, _) -> olds |> List.contains x)
                |> Map.ofList

            let timeRemovedPurged =
                this.timeRemoved
                |> Map.toList
                |> List.filter
                    (fun (x, _) -> olds |> List.contains x)
                |> Map.ofList
            {
                    timeAdded = timeAddedPurged
                    timeRemoved = timeRemovedPurged
            }

        interface Model with
            member this.AddTodo(todo) =
                {
                    this with
                        timeAdded = this.timeAdded.Add(todo.Id, DateTime.Now)
                }
                :> Model
                |> Ok
            member this.RemoveTodo(id) =
                {
                    this with
                        timeRemoved = this.timeRemoved.Add(id, DateTime.Now)
                }
                :> Model
                |> Ok

        member this.AverageTodoTime() =
            let addedAndFinished =
                this.timeAdded.Keys |> Set.ofSeq |> Set.intersect (this.timeRemoved.Keys |> Set.ofSeq)
            let times =
                addedAndFinished
                |>>
                (fun x ->
                    let startedAt = this.timeAdded.[x]
                    let finishedAt = this.timeRemoved.[x]
                    let interval = finishedAt.Subtract(startedAt)
                    interval.Milliseconds
                )
            let total =
                times
                |> Set.fold (fun x y -> x + y) 0
            let average = (double)total/(double)(addedAndFinished.Count)
            average

    type Todos =
        {
            todos: List<Todo>
        }
        with
            static member Zero =
                {
                    todos = []
                }
            interface Model with
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
