# swupd - like yum or apt-get, but for Windows

This is unofficial fork of choco.

Branch  | Github Actions | Coverage
------------- | ------------- | -------------
master | [![Github Builds Status](https://github.com/tapika/swupd/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/tapika/swupd/actions/workflows/build.yml) |  [![Coverage Status](https://coveralls.io/repos/github/tapika/swupd/badge.svg?branch=master)](https://coveralls.io/github/tapika/swupd?branch=master)
develop | [![Github Builds Status](https://github.com/tapika/swupd/actions/workflows/build.yml/badge.svg?branch=develop)](https://github.com/tapika/swupd/actions/workflows/build.yml) | [![Coverage Status](https://coveralls.io/repos/github/tapika/swupd/badge.svg?branch=develop)](https://coveralls.io/github/tapika/swupd?branch=develop)



### Compiling / Building Source on Windows

#### From Visual Studio

There are multiple solutions at the moment `chocolatey.sln` for .NET Framework 4.8, and others `chocolatey_*.sln` which targets to same .net platform which is set in solution name.

#### From command line

Run `cakebuild.bat` from root folder. Use additionally `--help` to get some help on command line arguments.

#### Running existing build

Check releases, and download latest suitable for you release.

 * choco.exe - Windows 64-bit executable ( Windows 7 64-bit or later )
 * choco - Linux 64-bit executable ( CentOS, Debian, Fedora, Ubuntu, and derivatives )

Both images are built using `ReadyToRun` technology - meaning you don't need to preinstall anything on your OS.

#### Building `ReadyToRun` executables

| `cakebuild` arguments | Description                             |
| ------------------------ | --------------------------------------- |
| `cakebuild --build false --r2r_targets choco --os win7 --r2r_build` | `choco` command line tool for Windows * |
| `cakebuild --build false --r2r_targets choco --os linux --r2r_build` | `choco` command line tool for Linux     |
| `cakebuild --build false --r2r_targets chocogui --os win7 --r2r_build` | `Chocolatey UI` for Windows 7 or higher |
| `cakebuild --build false --r2r_targets chocogui --os win81 --r2r_build` | `Chocolatey UI` for Windows 8.1 or higher |

Using additionally `--net net5.0`, `--net net6.0` can force for build to happen using specific .net platform for correspondent solution.

Potential compatibility issues when testing `ReadyToRun` builds:

* If you build `ReadyToRun` executable for windows 8.1 and run it on windows 7 - it might not run, because of missing runtime redistributables.

* If you build `ReadyToRun` executable for windows 10 and run it on windows 7 - it still might run, as nuget package might list maximum required windows version, but application itself does not use all the features of nuget package, so everything works ok on windows 7 as well.

##### Known ReadyToRun problems

Problem: `Single File publishing is not compatible with Windows 7.`

Solution: Create `global.json` file with following content:

```json
{
  "sdk": {
    "version": "5.0.403",
    "rollForward": "latestFeature"
  }
}
```

Open issue, see ticket:  [dotnet runtime #62453](https://github.com/dotnet/runtime/issues/62453).

### Testing

Documented in [here](docs/testing.md)
