open System.IO

open Suave

let path = Path.Combine("src","Client") |> Path.GetFullPath

let config =
  { defaultConfig with homeFolder = Some path }

startWebServer config Files.browseHome