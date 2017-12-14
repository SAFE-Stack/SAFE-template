namespace Shared

type Counter = int

#if (Remoting)
/// Defines how routes are generated on server and mapped from client
let routeBuilder typeName methodName = 
    sprintf "/api/%s/%s" typeName methodName

/// A type the specifies the communication protocol for client and server
/// Every record field must have the type : 'a -> Async<'b> where 'a can also be `unit`
type ICounterProtocol =
  { getInitCounter : unit -> Async<Counter> }
#endif