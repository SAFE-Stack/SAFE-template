namespace BackEnd
open BackEnd.Aggregate

open Shared
// open Shared.Todos
// open Shared.Commands
// open Shared.TodoEvents
open Shared.EventSourcing

module App =
    open Repository
    open Todos
    open Events
    open Commands

    let getAllTodos() =
        ceResult {
            let! (_, state) = getState<Aggregate, Event> Aggregate.Zero
            let todos = (state.todos :?> Todos).GetTodos()
            return todos
        }
    let AverageTime() =
        ceResult {
            let! (_, state) = getState<Aggregate, Event> Aggregate.Zero
            let averageTime = (state.projection :?> Stat).AverageTodoTime()
            return (int) averageTime
        }
    let addTodo todo =
        ceResult {
            let! _ =
                todo
                |> Command.AddTodo
                |> runCommand<Aggregate, Event> (Aggregate.Zero)
            return! mksnapshotIfInterval<Aggregate, Event> Aggregate.Zero
        }
    let removeTodo id =
        ceResult {
            let! _ =
                id
                |> Command.RemoveTodo
                |> runCommand<Aggregate, Event> Aggregate.Zero
            return! mksnapshotIfInterval<Aggregate, Event> Aggregate.Zero
        }