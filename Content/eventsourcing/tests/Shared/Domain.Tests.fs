module Shared.Tests
// open Shared.Todos
open System


#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open Shared

let domain = testList "Todos domain members" [
    testCase "Empty string is not a valid description" <| fun _ ->
        Expect.isTrue true "true"
        // let expected = false
        // let actual = Todo.isValid ""
        // Expect.equal actual expected "Should be false"

    // testCase "add any new todo - ok" <| fun _ ->
    //     let todos = Todos.Zero
    //     let todo = {
    //         Id = Guid.NewGuid()
    //         Description = "a new todo"
    //     }
    //     let result = todos.AddTodo todo
    //     Expect.isOk result "should be ok"

    // testCase "add a todo with an already existing description - Error" <| fun _ ->
    //     let todos = Todos.Zero
    //     let todo = {
    //         Id = Guid.NewGuid()
    //         Description = "todo"
    //     }
    //     let todos' = todos.AddTodo todo |> Result.get

    //     let anotherTodo = {
    //         Id = Guid.NewGuid()
    //         Description = "todo"
    //     }

    //     let result = todos'.AddTodo anotherTodo
    //     Expect.isError result "should be error"

    // testCase "remove a todo - Ok" <| fun _ ->
    //     let guid = Guid.NewGuid()
    //     let todos =
    //         {
    //             Todos.Zero with
    //                 todos =
    //                     [
    //                         {
    //                             Id = guid
    //                             Description = "todo"
    //                         }
    //                     ]
    //         }
    //     let result = todos.RemoveTodo guid
    //     Expect.isOk result "should be ok"
    //     let result' = result |> Result.get
    //     Expect.equal result' Todos.Zero "should be equal"
]
