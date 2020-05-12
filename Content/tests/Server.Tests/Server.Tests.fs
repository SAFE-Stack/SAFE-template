module Server.Tests

open Expecto

open Shared

let server = testList "Server" [
    testCase "Adding invalid Todo" <| fun _ ->
        let initialList = Server.todos
        let result = Server.addTodo (Todo.create "")
        let expectedResult = Error "Invalid todo"
        Expect.equal Server.todos initialList "Todos shouldn't change"
        Expect.equal result expectedResult "Result should be error"
    testCase "Adding valid Todo" <| fun _ ->
        let initialList = Server.todos
        let newTodo = Todo.create "TODO"
        let result = Server.addTodo newTodo
        let expectedResult = Ok ()
        Expect.contains Server.todos newTodo "Todos should contain new todo"
        Expect.equal result expectedResult "Result should be ok"
]

let all =
    testList "All"
        [
            Shared.Tests.shared
            server
        ]

[<EntryPoint>]
let main _ = runTests defaultConfig all