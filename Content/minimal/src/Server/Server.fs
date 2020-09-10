module Server

open Giraffe
open Saturn

open Shared

let webApp =
    router {
        get Route.hello (json "Hello from SAFE!")
    }

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
        use_gzip
    }

run app
