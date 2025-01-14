---
uid: articles.preamble
title: Article Preamble
---

>[!NOTE]
> These articles and the [API documentation][11] are built for the latest [nightly][18] version (`v5.0`)

## Knowledge Prerequisites

Before attempting to write a Discord bot, you should be familiar with the concepts of [Object Oriented Programing][0],
[the C# programming language][1], and [Task-based Asynchronous Pattern][2].

If you're brand new to C#, or programming in general, this library may prove difficult for you to use. Fortunately,
there are resources that can help you get started with the language!

An excellent tutorial series to go through would be [C# Fundamentals for Absolute Beginners][3] by Bob Tabor. His videos
go through all the basics, from setting up your development environment up to some of the more advanced concepts. If
you're not sure what to do first, Bob's tutorial series should be your starting point!

## Supported .NET Implementations

There are multiple different branches of DSharpPlus targeting different [.NET][4] versions.

See the table below for supported [.NET implementations][16]:

| DSharpPlus Branch | .NET | .NET Core | .NET Framework |
| :---------- | :-----: | :-----: | :-----: | 
| [Stable][17], `v4.5.X` | `v8.0` - `v9.0`</br>✔️ |  `v3.1`</br>⚠️ | `v4.6.1` - `v4.8.1`</br>⚠️ |
| [Nightly][18], `v5.0` | `v9.0`</br>✔️ | ❌ | ❌ |
| [Future][19], `v6.0` | `v10.0`</br>✔️ | ❌ | ❌ |

<sub> ✔️ `Recommended and supported`  &nbsp;●&nbsp; ⚠️ `Unsupported, might still work` &nbsp;●&nbsp; ❌ `Unsupported, do not use`</sub>

Generally, you should be targeting the latest version of .NET.

.NET Core and [.NET Framework][5] are not directly targeted by DSharpPlus, but may work in some senarios because of the [.NET Standard][20].

Using [Unity][7], [Mono][6], [.NET Framework][5], or any other .NET implementation other than the ones listed with a `✔️` above are _not_ supported by DSharpPlus, and you will be on your own regarding any arising issues.

If you are using a game engine with C# support (such as [Unity][7]), you should consider using the [Discord GameSDK][8] instead of DSharpPlus. 

## Getting Started

If you're writing a Discord bot for the first time, you'll want to start with [creating a bot account][9]. Otherwise, if
you have a bot account already, start off with the [writing your first bot][10] article.

Once you're up and running, feel free to browse through the [API Documentation][11]!

## Support and Questions

You can get in contact with us on Discord through one of the following guilds:

**DSharpPlus Guild**:</br>
[![DSharpPlus Guild][12]][13]

**#dotnet_dsharpplus** on **Discord API**:</br>
[![Discord API / #dotnet_dsharpplus][14]][15]

<!-- LINKS -->

[0]:  https://en.wikipedia.org/wiki/Object-oriented_programming
[1]:  https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/
[2]:  https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap
[3]:  https://channel9.msdn.com/Series/CSharp-Fundamentals-for-Absolute-Beginners
[4]:  https://dotnet.microsoft.com/en-us/
[5]:  https://en.wikipedia.org/wiki/.NET_Framework
[6]:  https://en.wikipedia.org/wiki/Mono_(software)
[7]:  https://en.wikipedia.org/wiki/Unity_(game_engine)
[8]:  https://discord.com/developers/docs/game-sdk/sdk-starter-guide
[9]:  xref:articles.basics.bot_account
[10]: xref:articles.basics.first_bot
[11]: /api/
[12]: https://discordapp.com/api/guilds/379378609942560770/embed.png?style=banner2
[13]: https://discord.gg/dsharpplus
[14]: https://discordapp.com/api/guilds/81384788765712384/embed.png?style=banner2
[15]: https://discord.gg/discord-api
[16]: https://learn.microsoft.com/en-us/dotnet/fundamentals/implementations
[17]: https://github.com/DSharpPlus/DSharpPlus/tree/release/4.5
[18]: https://github.com/DSharpPlus/DSharpPlus/tree/master
[19]: https://github.com/DSharpPlus/DSharpPlus/tree/v6
[20]: https://learn.microsoft.com/en-us/dotnet/standard/net-standard
