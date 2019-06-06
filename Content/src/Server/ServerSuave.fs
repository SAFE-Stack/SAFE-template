open System.IO
//#if (deploy == "iis")
open System
open System.Reflection
//#endif
open System.Net

open Shared

open Suave
open Suave.Files
open Suave.Successful
open Suave.Filters
open Suave.Operators

#if (bridge)
open Elmish
open Elmish.Bridge
#endif


#if (remoting)
open Fable.Remoting.Server
open Fable.Remoting.Suave

#else
open Thoth.Json.Net
open System.Web

#endif
#if (deploy == "azure")
open Microsoft.WindowsAzure.Storage

#endif
//#if (deploy == "azure")
let publicPath = Azure.tryGetEnv "public_path" |> Option.defaultValue "../Client/public" |> Path.GetFullPath
let port = Azure.tryGetEnv "HTTP_PLATFORM_PORT" |> Option.map System.UInt16.Parse |> Option.defaultValue 8085us
let storageAccount = Azure.tryGetEnv "STORAGE_CONNECTIONSTRING" |> Option.defaultValue "UseDevelopmentStorage=true" |> CloudStorageAccount.Parse
//#elseif (deploy == "iis")
module ServerPath =
    let workingDirectory =
        let currentAsm = Assembly.GetExecutingAssembly()
        let codeBaseLoc = currentAsm.CodeBase
        let localPath = Uri(codeBaseLoc).LocalPath
        Directory.GetParent(localPath).FullName

    let resolve segments =
        let paths = Array.concat [| [| workingDirectory |]; Array.ofList segments |]
        Path.GetFullPath(Path.Combine(paths))

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x
let publicPath = ServerPath.resolve [".."; "Client"; "public"]
let port = tryGetEnv "HTTP_PLATFORM_PORT" |> Option.map System.UInt16.Parse |> Option.defaultValue 8085us
//#else
let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x
let publicPath = Path.GetFullPath "../Client/public"
let port =
//#if (deploy == "heroku")
    "PORT"
//#else
    "SERVER_PORT"
//#endif
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us
//#endif

let config =
    { defaultConfig with
          homeFolder = Some publicPath
          bindings = [ HttpBinding.create HTTP (IPAddress.Parse "0.0.0.0") port ] }

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
let webApi =
    Bridge.mkServer "/socket/init" init update
    |> Bridge.run Suave.server

#elseif (remoting)
let counterApi = {
    initialCounter = fun () -> async { return {Value = 42} }
}

let webApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue counterApi
    |> Remoting.buildWebPart
#else
let webApi =
    path "/api/init" >=>
        fun ctx ->
            async {
                let counter = { Value = 42 }
                return! OK (Encode.Auto.toString(4, counter)) ctx
            }
#endif
let webApp =
    choose [
        webApi
        path "/" >=> browseFileHome "index.html"
        browseHome
        RequestErrors.NOT_FOUND "Not found!"
#if (deploy == "azure")
    ] |> Azure.AI.withAppInsights Azure.AI.buildApiOperationName

Azure.AppServices.addTraceListeners()
Azure.tryGetEnv "APPINSIGHTS_INSTRUMENTATIONKEY"
|> Option.iter(fun appInsightsKey ->
    Azure.AI.configure { AppInsightsKey = appInsightsKey; DeveloperMode = false; TrackDependencies = true })
#else
    ]
#endif

startWebServer config webApp