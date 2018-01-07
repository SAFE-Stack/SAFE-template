# SAFE template

A dotnet CLI template for [SAFE-Stack](https://safe-stack.github.io/) projects.

This is a basic template to get started with 3 core components of the stack:

* [Suave](https://suave.io/) or [Giraffe](https://github.com/giraffe-fsharp/Giraffe) (see `Server` option below)
* [Fable](http://fable.io/)
* [Elmish](https://fable-elmish.github.io/elmish/)

The template does not include any Azure / other Cloud integration. Refer to [SAFE-Bookstore](https://github.com/SAFE-Stack/SAFE-BookStore) to see an example of deploying to Azure and for more details about this stack.

## Prerequisites

* [dotnet SDK 2.0.0](https://www.microsoft.com/net/core) together with dotnet CLI
* [node.js](https://nodejs.org/)
* [yarn](https://yarnpkg.com/)

## Using the template

* Install or update the template: `dotnet new -i SAFE.Template`
* Create a new project from the template: `dotnet new SAFE`
* Build and run the project: `build.cmd run` / `./build.sh run`. This command:
  * Fetches all necessary dependencies
  * Builds Server and Client code
  * Runs `dotnet fable webpack-dev-server` in [src/Client](src/Client) (note: the Webpack development server will serve files on http://localhost:8080)
  * Runs `dotnet watch run` in [src/Server](src/Server) (note: Suave is launched on port **8085**)
  * Opens browser with url to Webpack development server (5 second delay after running client)

Now you can edit:
* `src\Client\App.fs` - changes will be automatically refreshed in browser
* `src\Server\Program.fs` - server code will be recompiled and re-run in the background

## Template options

Show template version and available options with : `dotnet new SAFE --help`

### Server

Choose between available Server back-ends:

* [Suave.io](http://suave.io) (default)
* [Giraffe](https://github.com/giraffe-fsharp/Giraffe) `dotnet new SAFE --Server giraffe`

### Fulma

Add [Fulma](https://mangelmaxime.github.io/Fulma) to the template:

* none - don't add Fulma (Fable bindings for Bulma.io) at all
* basic - adds Fulma basic template `dotnet new SAFE --Fulma basic`
* hero - adds Fulma with ['Hero' Bulma template](https://dansup.github.io/bulma-templates/) `dotnet new SAFE --Fulma hero`
* landing - adds Fulma with ['Landing' Bulma template](https://dansup.github.io/bulma-templates/) `dotnet new SAFE --Fulma landing`

### Fable.Remoting

Add [Fable.Remoting](https://github.com/Zaid-Ajaj/Fable.Remoting) to the template by specifying `--Remoting` (or `-R`) parameter:

`dotnet new SAFE --Remoting`

## Contributing

Refer to [Contribution guideline](CONTRIBUTING.md)
