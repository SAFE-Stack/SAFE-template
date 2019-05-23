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

let getInitCounter () : Task<Counter> = task { return { Value = 42 } }
#if (bridge)
/// Connect the Elmish functions to an endpoint for websocket connections
let socketApp =
    Bridge.mkServer Socket.clock Bridge.init Bridge.update
    |> Bridge.withSubscription Bridge.timer
    |> Bridge.run Giraffe.server
#endif


#if (remoting)

let counterApi = {
    initialCounter = getInitCounter >> Async.AwaitTask
}
#if (bridge)
let apiApp =
#else
let webApp =
#endif
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue counterApi
    |> Remoting.buildHttpHandler

#else
#if (bridge)
let apiApp =
#else
let webApp =
#endif
    route "/api/init" >=>
        fun next ctx ->
            task {
                let! counter = getInitCounter()
                return! json counter next ctx
            }
#endif
#if (bridge)
/// Uses the `choose` function to support multiple endpoints
let webApp = choose [ apiApp; socketApp ]
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
