![Logo of DSharpPlus](https://raw.githubusercontent.com/DSharpPlus/DSharpPlus/refs/heads/master/logo/dsharp%2B_smaller.png)

# DSharpPlus

An unofficial .NET wrapper for the Discord API, the continuation of [DiscordSharp](https://github.com/suicvne/DiscordSharp).

[![Nightly Build Status](https://github.com/DSharpPlus/DSharpPlus/actions/workflows/publish_nightly_master.yml/badge.svg?branch=master)](https://github.com/DSharpPlus/DSharpPlus/actions/workflows/build-commit.yml/badge.svg)
[![Discord Server](https://img.shields.io/discord/379378609942560770.svg?label=Discord&color=506de2)](https://discord.gg/dsharpplus)
[![NuGet](https://img.shields.io/nuget/v/DSharpPlus.svg?label=NuGet)](https://nuget.org/packages/DSharpPlus)
[![NuGet Latest Nightly/Prerelease](https://img.shields.io/nuget/vpre/DSharpPlus?color=505050&label=NuGet%20Latest%20Nightly%2FPrerelease)](https://nuget.org/packages/DSharpPlus)

## Documentation

The documentation for the latest nightly version is available at [dsharpplus.github.io](https://dsharpplus.github.io/DSharpPlus). We do not currently host documentation for stable versions, and you should use nightly versions whenever possible.

## Versions

1. Nightly versions are available on [Nuget](https://www.nuget.org/packages/DSharpPlus/) as a pre-release. These versions contain the latest features, improvements and bugfixes. You should, as a rule, use these versions: despite not being marked as stable, they are held to high standards of quality and merely represent the latest, most up-to-date state of the library.
2. Stable releases are available on NuGet. Important bugfixes will be backported to the latest stable release. Stable versions roughly follow [romantic versioning](https://github.com/romversioning/romver): major versions indicate very major changes to both the library surface and internals, minor versions indicate major additions or breaking changes as necessary and in limited scope, and the third version number indicates hotfixes or small features as necessary.

## Installing locally or from a pull request

You can install the library locally from following sources:

1. The library can be directly referenced from your csproj file. Cloning the repository and referencing the library is as easy as:

    ```sh
    git clone https://github.com/DSharpPlus/DSharpPlus.git DSharpPlus-Repo
    # you can switch to a pull request using
    gh pr <number>
    git checkout <branch-name>
    ```

    Edit MyProject.csproj and add the following line:

    ```xml
    <ProjectReference Include="../DSharpPlus-Repo/DSharpPlus/DSharpPlus.csproj" />
    ```

    This belongs in the ItemGroup tag with the rest of your dependencies. The library should not be in the same directory or subdirectory as your project. This method should only be used if you're making local changes to the library.

2. Every commit on a pull request is built into packages that can be manually downloaded from GitHub and referenced locally:

    1. Navigate to the pull request you are interested in, for example <https://github.com/DSharpPlus/DSharpPlus/pull/2455>.
    2. Click the checkmark next to the latest commit hash, then `Details`.
    3. Click on `Summary` and scroll all the way down. There, an artifact of the format `DSharpPlus-PR-<PR number>-<Incrementing build number>` can be downloaded.
    4. Unzip it and add the folder as a package source:
        
        Via command:
        ```sh
        dotnet add package <package-name> --source "path/to/your/package/folder"
        ```

        Via csproj, in your PropertyGroup:
        ```xml
        <RestoreAdditionalPackageSources>path/to/your/package/folder</RestoreAdditionalPackageSources>
        ```

## Resources

The following resources apply only for the latest stable version of the library.

### Tutorials

* [Making your first bot in C#](https://dsharpplus.github.io/DSharpPlus/articles/basics/bot_account.html).

### Example bots

* [Example by OoLunar](https://github.com/DSharpPlus/Example-Bots)

## Contributing

We're very glad about contributions! Pull requests should generally be made against the `master` branch, which tracks the latest development state of the library, or against a specific feature branch if you wish to collaborate on the feature. Please review the [Contributing Guidelines](CONTRIBUTING.md) and the [Code of Conduct](CODE_OF_CONDUCT.md).

By contributing to DSharpPlus, you agree to make your code available under the MIT License and that you can confer this license upon the code. Code from other people must be used in compliance with their license and clearly marked as such; AI-generated code cannot be submitted.

We generally recommend you come talk to us first, in our [Discord server](https://discord.gg/dsharpplus) at **#lib-development** (the necessary role is available in onboarding or in Channels & Roles), to see whether we would take your changes as proposed before you do any unnecessary work. This is also where we can be reached about all sorts of questions regarding the library's development.

## Extensions

The main DSharpPlus library provides all features of the API except Voice and HTTP interactions. These API features and other utilities are provided via the following additional packages:

Package                      | Description
:---------------------------:|:---:
`DSharpPlus.Commands`        | An extension that provides a command framework for both messages and application commands.
`DSharpPlus.Interactivity`   | An extension that provides utilities for interactive flows with your users..
`DSharpPlus.VoiceNext`       | An extension that enables connecting to Discord voice channels.
`DSharpPlus.Http.AspNetCore` | An extension that provides support for HTTP interactions and webhook events.

Additionally, the following third party extensions and packages are available and are considered by us to be good enough to recommend in good faith:

Package | Description
:---:|:---:
`Lavalink4NET.DSharpPlus` | An extension that provides Lavalink support for DSharpPlus stable versions.
`Lavalink4NET.DSharpPlus.Nightly` | An extension that provides Lavalink support for DSharpPlus nightly versions.

Want to see your extension in the list above? Send a pull request or talk to us on Discord!

## Natives

DSharpPlus also provides a number of native libraries that power certain features or extensions:

Package | Description
:---:|:---:
`DSharpPlus.Natives.Zstd` | Enables zstd compression for the gateway. DSharpPlus will pick up on this by default and use it.
`DSharpPlus.Natives.Sodium` | Provides the required encryption support for VoiceNext and for `DSharpPlus.Http.AspNetCore`.
`DSharpPlus.Natives.Opus` | Provides the native audio encoder for VoiceNext.

These are simply native libraries packaged as nuget packages, so you can use them from anywhere, not only DSharpPlus bots. They provide the targets `{win, linux, linux-musl, osx}-{x64, arm64}`. Note that `x64` packages are built for `x86_64_v2`, which is any CPU newer than about 2008-2012: if you use an older CPU, you may need to build the native libraries yourself.

## I want to throw my money at you

If you want to give us some money as a thank you gesture, you can do so using one of these links:

* Naamloos
    * [Ko-Fi](https://ko-fi.com/naamloos)
* Emzi0767
    * [Ko-Fi](https://ko-fi.com/emzi0767)
    * [PayPal](https://paypal.me/Emzi0767/5USD)
    * [Patreon](https://patreon.com/emzi0767)

## Questions?

Come talk to us here:

[![DSharpPlus Chat](https://discord.com/api/guilds/379378609942560770/embed.png?style=banner1)](https://discord.gg/dsharpplus)

Alternatively, you could also join us in the [Discord API chat](https://discord.gg/discord-api) at **#dotnet_dsharpplus**.

[![Discord API Chat](https://discord.com/api/guilds/81384788765712384/embed.png?style=banner1)](https://discord.gg/discord-api)
