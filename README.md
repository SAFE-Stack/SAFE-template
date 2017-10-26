# SAFE template

A dotnet CLI template for [SAFE-Stack](https://safe-stack.github.io/) projects.

This is a basic template to get started with 3 core components of the stack:

* [Suave](https://suave.io/)
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
  * Runs `dotnet run --project src/Server/Server.fsproj` in root directory (note: Suave is launched on port **8085**)
  * Opens browser with url to Webpack development server (5 second delay after running client)

## Contributing

Refer to [Contribution guideline](CONTRIBUTING.md)
