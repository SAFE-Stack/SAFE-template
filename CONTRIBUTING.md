# Contribution guideline

## Building the template

`build.cmd` / `build.sh`

## Testing template in development mode

### Testing directly

1. Change directory to `Content`
1. Trigger `build.cmd run` / `build.sh`

### Testing NuGet package

1. Build template from root directory
1. Uninstall currently installed template with `dotnet new -u SAFE.Template`
1. Install new version with `dotnet new -i <<repo-path>>/nupkg/SAFE.Template.<<version>>.nupkg`