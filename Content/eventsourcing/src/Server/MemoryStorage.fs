namespace BackEnd

open FSharp.Data.Sql
open System.Runtime.CompilerServices
open FSharpPlus
open System

module MemoryStorage =
    open BackEnd

    let mutable event_id_seq = 1
    let mutable snapshot_id_seq = 1

    [<MethodImpl(MethodImplOptions.Synchronized)>]
    let next_event_id() =
        let result = event_id_seq
        event_id_seq <- event_id_seq+1
        result

    [<MethodImpl(MethodImplOptions.Synchronized)>]
    let next_snapshot_id() =
        let result = snapshot_id_seq
        snapshot_id_seq <- snapshot_id_seq+1
        result

    let mutable events:List<StorageEvent> = []
    let mutable snapshots:List<StorageSnapshot> = []

    type MemoryStorage =
        new() =  {}
        interface IStorage with
            member this.DeleteAllEvents() =
                events <- []
                snapshots <- []
                event_id_seq <- 1
                snapshot_id_seq <- 1
            member this.TryGetLastSnapshot() =
                snapshots |> List.tryLast |>> (fun x -> (x.Id, x.EventId, x.Snapshot))
            member this.TryGetLastEventId() =
                events |> List.tryLast |>> (fun x -> x.Id)
            member this.TryGetLastSnapshotEventId() =
                snapshots |> List.tryLast |>> (fun x -> x.EventId)
            member this.TryGetLastSnapshotId() =
                snapshots |> List.tryLast |>> (fun x -> x.Id)
            member this.TryGetEvent(id: int) =
                events |> List.tryFind (fun x -> x.Id = id)
            member this.AddEvents (events') =
                let events'' =
                    [
                        for e in events' do
                            yield {
                                Id = next_event_id()
                                Event = e
                                Timestamp = DateTime.Now
                            }
                    ]
                events <- events@events''
                () |> Result.Ok
            member this.GetEventsAfterId (id) =
                events |> List.filter (fun x -> x.Id > id) |>> fun x -> x.Id, x.Event
            member this.SetSnapshot (id, snapshot) =
                let newSnapshot =
                    {
                        Id = next_snapshot_id()
                        Snapshot = snapshot
                        TimeStamp = DateTime.Now
                        EventId = id
                    }
                snapshots <- snapshots@[newSnapshot]
                () |> Ok







