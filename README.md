![Logo of DSharpPlus](https://github.com/DSharpPlus/DSharpPlus/raw/master/logo/dsharp+_smaller.png)

# DSharpPlus
An unofficial .NET wrapper for the Discord API, based off [DiscordSharp](https://github.com/suicvne/DiscordSharp), but rewritten to fit the API standards.

[![Build Status](https://img.shields.io/appveyor/ci/Emzi0767/dsharpplus/master.svg)](https://ci.appveyor.com/project/Emzi0767/dsharpplus/branch/master)
[![Discord Server](https://img.shields.io/discord/379378609942560770.svg?label=discord)](https://discord.gg/KeAS3pU) 
[![MyGet](https://img.shields.io/myget/dsharpplus-nightly/vpre/DSharpPlus.svg?label=myget)](https://www.myget.org/gallery/dsharpplus-nightly)
[![NuGet](https://img.shields.io/nuget/vpre/DSharpPlus.svg)](https://nuget.org/packages/DSharpPlus)

# Installing
You can install the library from following sources:

1. The latest nightly release is available on [MyGet](https://www.myget.org/gallery/dsharpplus-nightly). These are cutting-edge versions automatically built from the latest commit in the `master` branch in this repository, and as such always contains the latest changes.

   Despite the nature of pre-release software, all changes to the library are held under a level of scrutiny; for this library, unstable does not mean bad quality, rather it means that the API can be subject to change without prior notice (to ease rapid iteration) and that consumers of the library should always remain on the latest version available (to immediately get the latest fixes and improvements). You will usually want to use this version.
2. The latest stable release is always available on [NuGet](https://nuget.org/packages/DSharpPlus). This branch is less up-to-date than the nightly versions, but is guaranteed to not receive any breaking API changes without a major version bump.

   Critical bugfixes in the nightly releases will usually be backported to the latest major stable release, but only after they have passed our soak tests. Additionally, some smaller fixes may be infrastructurally impossible or very dificult to backport without "breaking everything", and as such they will remain only in the nightly release until the next major release. You should evaluate whether or not this version suits your specific needs.
3. The library can be compiled from source on Windows using Visual Studio 2017 or Visual Studio 2019. Compilation on Mac and GNU/Linux devices is possible using the .NET Core SDK, but you will only be able to build for the .NET Standard targets.

   On Windows, you will need SDKs for .NET 4.5, 4.6, and 4.7, as well as the .NET Core 1.1 and 2.0 SDKs. You can install these manually from the internet, or through the Visual Studio Installer.

# Documentation
The documentation for the latest stable version is available at [dsharpplus.emzi0767.com](https://dsharpplus.emzi0767.com).

For the latest nightly build, you can find it at [dsharpplus.github.io](https://dsharpplus.github.io/). Do note that the articles for the nightly builds may not yet reflect recent API changes. The API Documentation however is automatically generated and should always be up-to-date.

## Resources
The following resources apply only for the latest stable version of the library.

### Tutorials
* [Making your first bot in C#](https://dsharpplus.emzi0767.com/articles/intro.html).

### Example bots
* [Example by Emzi0767](https://github.com/DSharpPlus/Example-Bots)
* [Example by Naamloos](https://github.com/DSharpPlus/DSharpPlus-Example)

# Questions?
Come talk to us here:

[![DSharpPlus Chat](https://discordapp.com/api/guilds/379378609942560770/embed.png?style=banner1)](https://discord.gg/nTk7HgF)

Alternatively, you could also join us in the [Discord API chat](https://discord.gg/discord-api) at **#dotnet_dsharpplus**.

[![Discord API Chat](https://discordapp.com/api/guilds/81384788765712384/embed.png?style=banner1)](https://discord.gg/discord-api)
