namespace BackEnd

open Shared
open Shared.EventSourcing
open System.Runtime.CompilerServices

// don't:
// open System.Collections.Generic
open System
open FSharp.Core

module Cache =
    type EventCache<'H when 'H: equality> private () =
        let dic = Collections.Generic.Dictionary<'H * List<Processable<'H>>, Result<'H, string>>()
        let queue = Collections.Generic.Queue<'H * List<Processable<'H>>>()
        static let instance = EventCache<'H>()
        static member Instance = instance
        [<MethodImpl(MethodImplOptions.Synchronized)>]
        member private this.TryAddToDictionary (arg, res) =
            try
                dic.Add(arg, res)
                queue.Enqueue arg
                if (queue.Count > Conf.cacheSize) then
                    let removed = queue.Dequeue()
                    dic.Remove removed |> ignore
                ()
            with :? _ as e -> printf "warning: cache is doing something wrong %A\n" e

         member this.Memoize (f: unit -> Result<'H, string>) (arg: 'H * List<Processable<'H>>) =
            if (dic.ContainsKey arg) then
                dic.[arg]
            else
                let res = f()
                this.TryAddToDictionary(arg, res)
                res

    type SnapCache<'H> private () =
        let dic = Collections.Generic.Dictionary<int, Result<'H, string>>()
        let queue = Collections.Generic.Queue<int>()
        static let instance = SnapCache<'H>()
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