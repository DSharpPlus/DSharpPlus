Reference for ``DiscordGuild``
==============================

``DiscordGuild`` represents a single user guild, which holds users, roles, channels, and messages. Most of the time, 
your interaction with users is going to happen through guilds.

Members
-------

.. attribute:: Id

	This guild's ID.

.. attribute:: Name

	This guild's name.

.. attribute:: IconHash

	Hash of the guild's icon image.

.. attribute:: IconUrl

	URL of the guild's icon.

.. attribute:: SplashHash

	Hash of the guild's invite splash image. This is only available for partnered guilds.

.. attribute:: SplashUrl

	URL of the guild's invite splash image. This is only available for partnered guilds.

.. attribute:: Owner

	Guild's owner. Instance of :doc:`DiscordMember </reference/DiscordMember>`.

.. attribute:: RegionId

	Guild's voice region ID.

.. attribute:: AFKChannel

	ID of the voice AFK channel.

.. attribute:: AFKTimeout

	Voice AFK timeout in seconds.

.. attribute:: EmbedEnabled

	Whether the widget is enabled for this guild.

.. attribute:: EmbedChannel

	:doc:`DiscordChannel </reference/DiscordChannel>` to which the guild's widget invite leads to.

.. attribute:: VerificationLevel

	Guild's verification level. Instance of :doc:`VerificationLevel </reference/misc/VerificationLevel>` enum.

.. attribute:: DefaultMessageNotification

	Guild's default notification settings. Instance of :doc:`VerificationLevel </reference/misc/DefaultMessageNotifications>` enum.

.. attribute:: Roles

	List of :doc:`DiscordRoles </reference/DiscordRole>` defined in this guild.

.. attribute:: Emojis

	List of :doc:`DiscordEmojis </reference/DiscordEmoji>` defined in this guild.

.. attribute:: Features

	List of the guild's features, such as VIP, etc.

.. attribute:: MfaLevel

	Guild's multi-factor authentication level. Instance of :doc:`MfaLevel </reference/misc/MfaLevel>` enum.

.. attribute:: JoinedAt

	Date the guild was joined.

.. attribute:: IsLarge

	Whether this guild is considered a large guild. This can be adjusted by changing the ``LargeThreshold`` property in :doc:`DiscordConfig </reference/misc/DiscordConfig>`.

.. attribute:: IsUnavailable

	Whether this guild is unavailable.

.. attribute:: MemberCount

	Guild's member count. This value is independent of cached member count.

.. attribute:: VoiceStates

	List of :doc:`VoiceStates </reference/voice/VoiceState>` for this guild's members.

.. attribute:: Members

	.. note::
	
		This property contains cached members only. If you're looking for total member count, use the :attr:`MemberCount` property instead.
		
		To cache all users, use the :func:`GetAllMembersAsync` method.

	List of cached :doc:`DiscordMember </reference/DiscordMember>` for this guild. 

.. attribute:: Channels

	List of :doc:`DiscordChannels </reference/DiscordChannel>` defined in this guild.

.. attribute:: Presences

	List of :doc:`DiscordPresence </reference/misc/DiscordPresence>` for this guild's members.

.. attribute:: IsOwner

	Whether the current user is the owner of this guild.

.. attribute:: DefaultChannel

	The default text channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) for this guild.

.. attribute:: EveryoneRole

	The ``@everyone`` role (instance of :doc:`DiscordRole </reference/DiscordRole>`) for the guild.

Methods
-------

.. function:: DeleteAsync()
	
	Deletes this guild. This can only be done if the current user is the guild's owner, and the bot's owner has 2-factor authentication enabled on their account.

.. function:: ModifyAsync(name, region, icon, icon_format, verification_level, default_message_notifications, afk_channel, afk_timeout, owner, splash, splash_format, reason)
	
	Modifies this guild's properties.
	
	:param name: Changes the guild's name. Optional, defaults to ``null``.
	:param region: Changes the guild's voice region. Optional, defaults to ``null``.
	:param icon: Stream containing icon data for the guild. Must be valid PNG, JPG, or GIF image. Optional, defaults to ``null``. If this is specified, ``icon_format`` must also be specified.
	:param icon_format: Instance of :doc:`ImageFormat </reference/misc/ImageFormat>` specifying the format of attached data. Optional, defaults to ``null``.
	:param verification_level: Changes the guild's verification level. Optional, defaults to ``null``.
	:param default_message_notifications: Changes default notification settings. Optional, defaults to ``null``.
	:param afk_channel: Voice channel in which to put AFK users connected to voice. Optional, defaults to ``null``.
	:param afk_timeout: Timeout after which to move inactive voice users to AFK channel. Optional, defaults to ``null``.
	:param owner: Changes guild's owner. Optional, defaults to ``null``.
	:param splash: Stream containing splash data for the guild. Must be valid PNG, JPG, or GIF image. Optional, defaults to ``null``. If this is specified, ``splash_format`` must also be specified. Note that only partnered guilds can use this.
	:param splash_format: Instance of :doc:`ImageFormat </reference/misc/ImageFormat>` specifying the format of attached data. Optional, defaults to ``null``.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: BanMemberAsync(member, reason)
	
	Bans a member from this guild.
	
	:param member: An instance of :doc:`GuildMember </reference/GuildMember>` to ban.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: UnbanMemberAsync(user, reason)
	
	Unbans a member from this guild.
	
	:param user: An instance of :doc:`GuildUser </reference/GuildMember>` to unban.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: LeaveAsync()
	
	Leaves this guild.

.. function:: GetBansAsync()
	
	Gets all bans for this guild. Returns a list of :doc:`DiscordUsers </reference/GuildUser>`.

.. function:: CreateChannelAsync(name, type, bitrate, user_limit, overwrites, reason)
	
	Creates and returns a new channel.
	
	:param name: Name of the new channel.
	:param type: Type of the new channel.
	:param bitrate: Bitrate for the channel. This is only applicable to voice channels. Optional, defaults to ``0``.
	:param user_limit: User limit for the channel. This is only applicable to voice channels. Optional, defaults to ``0``.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: GetPruneCountAsync(days)
	
	Estimates and returns the number of users that would be pruned.
	
	:param days: Number of days the users have to be inactive to be pruned.

.. function:: Prune(days, reason)
	
	Prunes inactive users. Returns the number of users pruned.
	
	:param days: Number of days the users have to be inactive to be pruned.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: GetIntegrationsAsync()
	
	Gets this guild's integrations.

.. function:: AttachUserIntegrationAsync(integration)
	
	Attaches an integration to the guild.
	
	:param integration: Integration (instance of :doc:`DiscordIntegration </reference/misc/DiscordIntegration>`) to attach.

.. function:: ModifyIntegrationAsync(integration, expire_behaviour, expire_grace_period, enable_emoticons)
	
	Modifies an integration for this guild.
	
	:param integration: Integration (instance of :doc:`DiscordIntegration </reference/misc/DiscordIntegration>`) to modify.
	:param expire_behaviour: Integration subscription lapse behaviour.
	:param expire_grace_period: Period (in seconds) during which the integration will ignore lapsed subscription.
	:param enable_emoticons: Whether emoticons should be synced to this guild.

.. function:: DeleteIntegrationAsync(integration)
	
	Deletes an integration from this guild.
	
	:param integration: Integration (instance of :doc:`DiscordIntegration </reference/misc/DiscordIntegration>`) to remove.

.. function:: SyncIntegrationAsync(integration)
	
	Syncs an integration to this guild.

	:param integration: Integration (instance of :doc:`DiscordIntegration </reference/misc/DiscordIntegration>`) to remove.

.. function:: GetEmbedAsync()
	
	Gets this guild's widget.

.. function:: GetVoiceRegionsAsync()
	
	Gets the voice regions for this guild. Returns a list of :doc:`DiscordVoiceRegions </reference/misc/DiscordVoiceRegions>`.

.. function:: GetInvitesAsync()
	
	Gets invitations for this guild. Returns a list of :doc:`DiscordInvites </reference/misc/DiscordInvite>`.

.. function:: GetWebhooksAsync()
	
	Gets webhooks for this guild. Returns a list of :doc:`DiscordWebhooks </reference/misc/DiscordWebhook>`.

.. function:: RemoveMemberAsync(member)
	
	Kicks a member from this guild.
	
	:param member: Instance of :doc:`DiscordUser </reference/DiscordUser>` to kick.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: GetMemberAsync(member_id)
	
	Gets a member of this guild by their ID.
	
	:param member_id: ID of the member to get.

.. function:: GetAllMembersAsync()
	
	.. note::
	
		This method can take a while to execute. Once execution is completed, it will fill the guild's member cache.
	
	Dowloads all members of this guild. Returns a list of :doc:`DiscordMembers </reference/DiscordMember>`.

.. function:: GetChannelsAsync()
	
	Gets all channels in this guild. Returns a list of :doc:`DiscordChannels </reference/DiscordChannel>`.

.. function:: ListMembersAsync(limit, after)
	
	Gets paginated list of users. Returns a list of :doc:`DiscordMembers </reference/DiscordMember>`.
	
	:param limit: Max number of members to get. Cannot exceed 100.
	:param after: Id of the last member from previous page.

.. function:: UpdateRoleAsync(role, name, permissions, color, hoist, mentionable, reason)
	
	Modifies a role.
	
	:param role: Role to modify.
	:param name: New name for the role. Optional, defaults to ``null``.
	:param permissions: New permissions for the role. Instance of :doc:`Permissions </reference/misc/Permissions>`. Optional, defaults to ``null``.
	:param color: New color for the role. Optional, defaults to ``null``. For simplicity, you can specify colors as ``0xRRGGBB``.
	:param hoist: Whether the role should be hoisted. Optional, defaults to ``null``.
	:param mentionable: Whether the role should be mentionable. Optional, defaults to ``null``.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: CreateRoleAsync(name, permissions, color, hoist, mentionable, reason)
	
	Creates a new role and returns it.
	
	:param name: Name for the new role. Optional, defaults to ``null``.
	:param permissions: Permissions for the new role. Instance of :doc:`Permissions </reference/misc/Permissions>`. Optional, defaults to ``null``.
	:param color: Color for the new role. Optional, defaults to ``null``. For simplicity, you can specify colors as ``0xRRGGBB``.
	:param hoist: Whether the role should be hoisted. Optional, defaults to ``null``.
	:param mentionable: Whether the role should be mentionable. Optional, defaults to ``null``.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: DeleteRoleAsync(role, reason)

	Deletes a role.
	
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: GetRole(id)

	Gets a role by ID.
	
	:param id: ID of the role to get.

.. function:: AddRoleAsync(member, role, reason)
	
	Adds a single role to specified user.
	
	:param member: Member to add the role to.
	:param role: Role to add to the member.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: RemoveRoleAsync(member, role, reason)
	
	Removes a single role from specified user.
	
	:param member: Member to remove the role from.
	:param role: Role to remove from the member.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: GetAuditLogsAsync(limit, by_member, action_type)

	Gets audit logs entries for the guild. Returns a collection of :doc:`DiscordAuditLogEntry </reference/audit-logs/DiscordAuditLogEntry>`.
	
	:param limit: Maximum number of audit log entries to get. Optional, defaults to ``null``.
	:param by_member: Filter by responsible member. Optional, defaults to ``null``.
	:param action_type: Filter by action type. Optional, defaults to ``null``.