# Contribution guideline

## Building the template

`build.cmd` / `build.sh`

## Testing template in development mode

1. Uninstall currently installed version with `dotnet new -u SAFE.Template`
1. Install new version with `dotnet new -i <<repo-path>>/nupkg/SAFE.Template.<<version>>.nupkg`