PresenceUpdate
==============
Gets sent when a presence gets updated.

**Alternatively you could also use DiscordUser.GetPresence()**

## EventArgs:
`ulong UserID`: User that had their presence updated

`string Game`: Game this user is playing

`string Status`: User's status (`Online`, `Offline`, `Idle`, `DND`)

`ulong GuildID`: Guild ID for this presence update

`List<ulong> RoleIDs`: Roles this user has in this guild
