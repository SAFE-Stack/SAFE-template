
namespace BackEnd

open System.Runtime.CompilerServices
open System
open FSharp.Data.Sql
open FSharp.Core
open FSharpPlus
open FSharpPlus.Data
open Shared
open Shared.Utils
open Shared.EventSourcing
open BackEnd.Cache
open Newtonsoft.Json

module CacheRepository =
    type RepoCache<'H> private () =
        let dic = Collections.Generic.Dictionary<int, Result<'H, string>>()
        let queue = Collections.Generic.Queue<int>()
        static let instance = RepoCache<'H>()
        static member Instance = instance
        [<MethodImpl(MethodImplOptions.Synchronized)>]
        member private this.TryAddToDictionary(arg, res) =
            try
                dic.Add(arg, res)
                queue.Enqueue arg
                if (queue.Count > 1) then
                    let removed = queue.Dequeue()
                    dic.Remove removed |> ignore
                ()
            with :? _ as e -> printf "warning: cache is doing something wrong %A\n" e
        member this.Memoize (f: unit -> Result<'H, string>) (arg: int) =
            if (dic.ContainsKey arg) then
                dic.[arg]
            else
                let res = f()
                this.TryAddToDictionary(arg, res)
                res

module Repository =
    open CacheRepository
    let storage: IStorage =
        match Conf.storageType with
            | Conf.StorageType.Memory -> MemoryStorage.MemoryStorage()
            | Conf.StorageType.Postgres -> DbStorage.PgDb()

    let ceResult = CeResultBuilder()

    let getLastSnapshot<'H> (zero: 'H) =
        ceResult {
            let! result =
                // toto: memoize this could be a good idea
                match storage.TryGetLastSnapshot()  with
                | Some (id, eventId, json) ->
                    let state = RepoCache<'H>.Instance.Memoize(fun () -> json |> deserialize<'H>) id
                    match state with
                    | Error e -> Error e
                    | _ -> (eventId, state |> Result.get) |> Ok
                | None -> (0, zero) |> Ok
            return result
        }

    let getState<'H, 'E when 'E :> Processable<'H>> (zero: 'H) =
        ceResult {
            let! (id, state) = getLastSnapshot<'H> (zero)
            let events = storage.GetEventsAfterId id
            let lastId =
                match events.Length with
                | x when x > 0 -> events |> List.last |> fst
                | _ -> id
            let! events' =
                events |>> snd |> catchErrors deserialize<'E>
            let! result =
                events' |> evolve<'H, 'E> state
            return (lastId, result)
        }

    [<MethodImpl(MethodImplOptions.Synchronized)>]
    let inline runCommand<'H, ^E when ^E :> Processable<'H>> (zero: 'H) (command: Executable<'H, ^E>)  =
        ceResult {
            let! (_, state) = getState<'H, ^E> (zero)
            let! events =
                state
                |> command.Execute
            let! eventsAdded =
                storage.AddEvents (events |>> JsonConvert.SerializeObject)
            return eventsAdded
        }

    let mksnapshot<'H, 'E when 'E :> Processable<'H>> (zero: 'H) =
        ceResult
            {
                let! (id, state) = getState<'H, 'E> (zero: 'H)
                let snapshot = JsonConvert.SerializeObject(state, Utils.serSettings)
                let! result = storage.SetSnapshot (id, snapshot)
                return result
            }

    let mksnapshotIfInterval<'H, 'E when 'E :> Processable<'H>> (zero: 'H) =
        ceResult
            {
                let! lastEventId = storage.TryGetLastEventId() |> optionToResult
                let snapEventId = storage.TryGetLastSnapshotEventId() |> optionToDefault 0

                let! result =
                    if ((lastEventId - snapEventId) > Conf.intervalBetweenSnapshots || snapEventId = 0) then
                        mksnapshot<'H, 'E> (zero)
                    else
                        () |> Ok
                return result
            }

