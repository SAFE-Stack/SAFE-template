namespace SAFE.App.Shared

type Counter = { Value : int }
//#if (!minimal)
module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ICounterApi =
    { getInitialCounter : Async<Counter> }
//#endif