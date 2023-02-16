namespace BackEnd

open FSharp.Data.Sql
open Npgsql.FSharp
open FSharpPlus
open Shared
open Shared.Utils

type json = string

type StorageEvent =
    {
        Event: json
        Id: int
        Timestamp: System.DateTime
    }
type StorageSnapshot = {
    Id: int
    Snapshot: json
    TimeStamp: System.DateTime
    EventId: int
}
type IStorage =
    abstract member DeleteAllEvents: unit -> unit
    abstract member TryGetLastSnapshot: unit -> Option<int * int * json>
    abstract member TryGetLastEventId: unit -> Option<int>
    abstract member TryGetLastSnapshotEventId: unit -> Option<int>
    abstract member TryGetLastSnapshotId: unit -> Option<int>
    abstract member TryGetEvent: int -> Option<StorageEvent>
    abstract member SetSnapshot: int * string -> Result<unit, string>
    abstract member AddEvents: List<json> -> Result<unit, string>
    abstract member GetEventsAfterId: int -> List<int * string >

module DbStorage =
    let TPConnectionString = Conf.connectionString
    let ceResult = CeResultBuilder()
    type PgDb =
        new() = {}
        interface IStorage with
            member this.DeleteAllEvents() =
                if (Conf.isTestEnv) then
                    let _ =
                        TPConnectionString
                        |> Sql.connect
                        |> Sql.query "DELETE from snapshots"
                        |> Sql.executeNonQuery
                    let _ =
                        TPConnectionString
                        |> Sql.connect
                        |> Sql.query "DELETE from events"
                        |> Sql.executeNonQuery
                    ()
                else
                    failwith "operation allowed only in test db"

            member this.TryGetLastSnapshot() =
                TPConnectionString
                |> Sql.connect
                |> Sql.query "SELECT id, event_id, snapshot FROM snapshots ORDER BY id DESC LIMIT 1"
                |> Sql.executeAsync (fun read ->
                    (
                        read.int "id",
                        read.int "event_id",
                        read.text "snapshot"
                    )
                )
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> Seq.tryHead

            member this.TryGetLastSnapshotId() =
                TPConnectionString
                |> Sql.connect
                |> Sql.query "SELECT id FROM snapshots ORDER BY id DESC LIMIT 1"
                |> Sql.executeAsync (fun read ->
                    (
                        read.int "id"
                    )
                )
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> Seq.tryHead

            member this.TryGetLastEventId() =
                TPConnectionString
                |> Sql.connect
                |> Sql.query "SELECT id from events ORDER BY id DESC LIMIT 1"
                |> Sql.executeAsync  (fun read -> read.int "id")
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> Seq.tryHead

            member this.TryGetLastSnapshotEventId() =
                TPConnectionString
                |> Sql.connect
                |> Sql.query "SELECT event_id from snapshots ORDER BY id DESC LIMIT 1"
                |> Sql.executeAsync  (fun read -> read.int "event_id")
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> Seq.tryHead

            member this.TryGetEvent id =
                TPConnectionString
                |> Sql.connect
                |> Sql.query "SELECT * from events where id = @id"
                |> Sql.parameters ["id", Sql.int id]
                |> Sql.executeAsync
                    (
                        fun read ->
                        {
                            Id = read.int "id"
                            Event = read.string "event"
                            Timestamp = read.dateTime "timestamp"
                        }
                    )
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                    |> Seq.tryHead

            member this.SetSnapshot (id: int, snapshot: json) =
                ceResult
                    {
                        let! event = ((this :> IStorage).TryGetEvent id) |> optionToResult
                        let _ =
                            TPConnectionString
                            |> Sql.connect
                            |> Sql.executeTransactionAsync
                                [
                                    "INSERT INTO snapshots (event_id, snapshot, timestamp) VALUES (@event_id, @snapshot, @timestamp)",
                                        [
                                            [
                                                ("@event_id", Sql.int event.Id);
                                                ("snapshot",  Sql.jsonb snapshot);
                                                ("timestamp", Sql.timestamp event.Timestamp)
                                            ]
                                        ]
                                ]
                            |> Async.AwaitTask
                            |> Async.RunSynchronously
                        return ()
                    }

            member this.AddEvents (events: List<json>) =
                try
                    let _ =
                        TPConnectionString
                        |> Sql.connect
                        |> Sql.executeTransactionAsync
                            [
                                "INSERT INTO events (event, timestamp) VALUES (@event, @timestamp)",
                                events
                                |> List.map
                                    (
                                        fun x ->
                                            [
                                                ("@event", Sql.jsonb x);
                                                ("timestamp", Sql.timestamp (System.DateTime.Now))
                                            ]
                                    )
                            ]
                            |> Async.AwaitTask
                            |> Async.RunSynchronously
                    () |> Ok
                with
                    | _ as ex -> (ex.ToString()) |> Error

            member this.GetEventsAfterId id =
                TPConnectionString
                |> Sql.connect
                |> Sql.query "SELECT id, event FROM events WHERE id > @id ORDER BY id"
                |> Sql.parameters ["id", Sql.int id]
                |> Sql.executeAsync ( fun read ->
                    (
                        read.int "id",
                        read.text "event"
                    )
                )
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> Seq.toList
