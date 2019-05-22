namespace Shared

type Counter = { Value : int }

#if (remoting)
module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type ICounterApi =
    { initialCounter : unit -> Async<Counter> }
#endif

#if (bridge)
module Socket =
    /// Defines the socket endpoint for the clock's two-way communication
    let clock = "/socket/clock"

/// A type that specifies the messages sent to the server from the client on Elmish.Bridge
/// to learn more, read about at https://github.com/Nhowka/Elmish.Bridge#shared
type ServerMsg =
    | Start
    | Pause

/// A type that specifies the messages sent to the client from the server on Elmish.Bridge
type ClientMsg =
    | GetTime of System.DateTime

#endif