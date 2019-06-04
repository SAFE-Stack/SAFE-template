open System
open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection

open FSharp.Control.Tasks.V2
open Giraffe
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
let webApp =
    route "/api/init" >=>
        fun next ctx ->
            task {
                let counter = { Value = 42 }
                return! json counter next ctx
            }
#endif

let configureApp (app : IApplicationBuilder) =
    app.UseDefaultFiles()
       .UseStaticFiles()
#if (bridge)
       .UseWebSockets()
#endif
       .UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore
    #if (!remoting)
    services.AddSingleton<Giraffe.Serialization.Json.IJsonSerializer>(Thoth.Json.Giraffe.ThothSerializer()) |> ignore
    #endif
    #if (deploy == "azure")
    tryGetEnv "APPINSIGHTS_INSTRUMENTATIONKEY" |> Option.iter (services.AddApplicationInsightsTelemetry >> ignore)
    #endif

WebHost
    .CreateDefaultBuilder()
    .UseWebRoot(publicPath)
    .UseContentRoot(publicPath)
    .Configure(Action<IApplicationBuilder> configureApp)
    .ConfigureServices(configureServices)
    #if (deploy == "iis")
    .UseIISIntegration()
    #endif
    .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
    .Build()
    .Run()
