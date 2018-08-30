# Contribution guideline

## Building the template

1. install FAKE 5 as global tool
1. `fake build` - default target is `Install`, which will build the template and invoke `dotnet new -i <<repo-path>>/nupkg/SAFE.Template.<<version>>.nupkg`
1. you can now test current code with `dotnet new SAFE`

## Updating Paket dependencies

See https://github.com/SAFE-Stack/SAFE-template/pull/139

## Known issues

* In case `dotnet new -i` fails for some reason, try uninstalling previously installed version first: `dotnet new -u SAFE.Template`
