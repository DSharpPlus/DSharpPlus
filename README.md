![DSharpPlus Logo](./res/dsharp+.png)
--- 
An unofficial wrapper for the Discord API, written in C#.

# Warnings
Version 5.0 isn't done yet! It's not even in a working state. You're welcome to contribute, however it's recommended to use v4 for now.

If you wish to contribute to v5, please follow our [Contributing Guidelines](./CONTRIBUTING.md) and look at our [Development Plan](./Development\ Plan.md). If you want to help, but unsure how, you're welcome to join the [Discord](https://discord.gg/dsharpplus) and ask "How can I help with v5" in #lib-discussion.

# Installing
1. You can get both the stable and pre-release versions from [Nuget](https://nuget.org/packages/DSharpPlus). Pre-releases reflect the latest git commit. While each commit is thoroughly tested, there are no promises for there not to be breaking changes. It's recommended to test your nightly upgrades before using them in production.
```
dotnet add package DSharpPlus
 ```
2. You can clone the library from Github and do a local project reference.
```
git clone git@github.com:DSharpPlus/DSharpPlus.git ../DSharpPlus
dotnet add reference ../DSharpPlus/DSharpPlus.Core/DSharpPlus.Core.csproj
```

# Documentation
Documentation for the latest stable version can be found at the [DSharpPlus Github.io website](https://dsharpplus.github.io). This means that v5 is not yet reflected onto it.

Each class, method and property should have XML documentation on it, meaning if your editor/IDE has intellisense, you should be able to hover over it and read the docs. Usually v5 docs reflect the Discord API documentation, however it's entirely possible that we may add onto it for further clarity.

# Examples
As of the time of writing, there are no examples or bots written in v5. Come back at a later date, once v5 is usable.

# Support
You can ask for support over at our Discord:
![DSharpPlus Discord Guild Invite](https://discord.com/api/guilds/379378609942560770/embed.png?style=banner2)

Alternatively you can join the Discord API guild (Not Discord Developers) and ask in the #dotnet_dsharpplus channel:
![Discord API Guild Invite](https://discord.com/api/guilds/81384788765712384/embed.png?style=banner2)