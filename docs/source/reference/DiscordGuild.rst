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

