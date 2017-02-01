DiscordClient
=============
Basically the client that does everything you need.

## Creating a DiscordClient

It's easy to create a new DiscordClient. the only constructor argument it takes is either a DiscordConfig or nothing.

```cs
// Example
DiscordClient client = new DiscordClient(new DiscordConfig(){
  Token = "hello-mom.im.on.the.internet",
  AutoReconnect = true
  });
```
DiscordConfig has the following members:

`Branch DiscordBranch`: API branch to use. Either `Stable`, `PTB` or `Canary`

`string Token`: Your API token. **No** prefixes please.

`TokenType TokenType`: Token type you're using. Either `User`, `Bot` or `Bearer`

`LogLevel LogLevel`: Log level you want to use. Either `Unnecessary`, `Debug`, `Info`, `Warning`, `Error` or `Critical`.

`bool UseInternalLogHandler`: Whether you want to use the internal log handler. *(tip: set loglevel to unneccessary)*

`int LargeTreshold`: Total number of members where the gateway will stop sending offline members in the guild member list

`bool AutoReconnect`: Whether you want DSharpPlus to automatically reconnect

`VoiceSettings VoiceSettings`: What you intend to do with voice. Either `Sending`, `Receiving`, `Both` or `None`.

`VoiceApplication VoiceApplication`: What kind of media you intend to play. Either `Voice`, `Music` or `LowLatency.`

## Events
All events are located under `Programming Reference/Events`.
```cs
// Example
client.MessageReceived += (sender, e) =>
{
  // Code to execute on MessageReceived. 'e' are the event args.
};
```

## Connecting

To connect to Discord, use `DiscordClient.Connect()`. There's two ways to connect.

The first way: `Connect(int shard = 0)` (If DiscordConfig is set)

The second way: `Connect(string tokenOverride, TokenType tokenType, int shard = 0)` (if DiscordConfig isn't set)

(If you use multiple DiscordClients for sharding, enter a shard number)

## Members

`DebugLogger DebugLogger`: The internal debuglogger. Use this to log things.

`string GatewayURL`: Gateway URL

`string GatewayVersion`: Gateway Version

`DiscordVoiceClient VoiceClient`: Used for voice

`int Shards`: Number of shards connected with

`DiscordUser Me`: Current user connected with

`List<DiscordDMChannel> PrivateChannels`: List of DM Channels

`Dictionary<ulong, DiscordGuild> Guilds`: List of Guilds

## Methods

#### Connect
Connects to Discord's Gateway

Returns: Nothing

#### Connect
Connects to Discord's Gateway

`string tokenOverride`: Token (if not set)

`TokenType tokenType`: Type of token used

`int shard = 0`: shard to connect to

Returns: Nothing

#### GetUser
Gets a user

`string user`: User ID or @me

Returns: `DiscordUser`

#### DeleteChannel
Deletes a channel

`ulong id`: ID of channel to delete

Returns: Nothing

#### DeleteChannel
Deletes a channel

`DiscordChannel channel`: Channel to delete

Returns: Nothing

#### GetMessage
Gets a message

`DiscordChannel channel`: Channel message was sent in.

`ulong MessageID`: ID of message

Returns: `DiscordMessage`

#### GetMessage
Gets a message

`ulong ChannelID`: Channel ID message was sent in.

`ulong MessageID`: ID of message

Returns: `DiscordMessage`

#### GetChannel
Gets a channel

`ulong ID`: ID of channel

Returns: `DiscordChannel`

#### SendMessage
Sends a message

`DiscordChannel Channel`: Channel to send message to

`string content`: Contents of message

`bool tts = false`: Whether this message is TTS

Returns: `DiscordMessage`

#### SendMessage
Sends a message

`DiscordDMChannel Channel`: Channel to send message to

`string content`: Contents of message

`bool tts = false`: Whether this message is TTS

Returns: `DiscordMessage`

#### CreateGuild
Creates a guild. Only for whitelisted bots and users.

`string name`: Name of guild to create

Returns: `DiscordGuild`

#### GetGuild
Gets a guild.

`ulong id`: ID of guild

Returns: `DiscordGuild`

#### DeleteGuild
Deletes a guild.

`ulong id`: ID of guild to delete

Returns: `DiscordGuild`

#### DeleteGuild
Deletes a guild.

`DiscordGuild Guild`: Guild to delete

Returns: `DiscordGuild`

#### GetChannelByID
Gets a channel. Wait where have I heard this before? :^)

`ulong ID`: ID of channel to get

Returns: `DiscordChannel`

#### GetInviteByCode
Gets an invite by its code.

`string code`: Invite code

Returns: `DiscordInvite`

#### GetConnections
Gets current user's connections

Returns: `List<DiscordConnection>`

#### ListRegions
Lists regions

Returns: `List<DiscordVoiceRegion>`

#### GetWebhook
Gets a webhook by its ID

`ulong ID`: Webhook ID

Returns: `DiscordWebhook`

#### GetWebhookWithToken
Gets a webhook by its ID and token

`ulong ID`: Webhook ID

`string token`: Webhook token

Returns: `DiscordWebhook`

#### CreateDM
Creates a DM Channel

`ulong UserID`: ID of user to chat with

Returns: `DiscordDMChannel`

#### UpdateStatus
Updates "Playing:" status.

`string game`: Text you want Discord to display

`int idle_since = -1`: Since when you've been idle

#### ModifyMember
Modifies a guild member

`ulong GuildID`: ID of Guild

`ulong MemberID`: ID of member to modify

`string Nickname`: New nickname for this member

`List<DiscordRole> Roles`: New roles for this member

`bool Muted`: Whether this member has to be muted

`bool Deaf`: Whether this member has to be deafened

`ulong VoiceChannelID`: New voice channel to move this member to

Returns: Nothing

#### GetCurrentApp
Gets current API Application.

Returns: `DiscordApplication`

#### GetUserPresence
Gets current presence for a user

`ulong UserID`: ID of user.

Returns: `DiscordPresence`

#### Dispose
Kills your DiscordClient. Only use if you **don't** plan to do anything else.

Returns: Nothing.
