# Contribution guideline

## Building the template

1. install FAKE 5 as global tool
1. `fake run build.fsx`

## Testing template in development mode

### Testing directly

1. Change directory to `Content`
1. Trigger `fake run build.fsx`

### Testing NuGet package

1. Build template from root directory
1. Install built package with `dotnet new -i <<repo-path>>/nupkg/SAFE.Template.<<version>>.nupkg`

## Known issues

* In case that `dotnet new -i` fails with an 'Reference not set' error on Linux, try uninstalling previous version first: `dotnet new -u SAFE.Template`