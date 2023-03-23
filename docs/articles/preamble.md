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

Because DSharpPlus targets .NET Standard 2.0, there are many implementations that may function with DSharpPlus. However,
there are only a few versions we will *explicitly* provide support for.

Implementation      | Support | Notes
:------------------:|:-------:|:------
[.NET][4]      | ✔️      | EoL versions Core 3.1 and 5.0 should work; LTS version 6.0 and STS version 7.0 are supported.
[.NET Framework][5] | ⚠️      | Versions 4.6.1 through 4.8 *should* work fine. However, we do not directly support .NET Framework. We recommend that you use the latest or LTS version of .NET Core.
[Mono][6]           | ❌️       | Has numerous flaws which can break things without warning. If you need a cross platform runtime, use .NET.
[Unity][7]          | ❌️       | Game engines with C# support will never be supported by DSharpPlus. You should consider using the official [Discord GameSDK][8] instead.

If you use an unsupported implementation and encounter issues, you'll be on your own.

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
