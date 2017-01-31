DiscordInvite
=============
Represents an invite to a guild

## Members

`string Code`: The invite code (unique ID)

`DiscordInviteGuild Guild`: The guild this invite is for

`DiscordInviteChannel Channel`: The channel this invite is for

## Methods
#### Delete
Deletes an invite

Returns: `DiscordInvite`

#### Accept
Accepts an invite (Requires "guilds.join" scope or user token)

Returns: `DiscordInvite`
