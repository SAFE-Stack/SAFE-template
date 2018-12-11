open System.IO
open System.Net

open Shared

open Suave
open Suave.Files
open Suave.Successful
open Suave.Filters
open Suave.Operators

#if (remoting)
open Fable.Remoting.Server
open Fable.Remoting.Suave

#else
open Thoth.Json.Net

#endif
#if (deploy == "azure")
open Microsoft.WindowsAzure.Storage

#endif
//#if (deploy == "azure")
let publicPath = Azure.tryGetEnv "public_path" |> Option.defaultValue "../Client/public" |> Path.GetFullPath
let port = Azure.tryGetEnv "HTTP_PLATFORM_PORT" |> Option.map System.UInt16.Parse |> Option.defaultValue 8085us
let storageAccount = Azure.tryGetEnv "STORAGE_CONNECTIONSTRING" |> Option.defaultValue "UseDevelopmentStorage=true" |> CloudStorageAccount.Parse
//#else
let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port = "SERVER_PROXY_PORT" |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us
//#endif

let config =
    { defaultConfig with
          homeFolder = Some publicPath
          bindings = [ HttpBinding.create HTTP (IPAddress.Parse "0.0.0.0") port ] }

let getInitCounter() : Async<Counter> = async { return { Value = 42 } }
#if (remoting)
let counterApi = {
    initialCounter = getInitCounter
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
                let! counter = getInitCounter()
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