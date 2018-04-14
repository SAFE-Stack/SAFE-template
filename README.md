# SAFE template

A dotnet CLI template for [SAFE-Stack](https://safe-stack.github.io/) projects.

This is a basic template to get started with 3 core components of the stack:

* [Saturn](https://saturnframework.github.io/docs/), [Giraffe](https://github.com/giraffe-fsharp/Giraffe) or [Suave](https://suave.io/) (see `Server` option below)
* [Fable](http://fable.io/)
* [Elmish](https://fable-elmish.github.io/elmish/)

The template does not **yet** include any Azure / other Cloud integration. Refer to [SAFE-Bookstore](https://github.com/SAFE-Stack/SAFE-BookStore) to see an example of deploying to Azure and for more details about this stack.

## Prerequisites

* [dotnet SDK 2.0.0](https://www.microsoft.com/net/core) together with dotnet CLI
* [node.js](https://nodejs.org/)
* [yarn](https://yarnpkg.com/)
* [.NET Framework](https://www.microsoft.com/net/download/dotnet-framework-runtime) (Windows) / [mono](http://www.mono-project.com/) (MacOS / Linux) additionally for build tools (Paket, FAKE) - migration to dotnet SDK is WIP

## Using the template

The template comes with different options. Please see [SAFE docs](https://safe-stack.github.io/docs/safe-template/) for details.

## Contributing

Refer to [Contribution guideline](CONTRIBUTING.md)
