module App

open Browser
open Thoth.Fetch

promise {
    let header = document.getElementById "header"
    header.innerText <- "loading..."
    do! Promise.sleep 1000
    let! message = Fetch.get Shared.Route.hello
    header.innerText <- message
} |> Promise.start
