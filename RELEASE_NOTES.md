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