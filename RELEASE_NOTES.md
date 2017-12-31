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