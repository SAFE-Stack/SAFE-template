module Suave.Azure

open System
open System.Diagnostics

let tryGetEnv = Environment.GetEnvironmentVariable >> function | null | "" -> None | v -> Some v

module AppServices =
    /// Starts all trace listeners for the Azure App Service.
    let addTraceListeners() =
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

module AI =
    open Microsoft.ApplicationInsights
    open Microsoft.ApplicationInsights.DataContracts
    open Microsoft.ApplicationInsights.DependencyCollector
    open Microsoft.ApplicationInsights.Extensibility
    open Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse

    /// The global Telemetry Client that handles all AI requests.
    let telemetryClient = TelemetryClient()

    /// Builds an operation name from a typical "/api/" endpoint.
    let buildApiOperationName (uri:Uri) (meth:HttpMethod) =
        let meth = meth.ToString()
        if uri.AbsolutePath.StartsWith "/api/" && uri.Segments.Length > 2
        then meth + " /api/" + uri.Segments.[2]
        else meth + " " + uri.AbsolutePath

    /// Tracks a web part request with App Insights.
    let withAppInsights buildOperationName (webPart:WebPart) context =
        // Start recording a new operation.
        let operation = telemetryClient.StartOperation<RequestTelemetry>(operationName = buildOperationName context.request.url context.request.``method``)

        // Set basic AI details
        operation.Telemetry.Url <- context.request.url

        async {                
            try
                try
                    // Execute the webpart
                    let! context = webPart context
                
                    // Set response code + success
                    context
                    |> Option.iter(fun context ->
                        operation.Telemetry.ResponseCode <- context.response.status.code.ToString()
                        operation.Telemetry.Success <- Nullable (int context.response.status.code < 400))
                
                    return context
                with ex ->
                    // Hoppla! Fail the request
                    operation.Telemetry.ResponseCode <- "500"
                    operation.Telemetry.Success <- Nullable false

                    // log the error
                    let telemetry = ExceptionTelemetry(ex)
                    telemetryClient.TrackException telemetry

                    raise ex
                    return None
            finally
                telemetryClient.StopOperation operation
        }

    /// Configuration for setting up App Insights
    type AIConfiguration =
        { /// The Application Insights key
          AppInsightsKey : string
          /// Whether to use Developer Mode with AI - will send more frequent messages at cost of higher CPU.
          DeveloperMode : bool
          /// Track external dependencies e.g. SQL, HTTP etc.
          TrackDependencies : bool }

    /// Configures the App Insights client.
    let configure configuration =
        let config = TelemetryConfiguration.Active
        config.TelemetryChannel.DeveloperMode <- Nullable configuration.DeveloperMode
        config.InstrumentationKey <- configuration.AppInsightsKey

        // Turn on Live Stream
        let mutable processor:QuickPulseTelemetryProcessor = null
        config
            .TelemetryProcessorChainBuilder
            .Use(fun next ->
                processor <- QuickPulseTelemetryProcessor next
                processor :> _)
            .Build()
        let quickPulse = new QuickPulseTelemetryModule()
        quickPulse.Initialize config
        quickPulse.RegisterTelemetryProcessor processor
        
        // Turn on Dependency Tracking if requested
        if configuration.TrackDependencies then
            let dependencyTracking = new DependencyTrackingTelemetryModule()
            dependencyTracking.Initialize TelemetryConfiguration.Active