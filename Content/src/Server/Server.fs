open System.IO

open FSharp.Control.Tasks.V2
open Giraffe
open Saturn

open Shared

open SAFE.Server

let publicPath = Environment.getEnvVar "public_path" "../Client/public" |> Path.GetFullPath
let port = Environment.getEnvVar "PORT" "8085" |> uint16

let webApp =
    router {
        get "/api/init" (fun next ctx ->
            task {
                let counter = { Value = 42 }
                return! json counter next ctx
            }) }

let app =
    application {
        url ("http://0.0.0.0:" + port.ToString() + "/")
        use_router webApp
        memory_cache
        use_static publicPath
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
        use_gzip
    }

run app
