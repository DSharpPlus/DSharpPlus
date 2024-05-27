---
uid: articles.migration.dsharp
title: Migration From DiscordSharp
---

## Migration From DiscordSharp

### Connecting

```cs
// Old.
var discord = new DiscordClient("My First Token", true);

discord.SendLoginRequest();
discord.Connect();
```

The constructor of the `DiscordClient` now requires a `DiscordConfiguration` object instead of a
simple string token and boolean.

```cs
// New.
var discord = new DiscordClient(new DiscordConfiguration
{
    Token = "your token",
    TokenType = TokenType.Bot
});

await discord.ConnectAsync();
await Task.Delay(-1);
```

New versions of DSharpPlus implement [TAP][0], and the all DSharpPlus methods ending with *async* will need to be
`await`ed within an asynchronous method.

### Events

While the signature will look similar, many changes have been done to events behind the scenes.

```cs
discord.MessageReceived += async (sender, arg) =>
{
    // Code here
};
```


#### New events

* ChannelPinsUpdated
* ClientErrored
* GuildEmojisUpdated
* GuildIntegrationsUpdated
* GuildMembersChunked
* GuildRoleCreated
* GuildUnavailable
* Heartbeated
* MessageAcknowledged
* MessageReactionAdded
* MessageReactionRemoved
* MessageReactionsCleared
* MessagesBulkDeleted
* SocketErrored
* UnknownEvent
* UserSettingsUpdated
* VoiceServerUpdated
* WebhooksUpdated

#### Removed Events

* TextClientDebugMessageReceived
* VoiceClientDebugMessageReceived

#### Changed Event names

Old DiscordSharp Event | DSharpPlus Equivalent
:----------------------|:----------------------
MessageReceived        | MessageCreated
Connected              | Ready
PrivateChannelCreated  | DmChannelCreated
PrivateMessageReceived | MessageCreated
MentionReceived        | MessageCreated
UserTypingStart        | TypingStarted
MessageEdited          | MessageUpdated
URLMessageAutoUpdate   | MessageUpdate
VoiceStateUpdate       | VoiceStateUpdated
UserUpdate             | UserUpdated
UserAddedToServer      | GuildMemberAdded
UserRemovedFromServer  | GuildMemberRemoved
RoleDeleted            | GuildRoleDeleted
RoleUpdated            | GuildRoleUpdated
GuildMemberBanned      | GuildBanAdded
PrivateChannelDeleted  | DMChannelDeleted
BanRemoved             | GuildBanRemoved
PrivateMessageDeleted  | MessageDeleted.

<!-- LINKS -->
[0]:  https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap
