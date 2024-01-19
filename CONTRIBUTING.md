# Contribution guideline

## Working with the template

There's a `SAFE-template.sln` solution file in the root of this repository that references fsproj both from minimal and default option.

To test your changes simply navigate to `Content\minimal` or `Content\default` and invoke the corresponding CLI commands from these directories (see `Content\minimal\README.md` and `Content\default\README.md`).

## Testing template bundle

To build whole template invoke `dotnet run --project Build.fsproj` - default target is `Install`, which will build the template and invoke `dotnet new -i <<repo-path>>/nupkg/SAFE.Template.<<version>>.nupkg`

You can now test the local build of template using `dotnet new SAFE`

## Known issues

* In case `dotnet new -i` fails for some reason, try uninstalling previously installed version first: `dotnet new -u SAFE.Template`

## Release

The template release process is currently done from a local dev machine.

Pre-requisites:

* A NuGet API key which allows release of the SAFE.Template package
* Permission to push to the GitHub repo

Steps:

1. Check out the repo at latest `master` commit with the official SAFE-Template repo as the `origin` remote.
1. Ensure you have no other local changes.
1. Add an entry (without committing) to `RELEASE_NOTES.md` with the new version, date and release notes.
1. Set the `NUGET_KEY` env var.
1. Run `dotnet run --project Build.fsproj -- release`

This will release the nuget package and commit and push the release notes to master.
