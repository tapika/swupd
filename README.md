# swupd - like yum or apt-get, but for Windows

This is unofficial fork of choco.

Branch  | AppVeyor | Coverage
------------- | ------------- | -------------
master | [![AppVeyor Build Status](https://ci.appveyor.com/api/projects/status/44so8j4tsi0k2bx0/branch/master?svg=true)](https://ci.appveyor.com/project/tapika/swupd/branch/master) |  [![Coverage Status](https://coveralls.io/repos/github/tapika/swupd/badge.svg?branch=master)](https://coveralls.io/github/tapika/swupd?branch=master)
develop | [![AppVeyor Build Status](https://ci.appveyor.com/api/projects/status/44so8j4tsi0k2bx0/branch/develop?svg=true)](https://ci.appveyor.com/project/tapika/swupd/branch/develop) | [![Coverage Status](https://coveralls.io/repos/github/tapika/swupd/badge.svg?branch=develop)](https://coveralls.io/github/tapika/swupd?branch=develop)



### Compiling / Building Source on Windows

#### From Visual Studio

There are multiple solutions at the moment `chocolatey.sln` for .NET Framework 4.8, and others `chocolatey_*.sln` which targets to same .net platform
which is set in solution name.

#### From command line

Run `build.bat` from root folder.

#### Running existing build

Check releases, and download latest suitable for you release.

 * choco.exe - Windows 64-bit executable ( Windows 7 64-bit or later )
 * choco - Linux 64-bit executable ( CentOS, Debian, Fedora, Ubuntu, and derivatives )

Both images are built using `ReadyToRun` technology - meaning you don't need to preinstall anything on your OS.

#### Building `ReadyToRun` executables

| `build.bat` argument     | Description                             |
| ------------------------ | --------------------------------------- |
| `buildexe_choco_win7`    | `choco` command line tool for Windows * |
| `buildexe_choco_linux`   | `choco` command line tool for Linux     |
| `buildexe_chocogui_win7` | `Chocolatey UI` for Windows 7 or higher |
| `buildexe_chocogui_win81` | `Chocolatey UI` for Windows 8.1 or higher |

Using additionally `-net net5.0`, `-net net6.0` can force for build to happen using specific .net platform for correspondent solution.

