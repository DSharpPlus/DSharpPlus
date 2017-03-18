DiscordGuild
============

Represents a Guild

Members
-------

``ulong ID``: ID for this object

``DateTime CreationDate``: When this was created

``string Name``: Guild Name

``string Icon``: Icon Hash

``string IconUrl``: Icon Url

``string Splash``: Splash hash

``ulong OwnerID``: Owner’s ID

``string RegionID``: Region ID

``ulong AFKChannelID``: AFK Voice channel ID

``int AFKTimeout``: AFK Timeout in seconds

``bool EmbedEnabled``: Is this guild embeddable?

``ulong EmbedChannelID``: Embed Channel ID

``int VerificationLevel``: Verification level

``int DefaultMessageNotifications``: Default message notification level

``List<DiscordRole> Roles``: This guild’s Roles

``List<DiscordEmoji> Emojis``: This guild’s Emojis

``List<string> Features``: This guild’s Features

``int MFALevel``: This guild’s MFA level

``DateTime JoinedAt``: Your join date

``bool Large``: Wether this guild is considered Large

``bool Unavailable``: Wether this guild is Unavailable

``int MemberCount``: Amount of Members

``List<DiscordVoiceState> VoiceStates``: List of Voice States

``List<DiscordMember> Members``: List of Members

``List<DiscordChannel> Channels``: List of Channels

``List<DiscordPresence> Presences``: List of Presences

``bool IsOwner``: Wether you own this Guild

Methods
-------

Delete
^^^^^^

Deletes this Guild

Returns: ``DiscordGuild``

Modify
^^^^^^

Modifies this Guild

``string name = ""``: Guild Name

``string region = ""``: Guild Region

``int verification_level = -1``: Guild Verification level

``int default_message_notifications = -1``: Default message notification
level

``ulong afkchannelid = 0``: AFK Channel ID

``int afktimeout = -1``: AFK Timeout

``ulong ownerID = 0``: New owner ID

``string splash = ""``: New splash image

Returns: ``DiscordGuild``

BanMember
^^^^^^^^^

Bans a member

``DiscordMember member``: Member to ban

Returns: Nothing

UnbanMember
^^^^^^^^^^^

Unbans a member

``DiscordMember member``: Member to unban

Returns: Nothing

Leave
^^^^^

Leaves this guild

Returns: Nothing

GetBans
^^^^^^^

Gets Bans for this guild

Returns: ``List<DiscordBan>``

CreateChannel
^^^^^^^^^^^^^

Creates a channel

``string name``: Channel name

``ChannelType type``: Channel type

``int bitrate = 0``: Bitrate (if voice)

``int userlimit = 0``: User limit (if voice)

Returns: ``DiscordChannel``

GetPrunecount
^^^^^^^^^^^^^

Gets amount of members that’ll be kicked when pruning this Guild

``int days``: Amount of inactive days to allow

Returns: ``int``

Prune
^^^^^

Prune Members

``int days``: Amount of inactive days to allow

Returns: ``int``

GetIntegrations
^^^^^^^^^^^^^^^

Gets guild integrations

Returns: ``List<DiscordIntegration>``

AttachUserIntegration
^^^^^^^^^^^^^^^^^^^^^

Attaches a user’s integration to this Guild

``DiscordIntegration integration``: Integration to attach

Returns: ``DiscordIntegration``

ModifyIntegration
^^^^^^^^^^^^^^^^^

Modifies a guild integration

``DiscordIntegration integration``: Integration to edit

``int expire_behaviour``: Behaviour when this integration expires

``int expire_grace_period``: Grace period

``bool enable_emoticons``: Wether to enable emotes for this integration

Returns: ``DiscordIntegration``

DeleteIntegration
^^^^^^^^^^^^^^^^^

Deletes an integration

``DiscordIntegration integration``: Integration to Delete

Returns: Nothing

SyncIntegration
^^^^^^^^^^^^^^^

Syncs an integration

``DiscordIntegration integration``: Integration to sync

Returns: Nothing

GetEmbed
^^^^^^^^

Gets guild embed

Returns: ``DiscordGuildEmbed``

GetVoiceRegions
^^^^^^^^^^^^^^^

Gets voice regions for this guild

Returns: ``List<DiscordVoiceRegion>``

GetInvites
^^^^^^^^^^

Gets invites for this guild

Returns: ``List<DiscordInvite>``

GetWebhooks
^^^^^^^^^^^

Gets webhooks for this guild

Returns: ``List<DiscordWebhook>``

RemoveMember
^^^^^^^^^^^^

Removes a member

``DiscordUser user``: Member to remove

Returns: Nothing

RemoveMember
^^^^^^^^^^^^

Removes a member

``ulong UserID``: ID of member to remove

Returns: Nothing

GetMember
^^^^^^^^^

Gets a guild member

``ulong UserID``: Member’s ID

Returns: ``DiscordMember``

GetAllMembers
^^^^^^^^^^^^^

Gets all guild Members

Returns: ``List<DiscordMember>``

ModifyMember
^^^^^^^^^^^^

Modifies a guild member

``ulong MemberID``: ID of member to edit

``string Nickname``: New Nickname

``List<DiscordRole> Roles``: New Roles

``bool Muted``: Wether this member has been muted in voice

``bool Deaf``: Wether this member has been deafened in voice

``ulong VoiceChannelID``: Voice channel to place this member in

Returns: Nothing

GetChannels
^^^^^^^^^^^

Gets guild’s Channels

Returns: ``List<DiscordChannel>``

ListMembers
^^^^^^^^^^^

Gets guild’s Members

``int limit``: Limit of members to return

``int after``: Index to begin from

Returns: ``List<DiscordMember>``

UpdateRole
^^^^^^^^^^

Updates role

``DiscordRole role``: Updated role object

Returns: Nothing

CreateRole
^^^^^^^^^^

Creates a new DiscordRole (modify this and run UpdateRole)

Returns: DiscordRole
