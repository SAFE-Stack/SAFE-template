module Server.Tests

open Expecto

open Shared

let server = testList "Server" [
    testCase "Adding invalid Todo" <| fun _ ->
        let storage = Storage()
        let invalidTodo = Todo.create ""
        let expectedResult = Error "Invalid todo"

        let result = storage.AddTodo invalidTodo

        Expect.equal result expectedResult "Result should be error"
        Expect.isEmpty (storage.GetTodos()) "Storage should be empty"

    testCase "Adding valid Todo" <| fun _ ->
        let storage = Storage()
        let validTodo = Todo.create "TODO"
        let expectedResult = Ok ()

        let result = storage.AddTodo validTodo

        Expect.equal result expectedResult "Result should be ok"
        Expect.contains (storage.GetTodos()) validTodo "Storage should contain new todo"
]

let all =
    testList "All"
        [
            Shared.Tests.shared
            server
        ]

[<EntryPoint>]
let main _ = runTests defaultConfig all