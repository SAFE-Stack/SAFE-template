namespace Shared

type Counter = int

#if (Remoting)
type Init =
  { getCounter : unit -> Async<Counter> }
#endif