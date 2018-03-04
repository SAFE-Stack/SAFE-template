open System.IO
open System.Net

open Saturn
open Giraffe.Core
open Giraffe.ResponseWriters

#if (Remoting)
open Fable.Remoting.Saturn
#endif

open Shared


let clientPath = Path.Combine("..","Client") |> Path.GetFullPath
let port = 8085us

module Router =

  let browser = pipeline {
      plug acceptHtml
      plug putSecureBrowserHeaders
      plug fetchSession
      set_header "x-pipeline-type" "Browser"
  }

  let defaultView = scope {
      get "/" (htmlFile (Path.Combine(clientPath, "/index.html")))
  }

  let browserRouter = scope {
      pipe_through browser

      forward "" defaultView
  }

  let router = scope {
      forward "" browserRouter
  }

let app = application {
    router Router.router
    url ("http://0.0.0.0:" + port.ToString() + "/")
    memory_cache 
    use_static clientPath
    use_gzip
}

run app
