Reference for ``DiscordGuild``
================================

``DiscordGuild`` represents a single user guild, which holds users, roles, channels, and messages. Most of the time, 
your interaction with users is going to happen through guilds.

Members
---------

.. attribute:: Name

	This guild's name.

.. attribute:: Icon

	ID or name of the guild's icon.

.. attribute:: IconUrl

	URL of the guild's icon.

.. attribute:: Splash

	Guild's invite splash. This is available for partnered and special guilds only.

.. attribute:: OwnerID

	ID of the guild's owner

.. attribute:: RegionID

	Guild's voice region ID.

.. attribute:: AFKChannelID

	ID of the voice AFK channel.

.. attribute:: AFKTimeout

	Voice AFK timeout in seconds.

.. attribute:: EmbedEnabled

	Whether the widget is enabled for this guild.

.. attribute:: EmbedChannelID

	ID of the channel the widget leads to.

.. attribute:: VerificationLevel

	Guild's verification level.

.. attribute:: DefaultMessageNotification

	Guild's default notification level.

.. attribute:: Roles

	List of :doc:`DiscordRoles </reference/DiscordRole>` defined in this guild.

.. attribute:: Emojis

	List of :doc:`DiscordEmojis </reference/DiscordEmoji>` defined in this guild.

.. attribute:: Features

	List of the guild's features, such as VIP, etc.

.. attribute:: MFALevel

	Guild's multi-factor authentication level.

.. attribute:: JoinedAt

	Date the guild was joined.

.. attribute:: Large

	Whether this guild is considered a large guild.

.. attribute:: Unavailable

	Whether this guild is unavailable.

.. attribute:: MemberCount

	Guild's member count. This value is independent of cached member count.

.. attribute:: VoiceStates

	List of :doc:`VoiceStates </reference/voice/VoiceState>` for this guild's members.

.. attribute:: Members

	.. note::
	
		This property contains cached members only. If you're looking for total member count, use the ``MemberCount`` property instead.

	List of cached :doc:`VoiceStates </reference/DiscordMember>` for this guild. 

.. attribute:: Channels

	List of :doc:`DiscordChannels </reference/DiscordChannel>` defined in this guild.

.. attribute:: Presences

	List of :doc:`DiscordPresence </reference/misc/DiscordPresence>` for this guild's members.

.. attribute:: IsOwner

	Whether the current user is the owner of this guild.

Methods
---------

.. function:: Delete()

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Deletes this guild.

.. function:: Modify(name, region, verification_level, default_message_notifications, afk_channel_id, afk_timeout, owner_id, splash)

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Modifies this guild's properties.
	
	:param name: Changes the guild's name. Optional, defaults to empty string.
	:param region: Changes the guild's voice region. Optional, defaults to empty string.
	:param verification_level: Changes the guild's verification level. Optional, defaults to ``-1``.
	:param default_message_notifications: Changes default notification settings. Optional, defaults to ``-1``.
	:param owner_id: Changes guild's owner. Optional, defaults to 0.
	:param splash: Changes guild's invite splash. This is available for partnered guilds only. Optional, defaults to empty string.

.. function:: BanMember(member)

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Bans a member from this guild.
	
	:param Member: An instance of :doc:`GuildMember </reference/GuildMember>` to ban.

.. function:: UnbanMember(member)

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Unbans a member from this guild.
	
	:param Member: An instance of :doc:`GuildMember </reference/GuildMember>` to unban.

.. function:: Leave()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Leaves this guild.

.. function:: GetBans()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Gets all bans for this guild. Returns a list of :doc:`GuildMembers </reference/GuildMember>`.

.. function:: CreateChannel(name, type, bitrate, userlimit)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Creates and returns a new channel.
	
	:param name: Name of the new channel.
	:param type: Type of the new channel.
	:param bitrate: Bitrate for the channel. This is only applicable for voice channels. Optional, defaults to ``0``.
	:param userlimit: User limit for the channel. This is only applicable for voice channels. Optional, defaults to ``0``.

.. function:: GetPruneCount(days)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Estimates and returns the number of users that would be pruned.
	
	:param days: Number of days the users have to be inactive to be pruned.

.. function:: Prune(days)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Prunes inactive users. Returns the number of users pruned.
	
	:param days: Number of days the users have to be inactive to be pruned.

.. function:: GetIntegrations()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Gets this guild's integrations.

.. function:: AttachUserIntegration(integration)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Attaches an integration to the guild.
	
	:param integration: Integration (instance of :doc:`DiscordIntegration </reference/misc/DiscordIntegration>`) to attach.

.. function:: ModifyIntegration(integration, expire_behaviour, expire_grace_period, enable_emoticons)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Modifies an integration for this guild.
	
	:param integration: Integration (instance of :doc:`DiscordIntegration </reference/misc/DiscordIntegration>`) to modify.
	:param expire_behaviour: Integration subscription lapse behaviour.
	:param expire_grace_period: Period (in seconds) during which the integration will ignore lapsed subscription.
	:param enable_emoticons: Whether emoticons should be synced to this guild.

.. function:: DeleteIntegration(integration)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Deletes an integration from this guild.
	
	:param integration: Integration (instance of :doc:`DiscordIntegration </reference/misc/DiscordIntegration>`) to remove.

.. function:: SyncIntegration(integration)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Syncs an integration to this guild.

	:param integration: Integration (instance of :doc:`DiscordIntegration </reference/misc/DiscordIntegration>`) to remove.

.. function:: GetEmbed()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Gets this guild's widget.

.. function:: GetVoiceRegions()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Gets the voice regions for this guild. Returns a list of :doc:`DiscordVoiceRegions </reference/misc/DiscordVoiceRegions>`.

.. function:: GetInvites()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Gets invitations for this guild. Returns a list of :doc:`DiscordInvites </reference/misc/DiscordInvite>`.

.. function:: GetWebhooks()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Gets webhooks for this guild. Returns a list of :doc:`DiscordWebhooks </reference/misc/DiscordWebhook>`.

.. function:: RemoveMember(member)
.. function:: RemoveMember(member_id)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Kicks a member from this guild.
	
	:param member: Instance of :doc:`DiscordUser </reference/DiscordUser>` to kick.
	:param member_id: Id of the member to kick.

.. function:: GetMember(member_id)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Gets a member of this guild by their ID.
	
	:param member_id: ID of the member to get.

.. function:: GetAllMembers()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	.. note::
	
		This method can take a while to execute.
	
	Dowloads all members of this guild. Returns a list of :doc:`DiscordMembers </reference/DiscordMember>`.

.. function:: ModifyMember(member_id, nickname, roles, muted, deaf, voice_channel_id)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Modifies a guild member.

	:param member_id: ID of the member to modify.
	:param nickname: Nickname to give the member.
	:param roles: Roles to replace the user's roles with.
	:param muted: Whether the user should be muted in audio.
	:param deaf: Whether the user should be deafened in audio.
	:param voice_channel_id: Which voice channel to move the user to.

.. function:: GetChannels()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Gets all channels in this guild. Returns a list of :doc:`DiscordChannels </reference/DiscordChannel>`.

.. function:: ListMembers(limit, after)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Gets paginated list of users. Returns a list of :doc:`DiscordMembers </reference/DiscordMember>`.
	
	:param limit: Max number of members to get. Cannot exceed 100.
	:param after: Last member from previous page.

.. function:: UpdateRole(role)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Modifies a role.
	
	:param role: Role to modify.

.. function:: CreateRole()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Creates a new role.

.. function:: AddRole(user_id, role_id)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Adds a single role to specified user.
	
	:param user_id: ID of the user whom to add the role to.
	:param role_id: ID of the role to add to the user.

.. function:: RemoveRole(user_id, role_id)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Removes a single role from specified user.
	
	:param user_id: ID of the user whom to remove the role from.
	:param role_id: ID of the role to remove from the user.