---
uid: articles.migration.3x_to_4x
title: Migration 3.x - 4.x
---

## Migration From 3.x to 4.x
### Proxy Support
The client now supports proxies for both WebSocket and HTTP traffic. To proxy your traffic, create a new instance of
`System.Net.WebProxy` and assign it to @DSharpPlus.DiscordConfiguration.Proxy property.

### Module Rename
3.x                   | 4.x
:--------------------:|:------------------------:
`CommandsNextModule`  | `CommandsNextExtension`
`InteractivityModule` | `InteractivityExtension`
`VoiceNextClient`     | `VoiceNextExtension`
`BaseModule`          | `BaseExtension`

### Intents
Due to a change by Discord on their V8 endpoint which DSharpPlus targets, in order to recieve events, intents will have
to be enabled in both the @DSharpPlus.DiscordConfiguration and the Discord Application Portal. We have an [article][0]
that covers all that has to be done to set this up.

### Event Handlers
The signitures for all the event handlers have changed to have 2 parameters instead of one. Please refer to this
[article][1] for the changes.

### Entity mutation changes
Entity updating methods now take an action which mutates the state of the object, instead of taking large lists of
arguments. This means that instead of updating e.g. a role like this:
```cs
await role.UpdateAsync(name: "Modified Role", color: new DiscordColor(0xFF00FF));
```

you will update it like this:
```cs
await role.UpdateAsync(x =>
{
    x.Name = "Modified Role";
    x.Color = new DiscordColor(0xFF00FF);
});
```

### SendMessageAsync and SendFileAsync Methods
We now have a message builder that will handle any advanced creating and modifing of messages. Below are the overloads
for sending and modifing messages:
1. Sending Messages
	* `.SendMessageAsync(DiscordChannel, DiscordEmbed)`
	* `.SendMessageAsync(DiscordChannel, System.String)`
	* `.SendMessageAsync(DiscordChannel, System.String,DiscordEmbed)`
	* `.SendMessageAsync(DiscordChannel, DiscordMessageBuilder)`
2. Modifying Messages
	* `DiscordMessage.ModifyAsync(DSharpPlus.Entities.Optional<DSharpPlus.Entities.DiscordEmbed>)`
	* `DiscordMessage.ModifyAsync(DSharpPlus.Entities.Optional<string>)`
	* `DiscordMessage.ModifyAsync(DSharpPlus.Entities.Optional<string>, DSharpPlus.Entities.Optional<DSharpPlus.Entities.DiscordEmbed>)`
	* `DiscordMessage.ModifyAsync(DSharpPlus.Entities.DiscordMessageBuilder)`

Using the @DSharpPlus.Entities.DiscordMessageBuilder can be found [here][2].

### Logging Changes
Logging was overhauled and now some of the Properties on @DSharpPlus.DiscordConfiguration along with some of the events
on @DSharpPlus.DiscordClient are Gone/Modified/Added. Below is a listing of what changed:
- **@DSharpPlus.DiscordConfiguration.LoggerFactory** - this is where you can specify your own logging factory to help
  augment the output of the log messages, redirect the output to other locations, etc.
- **@DSharpPlus.DiscordConfiguration.MinimumLogLevel** - this replaces LogLevel
- **DebugLogger** - this has been removed.
- **UseInternalLogHandler** - this has been removed.
- **DebugLogMessageEventArgs** - this event has been removed.

We have also created an [article][3] on how to setup the new logger.

### Other minor changes
- **User DM handling** - Users can no longer be DM'd directly. Instead, you will need to find a member object for the
  user you want to DM, then use the appropriate methods on the member object.
- **Channel permission override enhancements** - You can now query the member or role objects for each permission
  override set on channels. Furthermore, the overwrite building is now more intuitive.
- **Indefinite reconnecting** - the client can now be configured to attempt reconnecting indefinitely.
- **Channel.Users** - you can now query users in voice and text channels by using
  @DSharpPlus.Entities.DiscordChannel.Users property.
- **SendFileAsync argument reordering** - arguments for these methods were reordered to prevent overload confusion.
- **New Discord features** - support for animated emoji and slow mode.

## CommandsNext
There were several major changes made to CommandsNext extension. While basics remain the same, some finer details are
different.

### Multiprefix support
Prefixes are now configured via @DSharpPlus.CommandsNext.CommandsNextConfiguration.StringPrefixes instead of old
`StringPrefix` property. Prefixes passed in this array will all function at the same time. At the same time,
@DSharpPlus.CommandsNext.CommandContext class has been augmented with @DSharpPlus.CommandsNext.CommandContext.Prefix
property, which allows for checking which prefix was used to trigger the command. Furthermore, the new
@DSharpPlus.CommandsNext.Attributes.RequirePrefixesAttribute can be used as a check to require a specific prefix to be
used with a command.

### Command hiding inheritance
Much like checks, the @DSharpPlus.CommandsNext.Attributes.HiddenAttribute is now inherited in modules which are not
command groups.

### Support for `Nullable<T>` and `System.Uri` conversion
The default argument converters have been augmented to allow for conversion of nullable value types. No further
configuration is required.

Furthermore, native support for `System.Uri` type now exists as well.

### Dependency Injection changes
CommandsNext now uses Microsoft's Dependency Injection abstractions, which greatly enhances flexibility, as well as
allows 3rd party service containers to be used. For more information, see [Dependency injection][4] page.

### Command overloads and group commands
Command overloads are now implemented. This means you can create a command which takes multiple various argument type
configurations. This is done by creating several commands and giving them all the same name.

Overloads need to have unique argument configurations, which means that it is possible to create commands which use the
same argument types in different order (e.g. `int, string` and `string, int`), however you cannot create two overloads
which have the same argument types and order.

Checks are pooled between all overloads, which means that specifying the same check on every overload will make it run
several times; if you apply a check to a single overload, it will apply to all of them.

Group command is also done by marking a command with @DSharpPlus.CommandsNext.Attributes.GroupCommandAttribute instead
of regular `CommandAttribute`. They can also be overloaded.

### Common module base
All command modules are now required to inherit from @DSharpPlus.CommandsNext.BaseCommandModule. This also enables the
modules to use @DSharpPlus.CommandsNext.BaseCommandModule.BeforeExecutionAsync(DSharpPlus.CommandsNext.CommandContext)
and @DSharpPlus.CommandsNext.BaseCommandModule.AfterExecutionAsync(DSharpPlus.CommandsNext.CommandContext).

### Module lifespans
It is now possible to create transient command modules. As opposed to regular singleton modules, which are instantiated
upon registration, these modules are instantiated before every command call, and are disposed shortly after.

Combined with dependency injection changes, this enables the usage of transient and scoped modules.

For more information, see [Module lifespans][5] page.

### Help formatter changes
Help formatter is now lower level, because it now receives a command object and a group object. Furthermore, they are
now also subject to dependency injection, receiving services and command context via DI.

Default help module is also transient, allowing it to take advantage of more advanced DI usages.

If you need to implement a custom help formatter, see [Custom Help Formatter][6].

### Custom command handlers
You can now disabe the built-in command handler, and create your own. For more information, see
[Custom Command Handlers][7].

### Minor changes
- **Case-insensitivity changes** - case insensitivity now applies to command name matching, prefix matching, and
  argument conversions.
- **DM help** - Default help can now be routed to DMs.
- **Custom attributes on commands** - CommandsNext now exposes all custom attributes declared on commands, groups, and
  modules.
- **Implicit naming** - Commands can be named from their method or class name, by not giving it a name in the Command or
  Group attribute.
- **Argument converters are now asynchronous** - this allows using async code in converters.

## Interactivity
Interactivity went through an extensive rewrite and many methods were changed:

Method                        | Change
:-----------------------------|:-------
`CollectReactionsAsync`       | Different return value
`CreatePollAsync`             | Changed to `DoPollAsync`.
`SendPaginatedMessage`        | Changed to `SendPaginatedMessageAsync`.
`GeneratePagesInEmbeds`       | New parameter.
`GeneratePagesInStrings`      | New parameter.
`GeneratePaginationReactions` | Removed.
`DoPagination`                | Removed.
`WaitForMessageReactionAsync` | Changed to `WaitForReactionAsync`.
`WaitForTypingUserAsync`      | Changed to `WaitForUserTypingAsync`.
`WaitForTypingChannelAsync`   | Changed to `WaitForTypingAsync`.

## VoiceNext
VoiceNext went through a substantial rewrite. Below are some of the key highlights:
- Implemented support for Voice Gateway v4
- Implemented support for lite and suffix encryption mode
- Improved performance
- Replaced old voice sending API with new stream-based transmit API that is non-blocking and has built-in support for
  Changing volume levels.
- Automatic sending of silence packets on connection to enable incoming voice
- Incoming voice now properly maintains an Opus decoder per source
- Packet loss is now concealed via Opus FEC (if possible) or silence packets
- VoiceNext will now properly send and process UDP keepalive packets
- UDP and WebSocket ping values are now exposed on VoiceNextConnection objects
- Voice OP12 and 13 (user join and leave respectively) are now supported and exposed on VoiceNextConnection objects.

## Lavalink
The library now comes with a Lavalink client, which supports both Lavalink 2.x and 3.x.

Lavalink is a standalone lightweight Java application, which handles downloading, transcoding, and transmitting audio to
Discord. For more information, see the [Lavalink][8] article.

<!-- LINKS -->
[0]:  xref:articles.beyond_basics.intents
[1]:  xref:articles.beyond_basics.events
[2]:  xref:articles.beyond_basics.messagebuilder
[3]:  xref:articles.beyond_basics.logging.default
[4]:  xref:articles.commands.dependency_injection
[5]:  xref:articles.commands.dependency_injection#modules
[6]:  xref:articles.commands.help_formatter
[7]:  xref:articles.commands.command_handler
[8]:  xref:articles.audio.lavalink.setup
