# Halp! I left my cave and the red light hurts my eyes!

DSharpPlus is a continuation of DiscordSharp made by Luigifan / Suicvne. The two libraries are quite similar, however a 
lot has changed since we've continued the lib. Here's a migration guide for those who still need it.

## Connecting

Due to some changes in Discord's API and in the library itself, connecting to Discord goes a bit differently. Instead 
of constructing a DiscordClient object with just a token and a boolean, we now have to provide a whole 
DiscordConfiguration object.

Before this change your code would've looked like this:

```cs
DiscordClient client = new DiscordClient("your token", true); // true is for isbot.
// these two lines were used for logging in with email and password,
// though Discord does not allow this anymore. Doing so will now flag your account.
client.ClientPrivateInformation.Email = "email";
client.ClientPrivateInformation.Password = "pass";
// You'll rather want to connect with a token.
client.SendLoginRequest();
client.Connect();
```

Now, your code should look more like this:

```cs
DiscordClient client = new DiscordClient(new DiscordConfiguration
{
    Token = "your token",
    TokenType = TokenType.Bot
});

// All code is now async. You'll want to move your code to an async method.
await client.ConnectAsync();
await Task.Delay(-1);
// We add a little delay because your code will no longer run on the main thread.
// Not doing this will exit the program and shut down your bot.
```

## Events

Events are now quite different. All events are now an `AsyncEvent`. These run asynchronously, as the name implies.

Your old events would've looked like this:

```cs
client.MessageReceived += (sender, e) =>
{
  // Code here
};
```

Now they should look like this:

```cs
client.MessageCreated += async e =>
{
  // Code here
}
```

Complete event guide is available in the [event reference](/articles/getting_started/event_reference.html "Events") article.

Event names were also changed. A list of changed event names is to be found at the bottom of this article, along with 
some new events not yet available in DiscordSharp.

## Changed Event names

* MessageReceived -> MessageCreated
* Connected -> Ready
* SocketOpened -> SocketOpened
* SocketClosed -> SocketClosed
* ChannelCreated -> ChannelCreated
* PrivateChannelCreated -> DmChannelCreated
* PrivateMessageReceived -> Not in DSharpPlus, use MessageCreated
* MentionReceived -> Not in DSharpPlus, Use MessageCreated.
* UserTypingStart -> TypingStarted
* MessageEdited -> MessageUpdated
* PresenceUpdated -> PresenceUpdated
* URLMessageAutoUpdate -> this is dispatched via regular MessageUpdate event
* VoiceStateUpdate -> VoiceStateUpdated
* UnknownMessageTypeReceived -> Not in DSharpPlus, Use MessageCreated.
* MessageDeleted -> MessageDeleted
* UserUpdate -> UserUpdated
* UserAddedToServer -> GuildMemberAdded
* UserRemovedFromServer -> GuildMemberRemoved
* GuildCreated -> GuildCreated
* GuildAvailable -> GuildAvailable
* GuildDeleted -> GuildDeleted
* ChannelUpdated -> ChannelUpdated
* TextClientDebugMessageReceived
* VoiceClientDebugMessageReceived
* ChannelDeleted -> ChannelDeleted
* GuildUpdated -> GuildUpdated
* RoleDeleted -> GuildRoleDeleted
* RoleUpdated -> GuildRoleUpdated
* GuildMemberUpdated -> GuildMemberUpdated
* GuildMemberBanned -> GuildBanAdded
* PrivateChannelDeleted -> DMChannelDeleted
* BanRemoved -> GuildBanRemoved
* PrivateMessageDeleted -> Not in DSharpPlus, use MessageDeleted.

### New events

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