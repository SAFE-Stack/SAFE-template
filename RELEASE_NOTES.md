#### 5.0.4 - 08.02.2024

* Bump vite from 5.0.5 to 5.0.12 in /Content/default - https://github.com/SAFE-Stack/SAFE-template/pull/590
* Bump vite from 5.0.5 to 5.0.12 in /Content/minimal - https://github.com/SAFE-Stack/SAFE-template/pull/591

#### 5.0.3 - 19.01.2024

* Update Fable to 4.1.4 - https://github.com/SAFE-Stack/SAFE-template/pull/586
* Resolve security vulnerabilities in .net packages - https://github.com/SAFE-Stack/SAFE-template/pull/587

#### 5.0.2 - 08.12.2023

* Update README and project/solution file references - https://github.com/SAFE-Stack/SAFE-template/pull/582
* Bump Vite from 5.0.0 to 5.0.5 - https://github.com/SAFE-Stack/SAFE-template/pull/584 - https://github.com/SAFE-Stack/SAFE-template/pull/585

#### 5.0.1 - 01.12.2023

* Fix client tests - https://github.com/SAFE-Stack/SAFE-template/pull/581

#### 5.0.0 - 01.12.2023

https://github.com/SAFE-Stack/SAFE-template/pull/564
https://www.compositional-it.com/news-blog/announcing-safe-template-v5/
* Upgrade to .NET 8
* Upgrade to F# 8
* Upgrade to Fable 4 and update Fable dependencies
* Use Vite instead of Webpack for JS bundling
* Use Tailwind for styling

#### 4.3.0 - 07.11.2023

* Remove old unused reference to Fulma - https://github.com/SAFE-Stack/SAFE-template/pull/546
* Fix failing build because of NPM version - https://github.com/SAFE-Stack/SAFE-template/pull/560
* Use CreateProcess.fromRawCommand - https://github.com/SAFE-Stack/SAFE-template/pull/558
* Specify major version in paket.dependencies - https://github.com/SAFE-Stack/SAFE-template/pull/559
* Update Fable to 3.7.22 - https://github.com/SAFE-Stack/SAFE-template/pull/562
* Fix issues in default template build script for deploy to Azure - https://github.com/SAFE-Stack/SAFE-template/pull/566

#### 4.2.0 - 25.11.2022

* Remove deprecated extension recommendation - https://github.com/SAFE-Stack/SAFE-template/pull/536
* Update package.json files to allow node 18 (as well as 16) - https://github.com/SAFE-Stack/SAFE-template/pull/541

#### 4.1.1 - 05.10.2022

* Fix .net sdk selection in global.json -  https://github.com/SAFE-Stack/SAFE-template/pull/534

#### 4.1.0 - 23.09.2022

* allow Linux Azure App Service - https://github.com/SAFE-Stack/SAFE-template/pull/528
* Include launchSettings.json in both templates - https://github.com/SAFE-Stack/SAFE-template/pull/524
* Pin FSharp.Core ~> 6 - https://github.com/SAFE-Stack/SAFE-template/pull/525

#### 4.0.0 - 23.06.2022

* .NET 6.0 - https://github.com/SAFE-Stack/SAFE-template/pull/506
* Update Fantomas to 4.6 - https://github.com/SAFE-Stack/SAFE-template/pull/499
* Add server unit tests to CI - https://github.com/SAFE-Stack/SAFE-template/pull/493
* Webpack improvements - https://github.com/SAFE-Stack/SAFE-template/pull/491
* Listen on all interfaces including ipv6 - https://github.com/SAFE-Stack/SAFE-template/pull/490
* Force node v16 and npm v8 - https://github.com/SAFE-Stack/SAFE-template/pull/489
* Update Server tests to use module that replaced class - https://github.com/SAFE-Stack/SAFE-template/pull/484
* Include a correctly configured VS launch settings file. - https://github.com/SAFE-Stack/SAFE-template/pull/482

#### 3.1.1 - 31.08.2021

* Fix flags for fable in bundle target - https://github.com/SAFE-Stack/SAFE-template/pull/476

#### 3.1.0 - 30.08.2021

* Update Farmer to 1.6.12 - https://github.com/SAFE-Stack/SAFE-template/pull/468
* Add Femto tool - https://github.com/SAFE-Stack/SAFE-template/pull/469
* Fable outputs to "output" folder - https://github.com/SAFE-Stack/SAFE-template/pull/470
* Remove font awesome from paket - https://github.com/SAFE-Stack/SAFE-template/pull/471
* Basic support for source maps - https://github.com/SAFE-Stack/SAFE-template/pull/474

#### 3.0.2 - 09.07.2021

* Added meta tag for viewport to help with mobile responsiveness - https://github.com/SAFE-Stack/SAFE-template/pull/457

#### 3.0.1 - 29.06.2021

* Remove unwanted directory from the template package - https://github.com/SAFE-Stack/SAFE-template/pull/415

#### 3.0.0 - 28.06.2021

* .NET 5 and Fable 3 - https://github.com/SAFE-Stack/SAFE-template/pull/415

#### 2.2.2 - 22.01.2021

* Fix corrupted package - https://github.com/SAFE-Stack/SAFE-template/issues/421

#### 2.2.1 - 21.01.2021

* Fix NPM vulnerabilities - https://github.com/SAFE-Stack/SAFE-template/pull/418

#### 2.2.0 - 21.09.2020

* Replace netstandard2.0 with netcoreapp3.1 - https://github.com/SAFE-Stack/SAFE-template/pull/407
* CVE-2020-7720 Prototype Pollution in node-forge - https://github.com/SAFE-Stack/SAFE-template/issues/406

#### 2.1.0 - 10.09.2020

* Update dependencies - https://github.com/SAFE-Stack/SAFE-template/pull/398
* Use do when yielding todos - https://github.com/SAFE-Stack/SAFE-template/pull/394
* set NODE_ENV to production - https://github.com/SAFE-Stack/SAFE-template/pull/393

#### 2.0.0 - 27.08.2020

* Version 2 is partially a rewrite of the original SAFE Template
* All template options from v1 have been removed
* New minimal option for advanced users allows to create a bare bone project and have full control
* Shared is now a project not a referenced file
* Unit tests for Shared Client and Server have been added to default template
* Counter sample has been replaced with Todo List in default template
* Other changes have been described in latest docs

#### 1.22.3 - 22.03.2020

* Fix Bulma template links - https://github.com/SAFE-Stack/SAFE-template/pull/345

#### 1.22.2 - 16.03.2020

* Bump acorn from 6.3.0 to 6.4.1 - https://github.com/SAFE-Stack/SAFE-template/pull/342

#### 1.22.1 - 12.03.2020

* Fix devServerProxy context paths - https://github.com/SAFE-Stack/SAFE-template/pull/341

#### 1.22.0 - 10.03.2020

* Use FAKE and Paket local tools in template output - https://github.com/SAFE-Stack/SAFE-template/pull/339
* Update publisher for vscode csharp extension - https://github.com/SAFE-Stack/SAFE-template/pull/340

#### 1.21.0 - 27.02.2020

* Update NPM and Yarn dependencies - https://github.com/SAFE-Stack/SAFE-template/pull/338

#### 1.20.0 - 04.02.2020

* Update Saturn to 0.11 - https://github.com/SAFE-Stack/SAFE-template/pull/334

#### 1.19.1 - 28.01.2020

* Fix program launch to correct netcoreapp - https://github.com/SAFE-Stack/SAFE-template/pull/331

#### 1.19.0 - 14.11.2019

* Update to .NET Core 3.0 - https://github.com/SAFE-Stack/SAFE-template/pull/318
* Fulma Admin template small position fix - https://github.com/SAFE-Stack/SAFE-template/pull/322

#### 1.18.0 - 10.09.2019

* Bump up JS dependencies - https://github.com/SAFE-Stack/SAFE-template/pull/314

#### 1.17.0 - 30.08.2019

* Add tenantId to Azure deployment option - https://github.com/SAFE-Stack/SAFE-template/pull/310

#### 1.16.0 - 13.08.2019

* Set font by customizing a Bulma variable - https://github.com/SAFE-Stack/SAFE-template/pull/305
* Include style.scss in Client.fsproj - https://github.com/SAFE-Stack/SAFE-template/pull/306
* Fix indentation for DU definition and pattern match - https://github.com/SAFE-Stack/SAFE-template/pull/302

#### 1.15.0 - 09.08.2019

* Update React to 16.9 - https://github.com/SAFE-Stack/SAFE-template/pull/304

#### 1.14.0 - 23.07.2019

* Update client dependencies - https://github.com/SAFE-Stack/SAFE-template/pull/300

#### 1.13.0 - 22.07.2019

* Fix runTool working directory - https://github.com/SAFE-Stack/SAFE-template/pull/297

#### 1.12.0 - 22.07.2019

* Remove obsolete 'reaction' option and keep using 'streams' - https://github.com/SAFE-Stack/SAFE-template/pull/299

#### 1.11.1 - 09.07.2019

* Add .ionide to gitignore - https://github.com/SAFE-Stack/SAFE-template/pull/291

#### 1.11.0 - 05.07.2019

* Pull Bulma FA and OpenSans from NPM repository instead of including from CDN - https://github.com/SAFE-Stack/SAFE-template/pull/286

#### 1.10.0 - 28.06.2019

* Update FAKE and use 'initEnvironment' to support Ionide 4 integration - https://github.com/SAFE-Stack/SAFE-template/pull/280

#### 1.9.0 - 19.06.2019

* Add Release Notes template to created project - https://github.com/SAFE-Stack/SAFE-template/pull/277

#### 1.8.0 - 06.06.2019

* Add Elmish.Bridge as a communication option - https://github.com/SAFE-Stack/SAFE-template/pull/267
* no need to restore client - https://github.com/SAFE-Stack/SAFE-template/pull/270

#### 1.7.0 - 23.05.2019

* Move .sln to the root directory and add files - https://github.com/SAFE-Stack/SAFE-template/pull/266
* Fontawesome small changes - https://github.com/SAFE-Stack/SAFE-template/pull/264
* Set Fable variable on client project - https://github.com/SAFE-Stack/SAFE-template/pull/262

#### 1.6.0 - 16.05.2019

* New Fable dependencies - https://github.com/SAFE-Stack/SAFE-template/pull/250

#### 1.5.1 - 14.05.2019

* Giraffe - Be explicit that the returned object should be JSON - https://github.com/SAFE-Stack/SAFE-template/pull/259

#### 1.5.0 - 13.05.2019

* Revert 'Add Shared.fsproj' - https://github.com/SAFE-Stack/SAFE-template/pull/256

#### 1.4.0 - 10.05.2019

* Add devcontainer definition - https://github.com/SAFE-Stack/SAFE-template/pull/254
* Add Shared.fsproj - https://github.com/SAFE-Stack/SAFE-template/pull/256

#### 1.3.0 - 25.04.2019

* Support heroku - https://github.com/SAFE-Stack/SAFE-template/pull/251

#### 1.2.0 - 23.04.2019

* Add IIS deployment option - https://github.com/SAFE-Stack/SAFE-template/pull/226

#### 1.1.0 - 17.04.2019

* Change default from sass to scss - https://github.com/SAFE-Stack/SAFE-template/pull/248

#### 1.0.1 - 11.04.2019

* Fix gcloud tool name on Windows - https://github.com/SAFE-Stack/SAFE-template/pull/243

#### 1.0.0 - 05.04.2019

* Release version 1.0 live from stage from F# eXchange 2019

#### 0.49.2 - 31.03.2019

* Fix initial build failure - https://github.com/SAFE-Stack/SAFE-template/pull/241

#### 0.49.1 - 29.03.2019

* Add unicode regex to recognize 'watch client' vs code task end - https://github.com/SAFE-Stack/SAFE-template/issues/240

#### 0.49.0 - 27.03.2019

* Take advantage of use_json_serializer - https://github.com/SAFE-Stack/SAFE-template/pull/237
* Remove FAKE issue warning - https://github.com/SAFE-Stack/SAFE-template/pull/236

#### 0.48.0 - 04.03.2019

* Remove paket.exe - https://github.com/SAFE-Stack/SAFE-template/issues/106

#### 0.47.0 - 31.01.2019

* Update dependencies - https://github.com/SAFE-Stack/SAFE-template/pull/223
* Update Fulma URL - https://github.com/SAFE-Stack/SAFE-template/pull/224

#### 0.46.0 - 21.01.2019

* Update to .NET Core 2.2 - https://github.com/SAFE-Stack/SAFE-template/pull/218

#### 0.45.0 - 21.01.2019

* Support for deploying to Google Cloud Kubernetes Engine - https://github.com/SAFE-Stack/SAFE-template/pull/209

#### 0.44.0 - 16.01.2019

* Add VSCode extensions.json to recommend extensions - https://github.com/SAFE-Stack/SAFE-template/issues/202

#### 0.43.0 - 15.01.2019

* Support for deploying to Google Cloud AppEngine - https://github.com/SAFE-Stack/SAFE-template/pull/207

#### 0.42.0 - 08.01.2019

* Update JS dependencies to remove security vulnerability warnings - https://github.com/SAFE-Stack/SAFE-template/issues/113
* Replace 'successful.ok' with 'json' in Saturn template - https://github.com/SAFE-Stack/SAFE-template/issues/211
* Replace 'Text.p' with 'Text.div' to prevent DOMNesting warning - https://github.com/SAFE-Stack/SAFE-template/issues/215

#### 0.41.0 - 07.01.2019

* Unify webpack.config.js with webpack-config-template - https://github.com/SAFE-Stack/SAFE-template/pull/204

#### 0.40.1 - 21.12.2018

* Upgrade CSS references for Bulma to 0.7.1 and Font Awesome to 5.6.1

#### 0.40.0 - 18.12.2018

* Update dependencies (nuget and npm) - https://github.com/SAFE-Stack/SAFE-template/pull/199

#### 0.39.0 - 05.12.2018

* Remove bulma burgers from fulma layouts - https://github.com/SAFE-Stack/SAFE-template/pull/197

#### 0.38.2 - 23.11.2018

* Webpack - set terser minifier configuration compress.inline to false - https://github.com/SAFE-Stack/SAFE-template/pull/193

#### 0.38.1 - 21.11.2018

* Cleanup 'Run' target in build script

#### 0.38.0 - 20.11.2018

* VS Code Launchers and Tasks do Debug the project - https://github.com/SAFE-Stack/SAFE-template/pull/161

#### 0.37.0 - 19.11.2018

* Pass Babel options to fable-loader - https://github.com/SAFE-Stack/SAFE-template/pull/189
* Add more comments to webpack.js - https://github.com/SAFE-Stack/SAFE-template/pull/186
* Change webpack environment variable from SUAVE_FABLE_PORT to SERVER_PROXY_PORT

#### 0.36.0 - 12.11.2018

* Model counter as a record - https://github.com/SAFE-Stack/SAFE-template/pull/182
* Add README.md file to SAFE Template content - https://github.com/SAFE-Stack/SAFE-template/pull/183

#### 0.35.0 - 08.11.2018

* Use Thoth.Json on server side when not using Fable.Remoting - https://github.com/SAFE-Stack/SAFE-template/pull/179

#### 0.34.0 - 07.11.2018

* Demonstrate usage of "nothing" from Fable.React in "fulma-hero" - https://github.com/SAFE-Stack/SAFE-template/pull/175

#### 0.33.1 - 06.11.2018

* Cleanup JavaScript dependencies - https://github.com/SAFE-Stack/SAFE-template/pull/173

#### 0.33.0 - 05.11.2018

* New option for Fable.Reaction - https://github.com/SAFE-Stack/SAFE-template/pull/168

#### 0.32.0 - 02.11.2018

* Build script - Change to runOrDefaultWithArguments to provide support for default arguments. - https://github.com/SAFE-Stack/SAFE-template/pull/170

#### 0.31.0 - 31.10.2018

* Use FAKE's new CreateProcess API - https://github.com/SAFE-Stack/SAFE-template/pull/169

#### 0.30.1 - 29.10.2018

* Restrict frameworks in paket.dependencies - https://github.com/SAFE-Stack/SAFE-template/pull/163

#### 0.30.0 - 15.10.2018

* Upgrade to Fable 2 - https://github.com/SAFE-Stack/SAFE-template/issues/144

#### 0.29.1 - 26.09.2018

* Add polyfill to webpack to support fetch in IE 11 - https://github.com/SAFE-Stack/SAFE-template/issues/155
* Use Microsoft.NET.Sdk.Web project type for all template options - https://github.com/SAFE-Stack/SAFE-template/issues/153

#### 0.29.0 - 25.09.2018

* Change 'remoting' parameter to 'communication' - https://github.com/SAFE-Stack/SAFE-template/pull/143
* Don't install dotnet in build.fsx

#### 0.28.0 - 19.09.2018

* Update paket dependencies - https://github.com/SAFE-Stack/SAFE-template/pull/151

#### 0.27.1 - 14.08.2018

* Fix 'fulma-admin' layout - https://github.com/SAFE-Stack/SAFE-template/issues/128
* Add lock file for NPM
* Move "remotedev" dependency to "devDependencies" - https://github.com/SAFE-Stack/SAFE-template/issues/113

#### 0.27.0 - 10.08.2018

* Add paket.lock for all possible template options - https://github.com/SAFE-Stack/SAFE-template/pull/139

#### 0.26.2 - 27.07.2018

* Allow symlinks in webpack config to overcome issue with NuGet feed on a symlink-ed directory - https://github.com/SAFE-Stack/SAFE-template/pull/133
* Remove intersperse function from Client to make life easier for new-comers - https://github.com/SAFE-Stack/SAFE-template/pull/134

#### 0.26.1 - 25.07.2018

* Remove redundant code from client - https://github.com/SAFE-Stack/SAFE-template/pull/126
* Use Async.Sleep instead of Thread.Sleep in build.fsx - https://github.com/SAFE-Stack/SAFE-template/pull/129
* Replace Menu.item with Menu.Item.a - https://github.com/SAFE-Stack/SAFE-template/pull/130

#### 0.26.0 - 23.07.2018

* Update Fable.Remoting to newest version - https://github.com/SAFE-Stack/SAFE-template/pull/118

#### 0.25.0 - 23.07.2018

* Use Saturn 0.7.4 in default template - https://github.com/SAFE-Stack/SAFE-template/issues/124

#### 0.24.2 - 11.07.2018

* Pin compatible Remoting.Server to 3.6 - https://github.com/SAFE-Stack/SAFE-template/pull/117

#### 0.24.1 - 10.07.2018

* Escape `dotnet publish` arguments - https://github.com/SAFE-Stack/SAFE-template/pull/115

#### 0.24.0 - 29.06.2018

* update Fable.Remoting dependencies - https://github.com/SAFE-Stack/SAFE-template/pull/112
* fix build script intellisense on Mono

#### 0.23.0 - 26.06.2018

* include paket.lock file for default options and azure option - https://github.com/SAFE-Stack/SAFE-template/pull/109

#### 0.22.0 - 20.06.2018

* update to dotnet SDK 2.1.300 and FAKE 5 - https://github.com/SAFE-Stack/SAFE-template/pull/101

#### 0.21.4 - 18.06.2018

* add 'disable_diagnostics' option to saturn application when Fable.Remoting is enabled - https://github.com/SAFE-Stack/SAFE-template/pull/105

#### 0.21.3 - 07.06.2018

* correct indentations for Azure option in build.fsx - https://github.com/SAFE-Stack/SAFE-template/issues/102

#### 0.21.2 - 27.05.2018

* Normalize the indentations - https://github.com/SAFE-Stack/SAFE-template/issues/92

#### 0.21.1 - 24.05.2018

* Remove redundant static files configuration for Saturn - https://github.com/SAFE-Stack/SAFE-template/pull/95

#### 0.21.0 - 17.05.2018

* Use lower-case parameter names - https://github.com/SAFE-Stack/SAFE-template/pull/94
* Change 'npm' parameter to 'js-deps'
* Change 'fulma' parameter to 'layout'

#### 0.20.1 - 14.05.2018

* Open missing namespace for combination of Saturn and Fable.Remoting - https://github.com/SAFE-Stack/SAFE-template/pull/93

#### 0.20.0 - 11.05.2018

* Use newest Fulma - https://github.com/SAFE-Stack/SAFE-template/pull/89

#### 0.19.1 - 10.05.2018

* Add solution file - https://github.com/SAFE-Stack/SAFE-template/issues/86

#### 0.19.0 - 10.05.2018

* Add support for Azure storage - https://github.com/SAFE-Stack/SAFE-template/pull/77

#### 0.18.1 - 08.05.2018

* Make 'basic' the default value for 'Fulma' parameter - https://github.com/SAFE-Stack/SAFE-template/issues/87
* Added Rider stuff to gitignore - https://github.com/SAFE-Stack/SAFE-template/pull/88
* Enable AppInsights only if Deploy option is azure for Suave - https://github.com/SAFE-Stack/SAFE-template/pull/81

#### 0.18.0 - 03.05.2018

* Update webpack to 4 - https://github.com/SAFE-Stack/SAFE-template/pull/80

#### 0.17.0 - 03.05.2018

* Add release scripts to .gitignore - https://github.com/SAFE-Stack/SAFE-template/pull/74

#### 0.16.0 - 02.05.2018

* Support for Azure App Insights - https://github.com/SAFE-Stack/SAFE-template/issues/68
* Fix file paths for Unix in additional Azure FAKE targets - https://github.com/SAFE-Stack/SAFE-template/pull/72

#### 0.15.0 - 01.05.2018

* Add Azure App Service PAAS deployment - https://github.com/SAFE-Stack/SAFE-template/pull/65
* Update dotnet sdk to 2.1.105
* Simplify Saturn routers

#### 0.14.0 - 23.04.2018

* Add YARN lock file - https://github.com/SAFE-Stack/SAFE-template/pull/61

#### 0.13.1 - 19.04.2018

* Add UTF-8 charset to index.html - https://github.com/SAFE-Stack/SAFE-template/pull/59

#### 0.13.0 - 14.04.2018

* Make Saturn the default option for Server - https://github.com/SAFE-Stack/SAFE-template/issues/54

#### 0.12.0 - 07.04.2018

* Update Fable.Remoting and use latest API - https://github.com/SAFE-Stack/SAFE-template/pull/53

#### 0.11.0 - 07.04.2018

* Add Fable Json serialization to Saturn and Giraffe - https://github.com/SAFE-Stack/SAFE-template/pull/50

#### 0.10.4 - 07.04.2018

* Fix path to index.html in Saturn template - https://github.com/SAFE-Stack/SAFE-template/pull/52

#### 0.10.3 - 30.03.2018

* Add type signatures for functions in Client

#### 0.10.2 - 27.03.2018

* Add SAFE favicon

#### 0.10.1 - 25.03.2018

* [Disable packages folder](https://fsprojects.github.io/Paket/dependencies-file.html#Disable-packages-folder) for Server and Client dependencies

#### 0.10.0 - 13.03.2018

* Initial support for `Saturn` backend option

#### 0.9.1 - 03.03.2018

* Correct Bulma Column classes from `Column.Desktop` to `Column.All`

#### 0.9.0 - 27.02.2018

* Add 3 bulma templates `admin` `cover` and `login`  - https://github.com/SAFE-Stack/SAFE-template/issues/27

#### 0.8.5 - 25.02.2018

* [Giraffe] add to services to prevent NRE - https://github.com/SAFE-Stack/SAFE-template/pull/43
* Add floating dependency on Giraffe - `~> 1`

#### 0.8.4 - 24.02.2018

* Reinstate postActions to make scripts executable on non-windows OSs - https://github.com/SAFE-Stack/SAFE-template/pull/37

#### 0.8.3 - 24.02.2018

* Update paket bootstrapper to version 5.142.0 - https://github.com/SAFE-Stack/SAFE-template/pull/38

#### 0.8.2 - 20.02.2018

* Revert Make build.sh executable via post-action - https://github.com/SAFE-Stack/SAFE-template/issues/36

#### 0.8.1 - 19.02.2018

* Make build.sh executable via post-action - https://github.com/SAFE-Stack/SAFE-template/pull/34

#### 0.8.0 - 31.01.2018

* Update Fulma to 1.0.0-beta-007

#### 0.7.0 - 28.01.2018

* `NPM` option to use NPM instead of default Yarn

#### 0.6.0 - 28.01.2018

* `Docker` option for additional FAKE targets to build Docker image

#### 0.5.3 - 22.01.2018

* Add `module Client` back to `Client.fs` - https://github.com/SAFE-Stack/SAFE-template/pull/30

#### 0.5.2 - 22.01.2018

* [Suave] Route `/` (root) requests to `index.html`
* [Suave] Fallback to 404 for unmatched requests
* Rename `App.fs` to `Client.fs` and `Program.fs` to `Server.fs` to better distinguish between the two

#### 0.5.1 - 07.01.2018

* Add `hero` and `landing` bulma templates - https://github.com/SAFE-Stack/SAFE-template/issues/27

#### 0.4.3 - 03.01.2018

* exclude .template.config from sources - https://github.com/SAFE-Stack/SAFE-template/issues/24

#### 0.4.2 - 31.12.2017

* Run `paket install` when `paket.lock` is missing (just after creating from template) - https://github.com/SAFE-Stack/SAFE-template/pull/26

#### 0.4.1 - 31.12.2017

* Replace CRLF with LF in `build.sh` - https://github.com/SAFE-Stack/SAFE-template/issues/25

#### 0.4.0 - 22.12.2017

* Add `Giraffe` as an option next to `Suave` for back-end (`--Server` template option)

#### 0.3.0 - 22.12.2017

* Do not include `Fulma` / `Fable.Remoting` in paket when not specified - https://github.com/SAFE-Stack/SAFE-template/issues/16
* Do not include `paket.lock` - require running manually

#### 0.2.2 - 14.12.2017

* Fable.Remoting usage improvements - https://github.com/SAFE-Stack/SAFE-template/pull/22

#### 0.2.1 - 13.12.2017

* Prevent reinstalling dotnet - https://github.com/SAFE-Stack/SAFE-template/issues/17

#### 0.2.0 - 06.12.2017

* Add `--Remoting` template option

#### 0.1.3 - 02.12.2017

* Add `--Fulma` template option

#### 0.1.2 - 28.11.2017

* Update all paket dependencies

#### 0.1.0 - 28.11.2017

* Add Client <-> Server communication
* Run `dotnet watch` for Server code

#### 0.0.8 - 26.11.2017

* Fix preprocessor directives - https://github.com/SAFE-Stack/SAFE-template/pull/10

#### 0.0.7 - 24.11.2017

* Add Hot Module Replacement
* Create separate "run" target to make it faster - https://github.com/SAFE-Stack/SAFE-template/pull/9

#### 0.0.6 - 29.10.2017

* File permissions for build script - https://github.com/SAFE-Stack/SAFE-template/pull/8/files

#### 0.0.5 - 26.10.2017

* Port the Run build target from the Bookstore - https://github.com/SAFE-Stack/SAFE-template/issues/6
* Do not gitignore paket.restore.targets - https://github.com/SAFE-Stack/SAFE-template/issues/7
* Exclude `obj` directories from nupkg

#### 0.0.4 - 24.10.2017

* Exclude paket-files from nupkg

#### 0.0.3 - 23.10.2017

* Remove unwanted CSS classes

#### 0.0.2 - 23.10.2017

* Make the Client code even simpler

#### 0.0.1 - 22.10.2017

* Initial release
