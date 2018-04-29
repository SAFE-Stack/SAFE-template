module Suave.Azure

open System
open System.Diagnostics

/// Starts all trace listeners for the Azure App Service.
let addAzureAppServicesTraceListeners() =
    let azureTraceListenerTypeNames =
        [ "Drive"; "Table"; "Blob" ]
        |> List.map (sprintf "Microsoft.WindowsAzure.WebSites.Diagnostics.Azure%sTraceListener, Microsoft.WindowsAzure.WebSites.Diagnostics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")

    for traceListenerTypeName in azureTraceListenerTypeNames do
        match Type.GetType(traceListenerTypeName, throwOnError = false) |> Option.ofObj with
        | Some traceListenerType ->
            try
            let listener = Activator.CreateInstance traceListenerType :?> TraceListener
            listener.Name <- traceListenerType.Name
            Trace.Listeners.Add listener |> ignore
            with _ -> ()
        | None -> ()

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function | null | "" -> None | v -> Some v
        