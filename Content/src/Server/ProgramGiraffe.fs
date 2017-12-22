open System
open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting

open Giraffe

open Fable.Remoting.Giraffe

open Shared

let clientPath = Path.Combine("..","Client") |> Path.GetFullPath
let port = 8085us

let getInitCounter () : Task<Counter> = task { return 42 }

let webApp : HttpHandler =
#if (Remoting)
  route "/api/init" >=>
    fun next ctx ->
      task {
        let! counter = getInitCounter()
        return! Successful.OK counter next ctx
      }
#else
  let counterProcotol = 
    { getInitCounter = getInitCounter >> Async.AwaitTask }
  // creates a WebPart for the given implementation
  FableGiraffeAdapter.httpHandlerWithBuilderFor counterProcotol Route.builder
#endif

let configureApp  (app : IApplicationBuilder) =
  app.UseStaticFiles()
     .UseGiraffe webApp

WebHost
  .CreateDefaultBuilder()
  .UseWebRoot(clientPath)
  .UseContentRoot(clientPath)
  .Configure(Action<IApplicationBuilder> configureApp)
  .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
  .Build()
  .Run()