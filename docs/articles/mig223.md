# I updated the library and now I'm drowning in red underline!

3.0 was a major breaking change compared to 2.0. A lot has changed internally, and even more externally.

Among the breaking changes there are:

 * Most classes were organized into namespaces.
 * Several classes and methods were renamed to maintain consistency with the rest of the library.
 * All events were renamed to be in the past tense.
 * @DSharpPlus.Entities.DiscordEmbed instances are no longer constructed directly; instead, they are built via the brand-new 
   @DSharpPlus.Entities.DiscordEmbedBuilder.
 * All colors are now passed as instances of @DSharpPlus.Entities.DiscordColor.
 * Concept of default channel has been removed as it no longer exists; this means that you can no longer create invites to 
   guilds directly.
 * Modules are now based on an abstract class rather than an interface.
 * A brand-new ratelimit handler was implemented.

The list of changes goes on, but the above are what affects the consumers of the library. Some of them require major changes to 
your code.

## Fixing namespace issues.

One of the fastest fixes is adding missing `using` instructions to your code. For entities such as `DiscordUser`, 
`DiscordChannel`, etc. this requires adding `DSharpPlus.Entities` namespace. Exceptions lie in `DSharpPlus.Exceptions`, event 
arg classes can be found in `DSharpPlus.EventArgs`, and network components are in `DSharpPlus.Net`.

## Major renames

Several classes and methods were renamed to fit the current naming scheme in the library. Most notable ones are:

 * `DiscordConfig` -> `DiscordConfiguration`
 * `CommandExecutedEventArgs` -> `CommandExecutionEventArgs`
 * `SnowflakeObject.CreationDate` -> `SnowflakeObject.CreationTimestamp`
 * `VoiceReceivedEventArgs` -> `VoiceReceiveEventArgs`
 * `DiscordMessage.EditAsync()` -> `DiscordMessage.ModifyAsync()`
 * `SocketDisconnectEventArgs` -> `SocketCloseEventArgs`
 * `DiscordMember.TakeRoleAsync()` -> `DiscordMember.RevokeRoleAsync()`
 * `MessageReactionRemoveAllEventArgs` -> `MessageReactionsClearEventArgs`
 
## Event renames

All events received a rename to maintain consistent naming across the library. If your event shows up as not found, try adding 
`d` or `ed` to the end of its name.

## Embed woes

Embeds can no longer be constructed or modified directly. Instead, you have to use the embed builder. For the most part, this 
can be achieved using Find/Replace and doing `new DiscordEmbed` -> `new DiscordEmbedBuilder`.

On top of that, to add fields to an embed, you no longer create a new list for fields and assign it to `Fields`, but instead 
you use the `.AddField()` method on the builder.

To modify an existing embed, pass said embed to builder's constructor. The builder will use it as a prototype.

## Color changes

This one is easy to fix for the most part. For situation where you were doing e.g. `Color = 0xC0FFEE`, you now do 
`Color = new DiscordColor(0xC0FFEE)`. This has the added advantage of letting you create a color from 3 RGB values or parse 
an RGB string.

## Default channel removal

`DefaultChannel` no longer exists on guilds, and, as such, `DiscordGuild.CreateInviteAsync()` is also gone, as it relied on 
that property.

The new concept of "default" channel is a fallback, and is basically top channel the user can see. In the library this is 
facilitated via `DiscordGuild.GetDefaultChannel()`.

## Module changes

The `IModule` interface was removed, and replaced with `BaseModule` class. The most notable change is that your module should 
no longer define the field which holds your instance of `DiscordClient`, as that's on the base class itself. On top of that, 
you need to change modifiers of `.Setup()` from `public` to `protected internal override`.

## New ratelimit handler

This does not actually cause any in-code changes, however the behavior of the REST client and the way requests are handled 
changes drastically.

The new handler is thread-safe, and uses queueing to handle REST requests, and should bucket requests properly now.
