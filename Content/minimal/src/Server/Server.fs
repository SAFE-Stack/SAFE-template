module Server

open Giraffe
open Saturn

open Shared

let webApp =
    router {
        get Route.hello (text "Hello from SAFE!")
    }

let app =
    application {
        url "http://*:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
