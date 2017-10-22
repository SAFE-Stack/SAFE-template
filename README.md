# SAFE template

A dotnet CLI template for [SAFE-Stack](https://safe-stack.github.io/) projects.

This is a basic template to get started with 3 core components of the stack:

* [Suave](https://suave.io/)
* [Fable](http://fable.io/)
* [Elmish](https://fable-elmish.github.io/elmish/)

The template does not include any Azure / other Cloud integration. Refer to [SAFE-Bookstore](https://github.com/SAFE-Stack/SAFE-BookStore) to see an example of deploying to Azure.

## Prerequisites

* [dotnet SDK 2.0.0](https://www.microsoft.com/net/core) together with dotnet CLI
* [node.js](https://nodejs.org/)
* [yarn](https://yarnpkg.com/)

## Using the template

* Install the template: `dotnet new -i SAFE.Template::*`
* Create new project from the template: `dotnet new SAFE`
* Build the project: `build.cmd` / `build.sh`
* Run (working dir root of project): `dotnet run --project src\Server\Server.fsproj`
* Preview in browser: `http://localhost:8080/index.html`

## Contributing

Refer to [Contribution guideline](CONTRIBUTING.md)