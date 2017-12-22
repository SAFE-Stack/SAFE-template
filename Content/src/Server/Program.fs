open System
open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting

open Giraffe
open Giraffe.HttpStatusCodeHandlers.Successful

open Shared

let clientPath = Path.Combine("..","Client") |> Path.GetFullPath
let port = 8085us

let getInitCounter () : Task<Counter> = task { return 42 }

let webApp =
  route "/api/init" >=>
    fun next ctx ->
      task {
        let! counter = getInitCounter()
        return! OK counter next ctx
      }

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