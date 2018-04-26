open System.IO
open System.Net

open Suave
open Suave.Operators

#if (Remoting)
open Fable.Remoting.Server
open Fable.Remoting.Suave
#endif

open Shared

let publicPath =
    match System.Environment.GetEnvironmentVariable "public_path" with
    | null | "" -> "../Client/public"
    | path -> path
    |> Path.GetFullPath
let port = 8085us

let config =
  { defaultConfig with 
      homeFolder = Some publicPath
      bindings = [ HttpBinding.create HTTP (IPAddress.Parse "0.0.0.0") port ] }

let getInitCounter () : Async<Counter> = async { return 42 }

let init : WebPart = 
#if (Remoting)
  let counterProcotol = 
    { getInitCounter = getInitCounter }
  // Create a WebPart for the given implementation of the protocol
  remoting counterProcotol {
    // define how routes are mapped
    use_route_builder Route.builder 
  }
#else
  Filters.path "/api/init" >=>
  fun ctx ->
    async {
      let! counter = getInitCounter()
      return! Successful.OK (string counter) ctx
    }
#endif

let webPart =
  choose [
    init
    Filters.path "/" >=> Files.browseFileHome "index.html"
    Files.browseHome
    RequestErrors.NOT_FOUND "Not found!"
  ]

startWebServer config webPart