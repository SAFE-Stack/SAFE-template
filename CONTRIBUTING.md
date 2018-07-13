# Contribution guideline

## Building the template

1. install FAKE 5 as global tool
1. `fake build`

## Testing template in development mode

1. `fake build --target Install` - this will build the template and invoke `dotnet new -i <<repo-path>>/nupkg/SAFE.Template.<<version>>.nupkg`

## Known issues

* In case that `dotnet new -i` fails with an 'Reference not set' error on Linux, try uninstalling previous version first: `dotnet new -u SAFE.Template`