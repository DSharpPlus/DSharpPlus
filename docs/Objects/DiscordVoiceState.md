DiscordVoiceState
=================
Represents a voice state

## Members
`ulong? GuildID`: The ID for this voice state's Guild

`ulong ChannelID`: The ID for this voice state's Channel

`ulong UserID`: The user id this voice state is for

`string SessionID`: The session id for this voice state

`bool Deaf`: Whether this user is deafened by the server

`bool Mute`: Whether this user is muted by the server

`bool SelfDeaf`: Whether this user is locally deafened

`bool SelfMute`: Whether this user is locally muted

`bool Suppress`: Whether this user is muted by the current user
