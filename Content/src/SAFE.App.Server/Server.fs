module SAFE.App.Server

(*#if (minimal)
open FSharp.Control.Tasks.V2
open Giraffe
#else*)
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
//#endif
open Saturn
open SAFE.App.Shared

(*#if (minimal)
let webApp =
    router {
        get "/api/init" (json { Value = 42 })
    }
#else*)
let counterApi =
    { getInitialCounter = async { return { Value = 42 } } }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue counterApi
    |> Remoting.buildHttpHandler
//#endif

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
(*#if (minimal)
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
#else
#endif*)
        use_gzip
    }

run app