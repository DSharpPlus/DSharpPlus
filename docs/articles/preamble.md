---
uid: articles.preamble
title: Article Preamble
---

## Knowledge Prerequisites

Before attempting to write a Discord bot, you should be familiar with the concepts of [Object Oriented Programing][0],
[the C# programming language][1], and [Task-based Asynchronous Pattern][2].

If you're brand new to C#, or programming in general, this library may prove difficult for you to use. Fortunately,
there are resources that can help you get started with the language!

An excellent tutorial series to go through would be [C# Fundamentals for Absolute Beginners][3] by Bob Tabor. His videos
go through all the basics, from setting up your development environment up to some of the more advanced concepts. If
you're not sure what to do first, Bob's tutorial series should be your starting point!

## Supported .NET Implementations

There are multiple different development and maintenance branches of DSharpPlus targeting different [.NET][4] versions and supported
on different .NET versions.

IF you are using [Unity][7], [Mono][6] or [the .NET Framework][5], no support will be provided, and you might break at any given
moment. .NET Framework 4.6.1 through 4.8.1 *should* work, but we will not provide support or fixes for any issues arising there.
Additionally, if you are using a game engine with C# support (such as Unity), you should consider using the [Discord GameSDK][8]
instead of DSharpPlus.

If you use unsupported software, you are on your own with any arising issues.

### Latest Stable 4.4.2

Version 4.4.2 *should* work on EoL versions Core 3.1 and 5.0 and is supported from version 6.0 upwards.

### Nightly 5.0

5.0 nightly builds target .NET 7.0, and will target the latest stable .NET version going forward.

### 6.0 early work

6.0 work targets the latest available bleeding edge of .NET, and will target the latest stable .NET version once there is a semblance
of completion and stability on the v6 branch.

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
