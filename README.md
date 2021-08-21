![Logo of DSharpPlus](https://github.com/DSharpPlus/DSharpPlus/raw/master/logo/dsharp+_smaller.png)

# DSharpPlus
An unofficial .NET wrapper for the Discord API, based off [DiscordSharp](https://github.com/suicvne/DiscordSharp), but rewritten to fit the API standards.

[![Build Status](https://img.shields.io/appveyor/ci/DSharpPlus/dsharpplus/master.svg)](https://ci.appveyor.com/project/DSharpPlus/dsharpplus/branch/master)
[![Discord Server](https://img.shields.io/discord/379378609942560770.svg?label=discord)](https://discord.gg/dsharpplus) 
[![SlimGet](https://img.shields.io/badge/dynamic/json.svg?color=yellow&label=slimget&query=%24.items%5B-1%3A%5D.upper&url=https%3A%2F%2Fnuget.emzi0767.com%2Fapi%2Fv3%2Fregistration%2FPlain%2Fdsharpplus%2Findex.json)](https://nuget.emzi0767.com/gallery/search?q=DSharpPlus&pre=true)
[![NuGet](https://img.shields.io/nuget/vpre/DSharpPlus.svg)](https://nuget.org/packages/DSharpPlus)

# Installing
You can install the library from following sources:

1. The latest nightly release is available on [SlimGet](https://nuget.emzi0767.com/gallery/packages). These are cutting-edge versions automatically built from the latest commit in the `master` branch in this repository, and as such always contains the latest changes.

   Despite the nature of pre-release software, all changes to the library are held under a level of scrutiny; for this library, unstable does not mean bad quality, rather it means that the API can be subject to change without prior notice (to ease rapid iteration) and that consumers of the library should always remain on the latest version available (to immediately get the latest fixes and improvements). You will usually want to use this version.
2. The latest stable release is always available on [NuGet](https://nuget.org/packages/DSharpPlus). This branch is less up-to-date than the nightly versions, but is guaranteed to not receive any breaking API changes without a major version bump.

   Critical bugfixes in the nightly releases will usually be backported to the latest major stable release, but only after they have passed our soak tests. Additionally, some smaller fixes may be infrastructurally impossible or very difficult to backport without "breaking everything", and as such they will remain only in the nightly release until the next major release. You should evaluate whether or not this version suits your specific needs.
3. The library can be compiled from source on Windows using Visual Studio 2017 or Visual Studio 2019. Compilation on Mac and GNU/Linux devices is possible using the .NET Core SDK, but you will only be able to build for the .NET Standard targets.

   On Windows, you will need SDKs for .NET 4.5, 4.6, and 4.7, as well as the .NET Core 1.1 and 2.0 SDKs. You can install these manually from the internet, or through the Visual Studio Installer.

# Documentation
The documentation for the latest stable version is available at [dsharpplus.github.io](https://dsharpplus.github.io/).

Do note that the documentation might not reflect the latest changes in nightly version of the library.

## Resources
The following resources apply only for the latest stable version of the library.

### Tutorials
* [Making your first bot in C#](https://dsharpplus.github.io/articles/basics/bot_account.html).

### Example bots
* [Example by Emzi0767](https://github.com/DSharpPlus/Example-Bots)

# I want to throw my money at you!
If you want to give us some money as a thank you gesture, you can do so using one of these links:

* Naamloos
   * [Ko-Fi](https://ko-fi.com/naamloos)
* Emzi0767
   * [Ko-Fi](https://ko-fi.com/emzi0767)
   * [PayPal](https://paypal.me/Emzi0767/5USD)
   * [Patreon](https://patreon.com/emzi0767)

# Questions?
Come talk to us here:

[![DSharpPlus Chat](https://discord.com/api/guilds/379378609942560770/embed.png?style=banner1)](https://discord.gg/dsharpplus)

Alternatively, you could also join us in the [Discord API chat](https://discord.gg/discord-api) at **#dotnet_dsharpplus**.

[![Discord API Chat](https://discord.com/api/guilds/81384788765712384/embed.png?style=banner1)](https://discord.gg/discord-api)
