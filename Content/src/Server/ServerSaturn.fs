open System.IO
open System.Net
open System.Threading.Tasks

open Giraffe
open Giraffe.Core
open Giraffe.ResponseWriters

#if (Remoting)
open Fable.Remoting.Saturn
#endif

open Saturn

open Shared

let clientPath = Path.Combine("..","Client") |> Path.GetFullPath
let port = 8085us

let getInitCounter () : Task<Counter> = task { return 42 }

let browserRouter = scope {
  get "/" (htmlFile (Path.Combine(clientPath, "/index.html")))
}

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

let app = application {
    router mainRouter
    url ("http://0.0.0.0:" + port.ToString() + "/")
    memory_cache 
    use_static clientPath
    use_gzip
}

run app