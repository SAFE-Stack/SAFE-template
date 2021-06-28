module Client

open Browser

promise {
    let header = document.getElementById "header"
    header.innerText <- "loading..."
    do! Promise.sleep 1000
    let! response = Fetch.fetch Shared.Route.hello []
    let! text = response.text()
    header.innerText <- text
} |> Promise.start
