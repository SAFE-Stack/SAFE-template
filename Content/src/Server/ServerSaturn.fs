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

let publicPath = "../Client/public" |> Path.GetFullPath
let port = 8085us

let getInitCounter () : Task<Counter> = task { return 42 }

let browserRouter = scope {
  get "/" (htmlFile (Path.Combine(publicPath, "index.html")))
}

#if (Remoting)
let server =
  { getInitCounter = getInitCounter >> Async.AwaitTask }

let webApp =
  remoting server {
    use_route_builder Route.builder
  }

let mainRouter = scope {
  forward "" browserRouter
  forward "" webApp
}
#else
let config (services:IServiceCollection) =
  let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
  fableJsonSettings.Converters.Add(Fable.JsonConverter())
  services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings) |> ignore
  services
let apiRouter = scope {
  get "/init" (fun next ctx ->
    task {
      let! counter = getInitCounter()
      return! Successful.OK counter next ctx
    })
}

let mainRouter = scope {
  forward "" browserRouter
  forward "/api" apiRouter
}
#endif

let app = application {
    router mainRouter
    url ("http://0.0.0.0:" + port.ToString() + "/")
    memory_cache
    use_static publicPath
    #if (!Remoting)
    service_config config
    #endif
    use_gzip
}

run app