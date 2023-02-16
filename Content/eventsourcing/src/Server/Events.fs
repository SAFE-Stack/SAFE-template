namespace BackEnd

open Shared
open System
open EventSourcing
open BackEnd.Aggregate
module Events =
    open Cache
    type Event =
        | TodoAdded of Todo
        | TodoRemoved of Guid
            interface Processable<Aggregate> with
                member this.Process (x: Aggregate ) =
                    match this with
                    | TodoAdded (t: Todo) ->
                        EventCache<Aggregate>.Instance.Memoize (fun () -> x.AddTodo t) (x, [TodoAdded t])
                    | TodoRemoved (g: Guid) ->
                        EventCache<Aggregate>.Instance.Memoize (fun () -> x.RemoveTodo g) (x, [TodoRemoved g])

