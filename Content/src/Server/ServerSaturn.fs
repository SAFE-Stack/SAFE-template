open System.IO
open System.Threading.Tasks

open Giraffe
open Saturn

#if (Remoting)
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
#else
open Giraffe.Serialization
open Microsoft.Extensions.DependencyInjection
#endif

open Shared
open Microsoft.AspNetCore.Builder

let publicPath =
    match System.Environment.GetEnvironmentVariable "public_path" with
    | null | "" -> "../Client/public"
    | path -> path
    |> Path.GetFullPath
let port = 8085us

let getInitCounter () : Task<Counter> = task { return 42 }

#if (Remoting)
let webApp =
  let server =
    { getInitCounter = getInitCounter >> Async.AwaitTask }
  remoting server {
    use_route_builder Route.builder
  }
#else
let webApp = scope {
  get "/init" (fun next ctx ->
    task {
      let! counter = getInitCounter()
      return! Successful.OK counter next ctx
    })
}
#endif

let mainRouter = scope {
  forward "/api" webApp
}

#if (!Remoting)
let configureSerialization (services:IServiceCollection) =
  let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
  fableJsonSettings.Converters.Add(Fable.JsonConverter())
  services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings)
#endif

#if (Deploy == "azure")
let configureAzure (services:IServiceCollection) =
  services.AddApplicationInsightsTelemetry(System.Environment.GetEnvironmentVariable "APPINSIGHTS_INSTRUMENTATIONKEY")
#endif

let configureApp (app:IApplicationBuilder) =
  app.UseDefaultFiles()

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    router mainRouter
    app_config configureApp
    memory_cache
    use_static publicPath
    #if (!Remoting)
    service_config configureSerialization
    #endif
    #if (Deploy == "azure")
    service_config configureAzure
    #endif
    use_gzip
}

run app