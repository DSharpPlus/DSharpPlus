---
uid: articles.commands.converters.built_in_converters
title: Built-In Converters
---

# Built-In Converters
DSharpPlus provides a number of built-in argument converters for common types. An easy way to remember which converters should be available is ".NET Primitives + Discord Entities." This means that all primitive types in .NET are supported, as well as Discord entities such as `DiscordUser`, `DiscordChannel`, `DiscordRole`, and so on. For an up-to-date list of built-in converters, go to the source and view which files are available: [DSharpPlus.Commands/Converters](https://github.com/DSharpPlus/DSharpPlus/tree/master/DSharpPlus.Commands/Converters). Below is a list of some of the most common built-in converters, and their caveats/quirks:

## .NET Primitives
All .NET primitive types have built-in support. Each primitive type is parsed through `.TryParse`.

### DateTime
The `DateTime` converter does not exist and is not planned to be added. Instead, use `DateTimeOffset` for more accurate time handling.

### Enum
Each enum type that used in a command will have the generic version of `EnumConverter<T>` created at startup. The generic converter is as fast as 30ns per conversion, while the non-generic converter is 60ns per conversion. Enums with less than 25 members have built-in choice provider support. Enums with more than 25 members will have auto-complete support.

### `long` and `ulong`
`long` and `ulong` are supported, however they will not have the numeric validation that Discord supports with slash commands. This is because the largest number Discord supports is 9,007,199,254,740,992, also known as 2^53 - JavaScript's maximum safe integer. When using `long` or `ulong`, the parameter will appear as a string in the Discord Client, however the argument converters will still ensure that the value is a valid number.

### TimeSpan
While `TimeSpan` does use `TimeSpan.TryParse` just as all the other primitives do, there is additional supported syntax ported over from CommandsNext: `1d2h3m4s5ms` will be parsed as `1 day, 2 hours, 3 minutes, 4 seconds, and 5 milliseconds`. The argument converter supports all of the following time units:

| Unit | Of Measurement |
|------|----------------|
| y    | Year           |
| mo   | Month          |
| w    | Week           |
| d    | Day            |
| h    | Hour           |
| m    | Minute         |
| s    | Second         |
| ms   | Millisecond    |
| us   | Microsecond    |
| µs   | Microsecond    |
| ns   | Nanosecond     |

### Discord Entities

The most common Discord entities have built-in converters. Almost all of these entities can be used by passing in the entity's ID, mention, or name. The following entities have built-in converters:

- `DiscordAttachment`
- `DiscordChannel`
- `DiscordEmoji`
- `DiscordGuild`
- `DiscordMember`
- `DiscordMessage`*
- `DiscordRole`
- `DiscordSnowflakeObjectConverter`*
- `DiscordThreadChannel`
- `DiscordUser`

By default, if the entity is not found, it will attempt to make an API rest request to fetch the entity.

### DiscordMessage
`DiscordMessage` can be parsed by passing a message link to any message that both the user and the bot can access. Alternatively, you can pass the message id if the message is in the current channel. If the parameter has the `TextMessageReply` attribute applied to it, then the message can be parsed by replying to the message with the command.

### DiscordSnowflakeObjectConverter
`DiscordSnowflakeObjectConverter` is a converter that supports parsing `DiscordUser`, `DiscordMember`, `DiscordChannel`, `DiscordThreadChannel` and `DiscordRole`.