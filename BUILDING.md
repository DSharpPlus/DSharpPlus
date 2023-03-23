# Building DSharpPlus

These are detailed instructions on how to build the DSharpPlus library under various environmnets.

It is recommended you have prior experience with multi-target .NET Core/Standard projects, as well as the `dotnet` CLI utility, and MSBuild.

## Requirements

In order to build the library, you will first need to install some software.

### Windows

On Windows, we only officially support Visual Studio 2017 15.3 or newer. Visual Studio Code and other IDEs might work, but are generally not supported or even guaranteed to work properly.

* **Windows 10** - while we support running the library on Windows 7 and above, we only support building on Windows 10.
* [**Git for Windows**](https://git-scm.com/download/win) - required to clone the repository.
* [**Visual Studio 2017**](https://www.visualstudio.com/downloads/) - community edition or better. We do not support Visual Studio 2015 and older. Note that to build the library, you need Visual Studio 2017 version 15.3 or newer.
  * **Workloads**:
    * **.NET Framework Desktop** - required to build .NETFX (4.5, 4.6, and 4.7 targets)
    * **.NET Core Cross-Platform Development** - required to build .NET Standard targets (1.1, 1.3, and 2.0) and the project overall.
  * **Individual Components**:
    * **.NET Framework 4.5 SDK** - required for .NETFX 4.5 target
    * **.NET Framework 4.6 SDK** - required for .NETFX 4.6 target
    * **.NET Framework 4.7 SDK** - required for .NETFX 4.7 target
* [**.NET Core SDK 2.0**](https://www.microsoft.com/net/download) - required to build the project.
* **Windows PowerShell** - required to run the build scripts. You need to make sure your script execution policy allows execution of unsigned scripts.

### GNU/Linux

On GNU/Linux, we support building via Visual Studio Code and .NET Core SDK. Other IDEs might work, but are not supported or guaranteed to work properly.

While these should apply to any modern distribution, we only test against Debian 10. Your mileage may vary.

When installing the below, make sure you install all the dependencies properly. We might ship a build environmnent as a docker container in the future.

* **Any modern GNU/Linux distribution** - like Debian 9.
* **Git** - to clone the repository.
* [**Visual Studio Code**](https://code.visualstudio.com/Download) - a recent version is required.
  * **C# for Visual Studio Code (powered by OmniSharp)** - required for syntax highlighting and basic Intellisense
* [**.NET Core SDK 2.0**](https://www.microsoft.com/net/download) - required to build the project.
* [**Mono 5.x**](http://www.mono-project.com/download/#download-lin) - required to build the .NETFX 4.5, 4.6, and 4.7 targets, as well as to build the docs.
* [**PowerShell Core**](https://docs.microsoft.com/en-us/powershell/scripting/setup/Installing-PowerShell-Core-on-macOS-and-Linux?view=powershell-6) - required to execute the build scripts.
* **p7zip-full** - required to package docs.

## Instructions

Once you install all the necessary prerequisites, you can proceed to building. These instructions assume you have already cloned the repository.

### Windows

Building on Windows is relatively easy. There's 2 ways to build the project:

#### Building through Visual Studio

Building through Visual Studio yields just binaries you can use in your projects.

1. Open the solution in Visual Studio.
2. Set the configuration to Release.
3. Select Build > Build Solution to build the project.
4. Select Build > Publish DSharpPlus to publish the binaries.

#### Building with the build script

Building this way outputs NuGet packages, and a documentation package. Ensure you have an internet connection available, as the script will install programs necessary to build the documentation.

1. Open PowerShell and navigate to the directory which you cloned DSharpPlus to.
2. Execute `.\rebuild-all.ps1 -configuration Release` and wait for the script to finish execution.
3. Once it's done, the artifacts will be available in *dsp-artifacts* directory, next to the directory to which the repository is cloned.

### GNU/Linux

When all necessary prerequisites are installed, you can proceed to building. There are technically 2 ways to build the library, though both of them perform the same steps, they are just invoked slightly differently.

#### Through Visual Studio Code

1. Open Visual Studio Code and open the folder to which you cloned DSharpPlus as your workspace.
2. Select Build > Run Task...
3. Select `buildRelease` task and wait for it to finish.
4. The artifacts will be placed in *dsp-artifacts* directory, next to whoch the repository is cloned.

#### Through PowerShell

1. Open PowerShell (`pwsh`) and navigate to the directory which you cloned DSharpPlus to.
2. Execute `.\rebuild-all.ps1 -configuration Release` and wait for the script to finish execution.
3. Once it's done, the artifacts will be available in *dsp-artifacts* directory, next to the directory to which the repository is cloned.
