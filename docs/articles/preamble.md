---
uid: preamble
title: Article Preamble
---

## Knowledge Prerequisites 
Before attempting to write a Discord bot, you should be familiar with the concepts of [Object Oriented Programing](https://en.wikipedia.org/wiki/Object-oriented_programming), [the C# programming language](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/), and [Task-based Asynchronous Pattern](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap).

If you're brand new to C#, or programming in general, this library may prove difficult for you to use.</br>
Fortunately, there are resources that can help you get started with the language! 

An excellent tutorial series to go through would be [C# Fundamentals for Absolute Beginners](https://channel9.msdn.com/Series/CSharp-Fundamentals-for-Absolute-Beginners) by Bob Tabor.
His videos go through all the basics, from setting up your development environment up to some of the more advanced concepts. 
If you're not sure what to do first, Bob's tutorial series should be your starting point!

## Supported .NET Implementations
Because DSharpPlus targets .NET Standard 2.0, there are many implementations that may function with DSharpPlus.
However, there are only a few versions we will *explicitly* provide support for.

Implementation|Support|Notes
:---: |:---:|:---
[.NET Core](https://en.wikipedia.org/wiki/.NET_Core)|✔️|LTS versions 2.1 and 3.1 are supported.
[.NET Framework](https://en.wikipedia.org/wiki/.NET_Framework)|⚠️|Versions 4.6.1 through 4.8 *should* work fine.<br/>However, we do not directly support .NET Framework.<br/>We recommend that you use the latest LTS version of .NET Core.
[Mono](https://en.wikipedia.org/wiki/Mono_(software))|❌️|Has numerous flaws which can break things without warning.<br/>If you need a cross platform runtime, use .NET Core.
[Unity](https://en.wikipedia.org/wiki/Unity_(game_engine))|❌️|Game engines with C# support will never be supported by DSharpPlus. You should consider using the official [Discord GameSDK](https://discord.com/developers/docs/game-sdk/sdk-starter-guide) instead.

If you use an unsupported implementation and encounter issues, you'll be on your own.

## Getting Started
If you're writing a Discord bot for the first time, you'll want to start with *[creating a bot account](xref:basics_bot_account)*.</br>
Otherwise, if you have a bot account already, start off with the *[writing your first bot](xref:basics_first_bot)* article.</br>

Once you're up and running, feel free to browse through the [API Documentation](/api/index.html)!

## Support and Questions
You can get in contact with us on Discord through one of the following guilds:

**DSharpPlus Guild**:</br>
[![DSharpPlus Guild](https://discordapp.com/api/guilds/379378609942560770/embed.png?style=banner2)](https://discord.gg/KeAS3pU)

**#dotnet_dsharpplus** on **Discord API**:</br>
[![Discord API / #dotnet_dsharpplus](https://discordapp.com/api/guilds/81384788765712384/embed.png?style=banner2)](https://discord.gg/discord-api)

