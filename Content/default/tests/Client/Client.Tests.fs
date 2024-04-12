module Client.Tests

open Fable.Mocha

open Index
open Shared
open SAFE

let client =
    testList "Client" [
        testCase "Added todo"
        <| fun _ ->
            let newTodo = Todo.create "new todo"
            let model, _ = init ()

            let model, _ = update (SaveTodo(Finished newTodo)) model

            Expect.equal
                (model.Todos |> RemoteData.map _.Length |> RemoteData.defaultValue 0)
                1
                "There should be 1 todo"

            Expect.equal
                (model.Todos
                 |> RemoteData.map List.head
                 |> RemoteData.defaultValue (Todo.create ""))
                newTodo
                "Todo should equal new todo"
    ]

let all =
    testList "All" [
//-:cnd:noEmit
#if FABLE_COMPILER // This preprocessor directive makes editor happy
        Shared.Tests.shared
#endif
//+:cnd:noEmit
        client
    ]

[<EntryPoint>]
let main _ = Mocha.runTests all