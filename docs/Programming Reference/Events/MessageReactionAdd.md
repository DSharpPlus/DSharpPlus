MessageReactionAdd
==================
Gets sent when a message reaction gets added

## EventArgs:
`ulong UserID`: ID of user that added this reaction

`ulong MessageID`: ID of message reaction was added to

`ulong ChannelID`: Channel this message belongs to

`DiscordEmoji Emoji`: Emoji used to react

`DiscordUser User`: User object for UserID

`DiscordMessage Message`: Message the reaction was added to

`DiscordChannel Channel`: Channel the message belongs to
