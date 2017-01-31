DiscordMember
=============
Represents a guilds member

## Members

`DiscordUser User`: User object

`string Nickname`: Nickname for this member

`List<ulong> Roles`: List of role object ids

`DateTime JoinedAt`: DateTime this member joined at

`bool IsDeafened`: Whether the user is deafened

`bool IsMuted`: Whether the user is muted

## Methods
#### SendDM
Creates a DM channel to this member.

Returns: `DiscordDMChannel`
