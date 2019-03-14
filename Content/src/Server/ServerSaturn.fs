open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2
open Giraffe
open Saturn
open Shared

#if (remoting)
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
#endif
#if (deploy == "azure")
open Microsoft.WindowsAzure.Storage
#endif

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

//#if (deploy == "azure")
let publicPath = tryGetEnv "public_path" |> Option.defaultValue "../Client/public" |> Path.GetFullPath
let storageAccount = tryGetEnv "STORAGE_CONNECTIONSTRING" |> Option.defaultValue "UseDevelopmentStorage=true" |> CloudStorageAccount.Parse
//#else
let publicPath = Path.GetFullPath "../Client/public"
//#endif

let port = "SERVER_PORT" |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let getInitCounter() : Task<Counter> = task { return { Value = 42 } }

#if (remoting)
let counterApi = {
    initialCounter = getInitCounter >> Async.AwaitTask
}

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue counterApi
    |> Remoting.buildHttpHandler

#else
let webApp = router {
    get "/api/init" (fun next ctx ->
        task {
            let! counter = getInitCounter()
            return! json counter next ctx
        })
}

#endif
#if (deploy == "azure")
let configureAzure (services:IServiceCollection) =
    tryGetEnv "APPINSIGHTS_INSTRUMENTATIONKEY"
    |> Option.map services.AddApplicationInsightsTelemetry
    |> Option.defaultValue services

#endif
let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    use_router webApp
    memory_cache
    use_static publicPath
    #if (!remoting)
    use_json_serializer(Thoth.Json.Giraffe.ThothSerializer())
    #endif
    #if (deploy == "azure")
    service_config configureAzure
    #endif
    use_gzip
}

run app
