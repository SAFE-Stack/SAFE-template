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
#if (bridge)
open Elmish
open Elmish.Bridge
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

let port =
//#if (deploy == "heroku")
    "PORT"
//#else
    "SERVER_PORT"
//#endif
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

#if (bridge)
/// Elmish init function with a channel for sending client messages
/// Returns a new state and commands
let init clientDispatch () =
    let value = { Value = 42 }
    clientDispatch (SyncCounter value)
    value, Cmd.none

/// Elmish update function with a channel for sending client messages
/// Returns a new state and commands
let update clientDispatch msg model =
    match msg with
    | Increment ->
        let newModel = { model with Value = model.Value + 1 }
        clientDispatch (SyncCounter newModel)
        newModel, Cmd.none
    | Decrement ->
        let newModel = { model with Value = model.Value - 1 }
        clientDispatch (SyncCounter newModel)
        newModel, Cmd.none

/// Connect the Elmish functions to an endpoint for websocket connections
let webApp =
    Bridge.mkServer "/socket/init" init update
    |> Bridge.run Giraffe.server

#elseif (remoting)
let counterApi = {
    initialCounter = fun () -> async { return { Value = 42 } }
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
            let counter = {Value = 42}
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
    #if (bridge)
    app_config Giraffe.useWebSockets
    #endif
    #if (deploy == "azure")
    service_config configureAzure
    #endif
    #if (deploy == "iis")
    use_iis
    #endif
    use_gzip
}

run app
