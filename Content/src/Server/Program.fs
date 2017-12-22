open System
open System.IO

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting

open Giraffe

let clientPath = Path.Combine("..","Client") |> Path.GetFullPath
let port = 8085us

let webApp = text "hello world"

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