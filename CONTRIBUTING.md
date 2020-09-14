# Contribution guideline

## Working with the template

There's a `SAFE-template.sln` solution file in the root of this repository that references fsproj both from minimal and default option.

To test your changes simply navigate to `Content\minimal` or `Content\default` and invoke the corresponding CLI commands from these directories (see `Content\minimal\README.md` and `Content\default\README.md`).

## Testing template bundle

To build whole template invoke `dotnet fake build` - default target is `Install`, which will build the template and invoke `dotnet new -i <<repo-path>>/nupkg/SAFE.Template.<<version>>.nupkg`

You can now test the local build of template using `dotnet new SAFE`

## Known issues

* In case `dotnet new -i` fails for some reason, try uninstalling previously installed version first: `dotnet new -u SAFE.Template`
