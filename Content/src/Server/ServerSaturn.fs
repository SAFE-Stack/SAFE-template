open System.IO
open System.Threading.Tasks

open Giraffe
open Saturn

#if (Remoting)
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
#endif

open Shared

let clientPath = Path.Combine("..","Client") |> Path.GetFullPath
let port = 8085us

let getInitCounter () : Task<Counter> = task { return 42 }

let browserRouter = scope {
  get "/" (htmlFile (Path.Combine(clientPath, "index.html")))
}

#if (Remoting)
let server =
  { getInitCounter = getInitCounter >> Async.AwaitTask }

let webApp =
  remoting server {
    with_builder Route.builder
  }

let mainRouter = scope {
  forward "" browserRouter
  forward "" webApp
}
#else
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
    use_static clientPath
    use_gzip
}

run app