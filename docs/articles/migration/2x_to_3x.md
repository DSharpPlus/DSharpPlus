---
uid: articles.migration.2x_to_3x
title: Migration 2.x - 3.x
---

# Migration From 2.x to 3.x

Major breaking changes:

* Most classes were organized into namespaces.
* Several classes and methods were renamed to maintain consistency with the rest of the library.
* All events were renamed to be in the past tense.
* @DSharpPlus.Entities.DiscordEmbed instances are no longer constructed directly.
  * Instead, they are built using a @DSharpPlus.Entities.DiscordEmbedBuilder.
* All colors are now passed as instances of @DSharpPlus.Entities.DiscordColor.
* Command modules are now based on an abstract class rather than an interface.
* A brand-new ratelimit handler has been implemented.

## Fixing namespace issues

Entities such as @DSharpPlus.Entities.DiscordUser, @DSharpPlus.Entities.DiscordChannel, and similar are in the
@DSharpPlus.Entities namespace, exceptions in @DSharpPlus.Exceptions, event arguments in @DSharpPlus.EventArgs, and
network components in @DSharpPlus.Net.

Be sure to add these namespaces to your `using` directives as needed.

## Class, Method, and Event Renames

Several classes and methods were renamed to fit the current naming scheme in the library.

2.x                                 | 3.x
:----------------------------------:|:-----------------------------------:
`DiscordConfig`                     | `DiscordConfiguration`
`CommandExecutedEventArgs`          | `CommandExecutionEventArgs`
`SnowflakeObject.CreationDate`      | `SnowflakeObject.CreationTimestamp`
`VoiceReceivedEventArgs`            | `VoiceReceiveEventArgs`
`DiscordMessage.EditAsync()`        | `DiscordMessage.ModifyAsync()`
`SocketDisconnectEventArgs`         | `SocketCloseEventArgs`
`DiscordMember.TakeRoleAsync()`     | `DiscordMember.RevokeRoleAsync()`
`MessageReactionRemoveAllEventArgs` | `MessageReactionsClearEventArgs`

Additionally, all events received a rename to maintain consistent naming across the library with many receiving an *d*
or *ed* to the end of their name.

## Embed woes

Embeds can no longer be constructed or modified directly. Instead, you have to use the embed builder. For the most part,
this can be achieved using Find/Replace and doing `new DiscordEmbed` -> `new DiscordEmbedBuilder`.

On top of that, to add fields to an embed, you no longer create a new list for fields and assign it to
@DSharpPlus.Entities.DiscordEmbedBuilder.Fields, but instead you use the
@DSharpPlus.Entities.DiscordEmbedBuilder.AddField* method on the builder.

To modify an existing embed, pass said embed to builder's constructor. The builder will use it as a prototype.

## Color changes

This one is easy to fix for the most part. For situation where you were doing e.g. `Color = 0xC0FFEE`, you now do
`Color = new DiscordColor(0xC0FFEE)`. This has the added advantage of letting you create a color from 3 RGB values or
parse an RGB string.

## Default channel removal

`DefaultChannel` no longer exists on guilds, and, as such, `DiscordGuild.CreateInviteAsync()` is also gone, as it relied
on that property.

The new concept of "default" channel is a fallback, and is basically top channel the user can see. In the library this
is facilitated via `DiscordGuild.GetDefaultChannel()`.

## Module changes

The `IModule` interface was removed, and replaced with `BaseModule` class. The most notable change is that your module
should no longer define the field which holds your instance of @DSharpPlus.DiscordClient, as that's on the base class
itself. On top of that, you need to change modifiers of `.Setup()` from `public` to `protected internal override`.

## New ratelimit handler

This does not actually cause any in-code changes, however the behavior of the REST client and the way requests are
handled changes drastically.

The new handler is thread-safe, and uses queueing to handle REST requests, and should bucket requests properly now.
