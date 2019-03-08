#if (remoting)
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
#endif
#if (deploy == "azure")
open FSharp.Azure.StorageTypeProvider
#endif
open FSharp.Control.Tasks.V2
open Giraffe
#if (deploy == "azure")
open global.Shared
#endif
open Microsoft.Extensions.DependencyInjection
open Saturn
#if (deploy != "azure")
open Shared
#endif
open System.IO
#if (deploy == "azure")
open Thoth.Json.Net
#endif

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

//#if (deploy == "azure")
let publicPath = tryGetEnv "public_path" |> Option.defaultValue "../Client/public" |> Path.GetFullPath
let runtimeAzure = tryGetEnv "STORAGE_CONNECTIONSTRING" |> Option.defaultValue "UseDevelopmentStorage=true"

/// A handle to an Azure storage account without a connection string. Schema is provided by the
/// azure-schema.json file. You can remove the blobSchema key/value and replace with a full Azure
/// connection string; schema will be inferred from the storage account contents directly.
type Azure = AzureTypeProvider<blobSchema="azure-schema.json">
#else
let publicPath = Path.GetFullPath "../Client/public"
#endif

let port = "SERVER_PORT" |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let getInitCounter() = task { return { Value = 42 } }

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
#if (!remoting)
let configureSerialization (services:IServiceCollection) =
    services.AddSingleton<Giraffe.Serialization.Json.IJsonSerializer>(Thoth.Json.Giraffe.ThothSerializer())

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
    service_config configureSerialization
    #endif
    #if (deploy == "azure")
    service_config configureAzure
    #endif
    use_gzip
}

run app
