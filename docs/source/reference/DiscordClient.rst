Reference for ``DiscordClient``
===============================

``DiscordClient`` is the heart of the library and your bot. It's the class that takes care of dispatching events,
communicating on your bot's behalf, and performing other tasks.

Constructors
------------

.. function:: DiscordClient(config)

	Initializes the client with specified configuration.

	:param config: An instance of :doc:`DiscordConfig </reference/misc/DiscordConfig>`. Used to specify the configuration options for the client.

Events
------

.. attribute:: ClientError

	Called whenever any other event handler throws an exception. Takes ``ClientErrorEventArgs`` as an argument, with following parameters:

	:param Exception: The exception that was thrown by the event handler.
	:param EventName: Name of the event that threw the exception.

.. attribute:: SocketError

	Called whenever any websocket operation throws an exception. Takes ``SocketErrorEventArgs`` as an argument, with following parameters:

	:param Exception: The exception that was thrown by websocket client.

.. attribute:: SocketOpened

	Called when the WebSocket connection is established. Takes no arguments.

.. attribute:: SocketClosed

	Called when the WebSocket connection is closed. Takes ``SocketDisconnectEventArgs`` as an argument, with following parameters:

	:param CloseCode: Numeric close code for the close event. Can be used to determine whether the socket was closed as a result of an error or other reasons.
	:param CloseMessage: Message attached to close message. This can often be used to determine the exact cause of connection termination.

.. attribute:: Ready

	Called when the client enters ready state. Takes no arguments.

.. attribute:: ChannelCreated

	Called when a new channel is created. Takes ``ChannelCreateEventArgs`` as an argument, with following parameters:

	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just created.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the channel was created in.

.. function DmChannelCreated

	Called when a new direct message channel is created. Takes ``DmChannelCreateEventArgs`` as an argument, with following
	parameters:

	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just created.

.. attribute:: ChannelUpdated

	Called when an existing channel is updated. Takes ``ChannelUpdateEventArgs`` as an argument, with following
	parameters:

	:param ChannelAfter: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just updated.
	:param ChannelBefore: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) before it was updated.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the channel was updated in.

.. attribute:: ChannelDeleted

	Called when an existing channel is deleted. Takes ``ChannelDeleteEventArgs`` as an argument, with following
	parameters:

	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just deleted.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the channel was deleted in.

.. attribute:: DmChannelDeleted

	Called when an existing direct message channel is deleted. Takes ``DmChannelDeleteEventArgs`` as an argument, with following
	parameters:

	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just deleted.

.. attribute:: ChannelPinsUpdated

	Called whenever pins in a channel are updated. Takes ``ChannelPinsUpdateEventArgs`` as an argument, with following parameters:
	
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that had its pins updated.
	:param LastPinTimestamp: Date and time of last message being pinned.

.. attribute:: GuildCreated

	Called when a new guild is created or joined. Takes ``GuildCreateEventArgs`` as an argument, with following parameters:

	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that was just created or joined.

.. attribute:: GuildAvailable

	Called when a guild becomes available. Takes ``GuildCreateEventArgs`` as an argument, with following parameters:

	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that has just become available.

.. attribute:: GuildUpdated

	Called when a guild is updated. Takes ``GuildUpdateEventArgs`` as an argument, with following parameters:

	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that was just updated.

.. attribute:: GuildDeleted

	Called when a guild is left or deleted. Takes ``GuildDeleteEventArgs`` as an argument, with following parameters:

	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that was just left or deleted.
	:param Unavailable: Whether the guild is unavailable or not.

.. attribute:: GuildUnavailable

	Called when a guild becomes unavailable. Takes ``GuildDeleteEventArgs`` as an argument, with following parameters:

	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that was just left or deleted.
	:param Unavailable: Whether the guild is unavailable or not.

.. attribute:: MessageCreated

	Called when the client receives a new message. Takes ``MessageCreateEventArgs`` as an argument, with following
	parameters:

	:param Message: The message (instance of :doc:`DiscordMessage </reference/DiscordMessage>`) that was received.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the message was sent in.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the message was sent in. This parameter is ``null`` for direct messages.
	:param Author: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that sent the message.
	:param MentionedUsers: A list of :doc:`DiscordMember </reference/DiscordMember>` that were mentioned in this message.
	:param MentionedRoles: A list of :doc:`DiscordRole </reference/DiscordRole>` that were mentioned in this message.
	:param MentionedChannels: A list of :doc:`DiscordChannel </reference/DiscordChannel>` that were mentioned in this message.

.. attribute:: PresenceUpdate

	Called when a presence update occurs. Takes ``PresenceUpdateEventArgs`` as an argument, with following parameters:

	:param Member: The member (instance of :doc:`DiscordMember </reference/DiscordMember>`) whose presence was updated.
	:param Game: Game (instance of :doc:`Game </reference/misc/Game>`) the user is playing or streaming.
	:param Status: User's status (online, idle, do not disturb, or offline).
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the presence update occured in.
	:param Roles: A list of :doc:`DiscordRole </reference/DiscordRole>` in the given guild.
	:param PresenceBefore: User's presence before it was updated.

.. attribute:: GuildBanAdd

	Called whenever a user gets banned from a guild. Takes ``GuildBanAddEventArgs`` as an argument, with following
	parameters:

	:param Member: The member (instance of :doc:`DiscordMember </reference/DiscordMember>`) that got banned.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the user got banned from.

.. attribute:: GuildBanRemove

	Called whenever a user gets unbanned from a guild. Takes ``GuildBanRemoveEventArgs`` as an argument, with
	following parameters:

	:param Member: The member (instance of :doc:`DiscordMember </reference/DiscordMember>`) that got unbanned.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the user got unbanned from.

.. attribute:: GuildEmojisUpdate

	Called whenever a guild has its emoji updated. Takes ``GuildEmojisUpdateEventArgs`` as an argument, with the
	following parameters:

	:param EmojisAfter: A list of :doc:`DiscordEmoji </reference/entities/DiscordEmoji>` that got updated.
	:param EmojisBefore: A list of :doc:`DiscordEmoji </reference/entities/DiscordEmoji>` before they got updated.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that had its emoji updated.

.. attribute:: GuildIntegrationsUpdate

	Called whenever a guild has its integrations updated. Takes ``GuildIntegrationsUpdateEventArgs`` as an argument,
	with following parameters:

	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that had its integrations updated.

.. attribute:: GuildMemberAdd

	Called whenever a member joins a guild. Takes ``GuildMemberAddEventArgs`` as an argument, with following
	parameters:

	:param Member: The member (instance of :doc:`DiscordMember </reference/DiscordMember>`) that joined the guild.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the member joined.

.. attribute:: GuildMemberRemove

	Called whenever a member leaves a guild. Takes ``GuildMemberRemoveEventArgs`` as an argument, with following
	parameters:

	:param Member: The member (instance of :doc:`DiscordMember </reference/DiscordMember>`) that left the guild.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the member left.

.. attribute:: GuildMemberUpdate

	Called whenever a guild member is updated. Takes ``GuildMemberUpdateEventArgs`` as an argument, with following
	parameters:

	:param GuildID: ID of the guild in which the update occured.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) in which the update occured.
	:param NickName: New nickname of the member.
	:param NickNameBefore: Old nickname of the member.
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that got updated.
	:param RolesAfter: A list of :doc:`DiscordRoles </reference/DiscordRole>` the member has after the update.
	:param RolesBefore: A list of :doc:`DiscordRoles </reference/DiscordRole>` the member had before the update.

.. attribute:: GuildRoleCreate

	Called whenever a role is created in a guild. Takes ``GuildRoleCreateEventArgs`` as an argument, with following
	parameters:

	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the role was created in.
	:param Role: The role (instance of :doc:`DiscordRole </reference/DiscordRole>`) that was created.

.. attribute:: GuildRoleUpdate

	Called whenever a role is updated in a guild. Takes ``GuildRoleUpdateEventArgs`` as an argument, with following
	parameters:

	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the role was updated in.
	:param RoleAfter: The role (instance of :doc:`DiscordRole </reference/DiscordRole>`) that was updated.
	:param RoleBefore: The role (instance of :doc:`DiscordRole </reference/DiscordRole>`) before it was updated.

.. attribute:: GuildRoleDelete

	Called whenever a role is deleted in a guild. Takes ``GuildRoleDeleteEventArgs`` as an argument, with following
	parameters:

	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the role was deleted in.
	:param Role: The role (instance of :doc:`DiscordRole </reference/DiscordRole>`) that was deleted.

.. attribute:: MessageUpdate

	Called whenever a message is updated. Takes ``MessageUpdateEventArgs`` as an argument, with following parameters:

	:param Message: The message (instance of :doc:`DiscordMessage </reference/DiscordMessage>`) that was updated.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the message was updated in.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the message was updated in. This parameter is ``null`` for direct messages.
	:param Author: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that updated the message.
	:param MentionedUsers: A list of :doc:`DiscordMember </reference/DiscordMember>` that were mentioned in this message.
	:param MentionedRoles: A list of :doc:`DiscordRole </reference/DiscordRole>` that were mentioned in this message.
	:param MentionedChannels: A list of :doc:`DiscordChannel </reference/DiscordChannel>` that were mentioned in this message.

.. attribute:: MessageDelete

	Called whenever a message is deleted. Takes ``MessageDeleteEventArgs`` as an argument, with following parameters:

	:param Message: The message (instance of :doc:`DiscordMessage </reference/DiscordMessage>`) that was deleted.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the message was deleted in.

.. attribute:: MessageBulkDelete

	Called whenever several messages are deleted at once. Takes ``MessageBulkDeleteEventArgs`` as an argument, with
	following parameters:

	:param Messages: A list of :doc:`DiscordMessages </reference/DiscordMessage>` of messages that were deleted.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the messages were deleted in.

.. attribute:: TypingStart

	Called whenever a user starts typing in a channel. Takes ``TypingStartEventArgs`` as an argument, with following
	parameters:

	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the user started typing in.
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that started typing.
	:param StartedAt: The timestamp of when user started typing.

.. attribute:: UserSettingsUpdate

	Called whenever user's settings are updated. Takes ``UserSettingsUpdateEventArgs`` as an argument, with following
	parameters:

	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) whose settings were updated.

.. attribute:: UserUpdate

	Called whenever a user is updated. Takes ``UserUpdateEventArgs`` as an argument, with following parameters:

	:param UserAfter: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that was updated.
	:param UserBefore: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) before it was updated.

.. attribute:: VoiceStateUpdate

	Called whenever a user's voice state is updated. Takes ``VoiceStateUpdateEventArgs`` as an argument, with
	following parameters:

	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) whose voice state was updated.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) where the voice state update occured.
	:param Channel: The channel (instance of :doc`DiscordChannel </reference/DiscordChannel>`) to which the user connected. Null if the user disconnected from voice.

.. attribute:: VoiceServerUpdate

	.. note::

		This event is used when negotiating voice information with Discord. It shouldn't be used by bots.

	Called whenever voice connection data is sent to the client. Takes ``VoiceServerUpdateEventArgs`` as an argument,
	with following parameters:

	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the client is connecting to.
	:param Endpoint: Voice endpoint to connect to.

.. attribute:: GuildMembersChunk

	.. note::

		This event is used when connecting to discord and requesting more members. It shouldn't be used by bots.

	Called whenever another batch of guild members is sent to client. Takes ``GuildMembersChunkEventArgs`` as an
	argument, with following parameters:

	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) for which the members were received.
	:param Members: A list of :doc:`DiscordMembers </reference/DiscordMember>` received in this chunk.

.. attribute:: UnknownEvent

	.. note::

		This event is invoked whenever discord dispatches an event that is not yet implemented in the library. 
		
		In case you ever get this event, please report it on the 
		`issue tracker <https://github.com/NaamloosDT/DSharpPlus/issues>`_ with details.

	Called whenever an unknown event is dispatched to the client. Takes ``UnknownEventArgs`` as an argument, with
	following parameters:

	:param EventName: Event's name.
	:param Json: Event's payload.

.. attribute:: MessageReactionAdd

	Called whenever a message has a reaction added to it. Takes ``MessageReactionAddEventArgs`` as an argument, with
	following parameters:

	:param Emoji: The emoji (instance of :doc:`DiscordEmoji </reference/DiscordEmoji>`) that was used to react to the message.
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) who reacted to the message.
	:param Message: The message (instance of :doc:`DiscordMessage </reference/DiscordMessage>`) the reaction was added to.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the message is located in.

.. attribute:: MessageReactionRemove

	Called whenever a message has a reaction removed from it. Takes ``MessageReactionRemoveEventArgs`` as an argument,
	with following parameters:

	:param Emoji: The emoji (instance of :doc:`DiscordEmoji </reference/DiscordEmoji>`) that was used to react to the message.
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) who removed the reaction.
	:param Message: The message (instance of :doc:`DiscordMessage </reference/DiscordMessage>`) the reaction was removed from.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the message is located in.

.. attribute:: MessageReactionRemoveAll

	Called whenever a message has all of its reactions remvoed from it. Takes ``MessageReactionRemoveAllEventArgs`` as
	an argument, with following parameters:

	:param Message: The message (instance of :doc:`DiscordMessage </reference/DiscordMessage>`) the reactions were removed from.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the message is located in.

.. attribute:: WebhooksUpdate

	Called whenever webhooks are updated. Takes ``WebhooksUpdateEventArgs`` as an argument, with following parameters:

	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the webhook was updated in.
	:param Channe: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the webhook was updated in.

Members
-------

.. attribute:: DebugLogger

	An instance of :doc:`DebugLogger </reference/misc/DebugLogger>` used to log messages from the library.

.. attribute:: GatewayVersion

	Version of the gateway used by the library.

.. attribute:: GatewayUrl

	URL of the gateway used by the library.

.. attribute:: ShardCount

	Total number of shards the bot is connected to.
	
.. attribute:: ShardId

	ID of the shard given instance of DiscordClient is connected to.

.. attribute:: CurrentUser

	The user the bot is connected as (instance of :doc:`DiscordUser </reference/DiscordUser>`).

.. attribute:: CurrentApplication

	The application the bot belongs to (instance of :doc:`DiscordApplication </reference/misc/DiscordApplication>`). This is null for user tokens.

.. attribute:: PrivateChannels

	List of DM channels (instances of :doc:`DiscordDMChannel </reference/DiscordDmChannel>`).

.. attribute:: Guilds

	A dictionary of guilds (instances of :doc:`DiscordGuild </reference/DiscordGuild>`) the bot is in.

.. attribute:: Ping

	Last websocket latency measured during heartbeating.

.. attribute:: Presences

	Presences of all known users that the bot received.

Methods
-------

.. function:: SetWebSocketClient<T>()

	Sets the websocket client implementation the client will use. See :doc:`Alternative WebSocket and UDP implementations </getting-started/alternative-socket-clients>` for more information.
	
	``T`` needs to be a type that inherits from :doc:`BaseWebSocketClient </reference/socket/BaseWebSocketClient>` and has a default constructor.
	
	In most cases you won't need to use this method.

.. function:: SetUdpClient<T>()

	Sets the UDP client implementation the client will use. This only matters when you intend to use Voice. See :doc:`Alternative WebSocket and UDP implementations </getting-started/alternative-socket-clients>` for more information.
	
	``T`` needs to be a type that inherits from :doc:`BaseUdpClient </reference/socket/BaseUdpClient>` and has a default constructor.
	
	In most cases you won't need to use this method.

.. function:: AddModule(module)

	Adds a module to the client, and returns it.

	:param module: An instance of a class implementing :doc:`IModule </reference/misc/IModule>` interface.

.. function:: GetModule<T>(module)

	Finds and returns an instance of the module specified by the generic argument. ``T`` needs to be a class
	implementing :doc:`IModule </reference/misc/IModule>` interface.

.. function:: ConnectAsync()

	Connects to Discord and begins dispatching events. This method will make up to 5 connection attempts with exponential backoff. Failing that, it will throw.

.. function:: ReconnectAsync(start_new_session)

	Reconnects with Discord.

	:param start_new_session: Whether to start a new session upon successfully reconnecting. Defaults to false.

.. function:: DisconnectAsync()

	Disconnects from Discord and stops dispatching events.

.. function:: GetUserAsync(user)

	Gets a user by their ID.

	:param name: Id of the user or ``"@me"``.

.. function:: DeleteChannelAsync(channel, reason)

	Deletes a channel, optionally specifying a reason for audit logs.

	:param channel: An instance of :doc:`DiscordChannel </reference/DiscordChannel>` to delete.
	:param reason: Reason for channel's deletion. This gets put in guild's audit logs. Optional, defaults to ``null``.

.. function:: GetMessageAsync(channel, message_id)

	Gets a specified message from the specified channel.

	:param channel: An instance of :doc:`DiscordChannel </reference/DiscordChannel>` to get the message from.
	:param message_id: ID of the message to get.

.. function:: GetChannelAsync(id)

	Gets a channel.

	:param id: ID of the channel to get.

.. function:: SendMessageAsync(channel, contents, tts, embed)

	Sends a message to specified channel.

	:param channel: An instance of :doc:`DiscordChannel </reference/DiscordChannel>` to send the message to.
	:param contents: Contents of the message to send.
	:param tts: Whether the message is a TTS message or not. Optional, defaults to ``false``.
	:param embed: Embed to attach to the message. Optional, defaults to ``null``.

.. function:: CreateGuildAsync(name, region, icon, icon_format, verification_level, default_message_notifications)

	Creates a new guild and returns it.

	:param name: Name of the guild to create.
	:param region: Voice region for the guild. Optional, defaults to ``null``.
	:param icon: Stream containing icon data for the guild. Optional, defaults to ``null``. If this is specified, ``icon_format`` must also be specified.
	:param icon_format: Instance of :doc:`ImageFormat </reference/misc/ImageFormat>` specifying the format of attached data. Optional, defaults to ``null``.
	:param verification_level: Verification level for the guild. Instance of :doc:`VerificationLevel </reference/misc/VerificationLevel>`. Optional, defaults to ``null``.
	:param default_message_notifications: Default message notification level. Instance of :doc:`DefaultMessageNotifications </reference/misc/DefaultMessageNotifications>`. Optional, defaults to ``null``.

.. function:: GetGuildAsync(id)

	Gets a guild by ID.

	:param id: ID of the guild to get.

.. function:: DeleteGuildAsync(guild)

	Deletes a guild.

	:param guild: An instance of :doc:`DiscordGuild </reference/DiscordGuild>` to delete.

.. function:: GetInviteByCodeAsync(code)

	.. note::

		This method is not usable by bot users.

	.. warning::

		Using this method on a user account will unverify your account and flag you for raiding.

	Gets a guild invite by code.

	:param code: Invite code to get the invite for.

.. function:: GetConnectionsAsync()

	Gets connections for the current user.

.. function:: ListRegionsAsync()

	Gets the list of voice regions.

.. function:: GetWebhookAsync(id)

	Gets a webhook by ID.

	:param id: ID of the webhook to get.

.. function:: GetWebhookWithTokenAsync(id, token)

	Gets a webhook with a token by ID.

	:param id: ID of the webhook to get.
	:param token: Webhook's token.

.. function:: CreateDmAsync(id)

	.. note::
	
		You need to share at least one guild with the target user or this method will fail.

	Creates a direct message channel between the bot and the specified user.

	:param id: ID of the user to create a DM channel with.

.. function:: UpdateStatusAsync(game, user_status, idle_since, afk)

	Updates current user's status.

	:param game: Game (instance of :doc:`Game </reference/misc/Game>`) to set in the status. Optional, defaults to ``null``.
	:param user_status: User status (instance of :doc:`UserStatus </reference/misc/UserStatus>`) to set. Optional, defaults to ``null``.
	:param idle_since: How long has the user been idle. Optional, defaults to ``null``.
	:param afk: Whether the user is away from keyboard. Optional, defaults to ``null``.

.. function:: GetCurrentAppAsync()

	.. note:: 
	
		This method will fail for regular users.

	Gets the current application.

.. function:: EditCurrentUserAsync(username, avatar, avatar_format)

	Edits the current user.
	
	:param username: New username to set. Optional, defaults to ``null``.
	:param avatar: Stream containing avatar data for the user. Optional, defaults to ``null``. If this is specified, ``avatar_format`` must also be specified.
	:param avatar_format: Instance of :doc:`ImageFormat </reference/misc/ImageFormat>` specifying the format of attached data. Optional, defaults to ``null``.

Additional notes
----------------

``DiscordClient`` has several extensions available, which extend its functionality in various ways.

CommandsNext module
^^^^^^^^^^^^^^

Several command-specific :doc:`extensions </reference/commands/DiscordClient-extensions>` are defined in that module, which enable the its usage.

VoiceNext module
^^^^^^^^^^^^^^^^

Several voice-specific :doc:`extensions </reference/voice/DiscordClient-extensions>` are defined in that module, which enable the its usage.