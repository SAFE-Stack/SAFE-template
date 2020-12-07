module App

open Browser
open Thoth.Fetch

promise {
    let! msg = Fetch.get Shared.Route.hello
    document.getElementById("header").innerText <- msg
} |> Promise.start
