# SAFE Template

This template can be used to generate a full-stack web application using the [SAFE Stack](https://safe-stack.github.io/). It was created using the dotnet [SAFE Template](https://safe-stack.github.io/docs/template-overview/). If you want to learn more about the template why not start with the [quick start](https://safe-stack.github.io/docs/quickstart/) guide?

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET Core SDK](https://www.microsoft.com/net/download)
* [FAKE 5](https://fake.build/) installed as a [global tool](https://fake.build/fake-gettingstarted.html#Install-FAKE)
* The [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager (you can also use `npm` but the usage of `yarn` is encouraged).
* [Node LTS](https://nodejs.org/en/download/) installed for the front end components.
* If you're running on OSX or Linux, you'll also need to install [Mono](https://www.mono-project.com/docs/getting-started/install/).

## Work with the application

To concurrently run the server and the client components in watch mode use the following command:

```bash
fake build -t Run
```

//#if (deploy == "docker")

You can use the included `Dockerfile` and `build.fsx` script to deploy your application as Docker container. You can find more regarding this topic in the [official template documentation](https://safe-stack.github.io/docs/template-docker/).

//#endif
//#if (deploy == "azure")

You can use the included `arm-template.json` file and `build.fsx` script to deploy you application as an Azure Web App. Consult the [official template documentation](https://safe-stack.github.io/docs/template-appservice/) to learn more.

//#endif

## SAFE Stack Documentation

You will find more documentation about the used F# components at the following places:

//#if (server == "suave")
* [Suave](https://suave.io/index.html)
//#elseif (server == "giraffe")
* [Giraffe](https://github.com/giraffe-fsharp/Giraffe/blob/master/DOCUMENTATION.md)
//#elseif (server == "saturn")
* [Saturn](https://saturnframework.org/docs/)
//#endif
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
//#if (streams)
* [Elmish.Streams](https://elmish-streams.readthedocs.io/)
//#endif
//#if (remoting)
* [Fable.Remoting](https://zaid-ajaj.github.io/Fable.Remoting/)
//#endif
//#if (layout != "none")
* [Fulma](https://fulma.github.io/Fulma/)
//#endif

If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).

## Troubleshooting

* **fake not found** - If you fail to execute `fake` from command line after installing it as a global tool, you might need to add it to your `PATH` manually: (e.g. `export PATH="$HOME/.dotnet/tools:$PATH"` on unix) - [related GitHub issue](https://github.com/dotnet/cli/issues/9321)
