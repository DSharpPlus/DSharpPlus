DiscordChannel
==============
 Represents a guild channel

## Members

`ulong ID`: ID for this object

`DateTime CreationDate`: When this was created


`ulong GuildID`: This channel's Guild's ID

`string Name`: This channel's Name

`ChannelType Type`: This channel's type (Text or Voice)

`int Position`: this channel's position

`bool IsPrivate`: Wether this is a private channel

`DiscordGuild Parent`: This channel's DiscordGuild (if public)

`List<DiscordOverwrite> PermissionOverwrites`: Permissions for this channel

`string Topic`: This channel's topic

`ulong LastMessageID`: ID for this channel's last message (if Text)

`int Bitrate`: This channel's bitrate (if Voice)

`int UserLimit`: This channel's user limit (if Voice)

`string Mention`: Builds a mention for this channel

## Methods

#### SendMessage

Sends a message to this channel

`string content`: Content for this message

`bool tts = false`: Wether this message is a TTS message

`DiscordEmbed embed = null`: Embed to attach

Returns: `DiscordMessage`

#### SendFile

Sends a message to this channel with a file attached

`string filepath`: Path to the file you want to send

`string filename`: Name for this file (with extension)

`string content = ""`: Content for this message

`bool tts = false`: Wether this is a TTS message

Returns: `DiscordMessage`

#### Delete

Deletes this channel (No arguments)

Returns: Nothing

#### GetMessage

Gets a message from this channel

`ulong ID` ID for this message

Returns: `DiscordMessage`

#### ModifyPosition

Modifies this channel's position

`int position`: New position for this channel

Returns: Nothing

#### GetMessages

Gets a list of DiscordMessages (Only set one out of around, before and after!)

`ulong around = 0`: Gets messages around this ID

`ulong before = 0`: Gets messages before this ID

`ulong after = 0`: Gets messages after this ID

`int limit = 50`: Limits the amount of messages

Returns: `List<DiscordMessage>`

#### BulkDeleteMessages

Deletes multiple DiscordMessages

`List<ulong> MessageIDs`: List of messages to delete

Returns: Nothing

#### GetInvites

Gets this channel's DiscordInvites

Returns: `List<DiscordInvite>`

#### CreateInvite

Creates an invite for this channel

`int MaxAge = 86400`: Max age for this invite

`int MaxUses = 0`: Max uses for this invite (0 = unlimited)

`bool temporary = false`: Wether this invite is temporary or not

`bool unique = false`: Wether this invite has to be a new one or a pre-existing one may be returned

Returns: `DiscordInvite`

#### DeleteChannelPermission

Deletes a channel's permission

`ulong OverwriteID`: ID for this permission

Returns: Nothing

#### TriggerTyping

Makes you appear typing

Returns: Nothing

#### GetPinnedMessages

Gets pinned messages for this channel

Returns: `List<DiscordMessage>`

#### CreateWebhook

Creates a webhook for this channel

`string Name = ""`: Name for this webhook

`string base64avatar = ""`: Avatar for this webhook in base64

Returns: `DiscordWebhook`

#### GetWebhooks

Gets this channel's webhooks

Returns: `List<DiscordWebhook>`

#### ConnectToVoice

Connects to this voice channel

Returns: Nothing

#### PlaceMember

Places a member in this voice channel (if member is connected to voice)

`ulong MemberID`: Member's ID

Returns: Nothing
