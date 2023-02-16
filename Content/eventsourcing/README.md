# Event sourcing version of SAFE template

I adapted the SAFE Template integrating it into a tiny _event sourcing_ set of utilities I created.

I created an aggregate implementing our logic [Todos.fs](./src/Shared/Todos.fs). It is unaware the persistency logic, because I wrap members that are supposed to "change the state" (in a functional way) into commands and events, and then expose the commands in [App.fs](./src/Server/App.fs).
To wrap specific domain logic members we need a discriminated union for the Events (in the Todos.fs module) and a discriminated union for commands [Commands.fs](./src/Shared/Commands.fs). What commands do is probing the respective members and return events if they don't return error.

Recap. You create the aggregate, the events, expose them (as in App.fs), and anything else can be reused to implement the business domain.

More details:
* [DbStorage.fs](./src/Server/DbStorage.fs): connect to the database for writing and reading events and snapshots.
* [MemoryStorage.fs](./src/Server/DbStorage.fs): alternative of db, is this one to use in memory data structures to manage events and snapshots.
* [Repository.fs](./src/Server/Repository.fs): rely on the storage to:
* 1) get the current state.
* 2) run commands, generating the corresponding events and storing them.
* 3) store and read snapshots
* [EventSourcing.fs](./src/Shared/EventSourcing.fs): defines abstractions based on interfaces, generics, constraints.

Description:
* 1.  events must be discriminated Unions implementing the _Processable_ interface, to wrap the members defined in the aggregate [Todos.fs](./src/Shared/Todos.fs).
* 2.  _Commands_ implement the _Executable_ interface and returns events or error, as in the [Commands.fs](./src/Shared/Commands.fs).
* 3.  [App.fs](./src/Server/App.fs) Exposes the domain logic. Methods present there run command if needed and create snapshots.
* 4. __How to make upgrades i.e. manage versions__: I don't manage versioning yet, but when you add behavior to the aggregate without changing previous behavior then I know that a simple solution is:
* * A. when the new version of your root class is ready (i.e. the new version of the todo), then just deploy it, and delete all the snapshots entries in the snapshots table. What will happen is that the snapshot in the new format will be rebuilt, after restart or after an event.
* * B. Deploy the part related to new events (Processable), commands (Executable) and App (App.fs exposing the business logic to outside).
* * C. Make the new behavior available by extending the interface used by SAFE (i.e. the ITodosApi) and its implementation (todosapi in [Server.fs](./src/Server/Server.fs))
* * Basically: under the acceptable assumption that when upgrading we just extend, and not change the aggregate (substitution principle), then any upgrade should not be a problem.

Other "dispatching" logic work in the same way as they original are part of the way the original SAFE example. (i.e. ITodosApi and  interface and implementation)

## Installation
The ordinary SAFE requirement (as follows) plus a postgres database using the Schema.sql script to create user, database, and tables.

#### What follows now is the original SAFE doc


# SAFE Template

This template can be used to generate a full-stack web application using the [SAFE Stack](https://safe-stack.github.io/). It was created using the dotnet [SAFE Template](https://safe-stack.github.io/docs/template-overview/). If you want to learn more about the template why not start with the [quick start](https://safe-stack.github.io/docs/quickstart/) guide?

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* [.NET Core SDK](https://www.microsoft.com/net/download) 6.0 or higher
* [Node 16](https://nodejs.org/en/download/)

## Starting the application

Before you run the project **for the first time only** you must install dotnet "local tools" with this command:

```bash
dotnet tool restore
```

To concurrently run the server and the client components in watch mode use the following command:

```bash
dotnet run
```

Then open `http://localhost:8080` in your browser.

The build project in root directory contains a couple of different build targets. You can specify them after `--` (target name is case-insensitive).

To run concurrently server and client tests in watch mode (you can run this command in parallel to the previous one in new terminal):

```bash
dotnet run -- RunTests
```

Client tests are available under `http://localhost:8081` in your browser and server tests are running in watch mode in console.

Finally, there are `Bundle` and `Azure` targets that you can use to package your app and deploy to Azure, respectively:

```bash
dotnet run -- Bundle
dotnet run -- Azure
```

## SAFE Stack Documentation

If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
