
[<RequireQualifiedAccess>]
module Promise

#nowarn "1182" // Unused values

open System
open Fable.Core
open Fable.Core.JsInterop

let inline private (!!) (x:obj): 'T = unbox x

[<Emit("new Promise($0)")>]
/// The promise function receives two other function parameters: success and fail
let create (f: ('T->unit)->(Exception->unit)->unit): JS.Promise<'T> = jsNative

[<Emit("new Promise(resolve => setTimeout(resolve, $0))")>]
let sleep (ms: int): JS.Promise<unit> = jsNative

[<Emit("Promise.resolve($0)")>]
let lift<'T> (a: 'T): JS.Promise<'T> = jsNative

/// Creates promise (in rejected state) with supplied reason.
let reject<'T> reason : JS.Promise<'T> = JS.Constructors.Promise.reject<'T> reason

[<Emit("$1.then($0)")>]
let bind (a: 'T->JS.Promise<'R>) (pr: JS.Promise<'T>): JS.Promise<'R> = jsNative

[<Emit("$1.then($0)")>]
let map (a: 'T->'R) (pr: JS.Promise<'T>): JS.Promise<'R> = jsNative

[<Emit("$1.then($0)")>]
let iter (a: 'T->unit) (pr: JS.Promise<'T>): unit = jsNative

[<Emit("$1.then(void 0, $0)")>]
/// This version of `catch` fakes a function returning just 'T, as opposed to `Promise<'T>`. If you need to return `Promise<'T>`, use `catchBind`.
let catch (fail: Exception->'T) (pr: JS.Promise<'T>): JS.Promise<'T> = jsNative

[<Emit("$1.then(void 0, $0)")>]
/// This is a version of `catch` that fakes a function returning Promise<'T> as opposed to just 'T. If you need to return just 'T, use `catch`.
let catchBind (fail: Exception->JS.Promise<'T>) (pr: JS.Promise<'T>): JS.Promise<'T> = jsNative

[<Emit("$1.then(void 0, $0)")>]
/// Used to catch errors at the end of a promise chain.
let catchEnd (fail: Exception->unit) (pr: JS.Promise<'T>): unit = jsNative

[<Emit("$2.then($0,$1)")>]
/// A combination of `map/bind` and `catch/catchBind`, this function applies the `success` continuation when the input promise resolves successfully, or `fail` continuation when the input promise fails. Both continuations may return either naked value `'R` or another promise `Promise<'R>`. Use the erased-cast operator `!^` to cast values when returning, for example:
/// ```
/// somePromise |> Promise.either (fun x -> !^(string x)) (fun err -> ^!(Promise.lift err.Message))
/// ```
let either (success: 'T->U2<'R, JS.Promise<'R>>) (fail: 'E->U2<'R, JS.Promise<'R>>) (pr: JS.Promise<'T>): JS.Promise<'R> = jsNative

[<Emit("$2.then($0,$1)")>]
let eitherEnd (success: 'T->unit) (fail: 'E->unit) (pr: JS.Promise<'T>): unit = jsNative

[<Emit("$0.then()")>]
let start (pr: JS.Promise<'T>): unit = jsNative

[<Emit("$1.then(void 0, $0)")>]
let tryStart (fail: Exception->unit) (pr: JS.Promise<'T>): unit = jsNative

[<Emit("Promise.all($0)")>]
let Parallel (pr: seq<JS.Promise<'T>>): JS.Promise<'T[]> = jsNative

[<Emit("Promise.all($0)")>]
let all (pr: seq<JS.Promise<'T>>): JS.Promise<'T[]> = jsNative

let result (a: JS.Promise<'A>): JS.Promise<Result<'A, 'E>> =
    either (U2.Case1 << Ok) (U2.Case1 << Error) a

let mapResult (fn: 'A -> 'B) (a: JS.Promise<Result<'A, 'E>>): JS.Promise<Result<'B, 'E>> =
    a |> map (Result.map fn)

let bindResult (fn: 'A -> JS.Promise<'B>) (a: JS.Promise<Result<'A, 'E>>): JS.Promise<Result<'B, 'E>> =
    a |> bind (fun a ->
        match a with
        | Ok a ->
            result (fn a)
        | Error e ->
            lift (Error e))

let mapResultError (fn: 'B -> 'C) (a: JS.Promise<Result<'A,'B>>): JS.Promise<Result<'A,'C>> =
    a |> map (Result.mapError fn)

let tap (fn: 'A -> unit) (a: JS.Promise<'A>): JS.Promise<'A> =
    a |> map (fun x -> fn x; x)

type PromiseBuilder() =
    [<Emit("$1.then($2)")>]
    member x.Bind(p: JS.Promise<'T>, f: 'T->JS.Promise<'R>): JS.Promise<'R> = jsNative

    [<Emit("$1.then(() => $2)")>]
    member x.Combine(p1: JS.Promise<unit>, p2: JS.Promise<'T>): JS.Promise<'T> = jsNative

    member x.For(seq: seq<'T>, body: 'T->JS.Promise<unit>): JS.Promise<unit> =
        // (lift (), seq)
        // ||> Seq.fold (fun p a ->
        //     bind (fun () -> body a) p)
        let mutable p = lift ()
        for a in seq do
            p <- !!p?``then``(fun () -> body a)
        p

    [<Emit("$1.then($2)")>]
    member x.For(p: JS.Promise<'T>, f: 'T->JS.Promise<'R>): JS.Promise<'R> = jsNative

    member x.While(guard, p): JS.Promise<unit> =
        if guard()
        then bind (fun () -> x.While(guard, p)) p
        else lift()

    [<Emit("Promise.resolve($1)")>]
    member x.Return(a: 'T): JS.Promise<'T> = jsNative

    [<Emit("$1")>]
    member x.ReturnFrom(p: JS.Promise<'T>): JS.Promise<'T> = jsNative

    [<Emit("Promise.resolve()")>]
    member x.Zero(): JS.Promise<unit> = jsNative

    member x.TryFinally(p: JS.Promise<'T>, compensation: unit->unit): JS.Promise<'T> =
        either (fun (x: 'T) -> compensation(); U2.Case1 x) (fun er -> compensation(); raise !!er) p

    [<Emit("$1.catch($2)")>]
    member x.TryWith(p: JS.Promise<'T>, catchHandler: Exception->JS.Promise<'T>): JS.Promise<'T> = jsNative

    member x.Delay(generator: unit->JS.Promise<'T>): JS.Promise<'T> =
        !!createObj[
            "then" ==> fun f1 f2 ->
                try generator()?``then``(f1,f2)
                with er ->
                    if box f2 = null
                    then !!JS.Constructors.Promise.reject(er)
                    else
                        try !!JS.Constructors.Promise.resolve(f2(er))
                        with er -> !!JS.Constructors.Promise.reject(er)
            "catch" ==> fun f ->
                try generator()?catch(f)
                with er ->
                    try !!JS.Constructors.Promise.resolve(f(er))
                    with er -> !!JS.Constructors.Promise.reject(er)
        ]

    member x.Run(p:JS.Promise<'T>): JS.Promise<'T> =
        create (fun success fail ->
            try
                let p : JS.Promise<'T> = !!JS.Constructors.Promise.resolve(p)
                p?``then``(success, fail)
            with
              er -> fail(er)
        )

    member x.Using<'T, 'R when 'T :> IDisposable>(resource: 'T, binder: 'T->JS.Promise<'R>): JS.Promise<'R> =
        x.TryFinally(binder(resource), fun () -> resource.Dispose())

    [<Emit("Promise.all([$1, $2])")>]
    member x.MergeSources(a: JS.Promise<'T1>, b: JS.Promise<'T2>): JS.Promise<'T1 * 'T2>= jsNative
    
    [<Emit("Promise.all([$1, $2, $3])")>]
    member x.MergeSources3(a: JS.Promise<'T1>, b: JS.Promise<'T2>, c: JS.Promise<'T3>): JS.Promise<'T1 * 'T2 * 'T3>= jsNative

    [<Emit("Promise.all([$1, $2, $3, $4])")>]
    member x.MergeSources4(a: JS.Promise<'T1>, b: JS.Promise<'T2>, c: JS.Promise<'T3>, d: JS.Promise<'T4>): JS.Promise<'T1 * 'T2 * 'T3 * 'T4>= jsNative
    
    [<Emit("Promise.all([$1, $2, $3, $4, $5])")>]
    member x.MergeSources5(a: JS.Promise<'T1>, b: JS.Promise<'T2>, c: JS.Promise<'T3>, d: JS.Promise<'T4>, e: JS.Promise<'T5>): JS.Promise<'T1 * 'T2 * 'T3 * 'T4 * 'T5>= jsNative
    
    [<Emit("Promise.all([$1, $2, $3, $4, $5, $6])")>]
    member x.MergeSources6(a: JS.Promise<'T1>, b: JS.Promise<'T2>, c: JS.Promise<'T3>, d: JS.Promise<'T4>, e: JS.Promise<'T5>, f: JS.Promise<'T6>): JS.Promise<'T1 * 'T2 * 'T3 * 'T4 * 'T5 * 'T6>= jsNative
    
//    member x.BindReturn(y: JS.Promise<'T1>, f) = map f y
    
    [<Emit("Promise.all([$1,$2]).then(([a,b]) => $3(a,b))")>]
    [<CustomOperation("andFor", IsLikeZip=true)>]
    member x.Merge(a: JS.Promise<'T1>, b: JS.Promise<'T2>, [<ProjectionParameter>] resultSelector : 'T1 -> 'T2 -> 'R): JS.Promise<'R> = jsNative
