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