MessageReactionRemove
==================
Gets sent when a message reaction gets removed

## EventArgs:
`ulong UserID`: ID of user that removed this reaction

`ulong MessageID`: ID of message reaction was removed from

`ulong ChannelID`: Channel this message belongs to

`DiscordEmoji Emoji`: Emoji used to react

`DiscordUser User`: User object for UserID

`DiscordMessage Message`: Message the reaction was removed from

`DiscordChannel Channel`: Channel the message belongs to
