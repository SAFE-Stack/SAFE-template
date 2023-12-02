open Farmer
open Farmer.Builders
open Fun.Build
open Fun.Result
open System.IO
open type System.IO.Path

[<EntryPoint>]
let main _ =
    let serverPath = GetFullPath "src/Server"
    let clientPath = GetFullPath "src/Client"

    pipeline "Test" {
        description "Runs all tests"

        stage "Build Shared Tests Component" { run $"""dotnet build {GetFullPath "tests/Shared"}""" }

        stage "Run Tests" {
            paralle

            stage "Server Tests" { run $"""dotnet watch run --project {GetFullPath "tests/Server"}""" }

            stage "Client Tests" {
                run $"""dotnet fable watch -o output -s --cwd {GetFullPath "tests/Client"} --run npx vite"""
            }
        }

        runIfOnlySpecified // custom pipeline - dotnet run -- -p Test
    }

    pipeline "Format" {
        description "Formats all code using Fantomas"
        stage "format" { run "dotnet fantomas ." }
        runIfOnlySpecified // custom pipeline - dotnet run -- -p Format
    }

    pipeline "Bundle" {
        description "Builds and packages the app for production"

        stage "Build" {
            paralle

            stage "Server" {
                run (fun ctx -> asyncResult {
                    let deployPath = GetFullPath "deploy"

                    if Directory.Exists deployPath then
                        Directory.Delete(deployPath, true)

                    do! ctx.RunCommand $"dotnet publish -c Release {serverPath} -o {deployPath}"
                })
            }

            stage "Client" {
                run "npm install"
                run $"dotnet fable clean --cwd {clientPath} --yes"
                run $"dotnet fable -o output -s --cwd {clientPath} --run npx vite build"
            }
        }

        runIfOnlySpecified // custom pipeline - dotnet run -- -p Bundle
    }

    pipeline "Azure" {
        description "Deploy to Azure"

        stage "Farmer deploy" {
            run (fun _ ->
                let web = webApp {
                    name "SAFE-App"
                    operating_system Linux
                    runtime_stack (DotNet "8.0")
                    zip_deploy "deploy"
                }

                let deployment = arm {
                    location Location.WestEurope
                    add_resource web
                }

                deployment |> Deploy.execute "SAFE-App" Deploy.NoParameters |> ignore)
        }

        runIfOnlySpecified // custom pipeline - dotnet run -- -p Azure
    }

    pipeline "Run" {
        description "Runs the SAFE Stack application in watch mode"

        stage "Run" {
            paralle

            stage "Server" { run $"dotnet watch run --project {serverPath}" }

            stage "Client" {
                run "npm install"
                run $"dotnet fable watch -o output -s --cwd {clientPath} --run npx vite"
            }
        }

        runIfOnlySpecified false // default pipeline - dotnet run
    }

    tryPrintPipelineCommandHelp ()
    0