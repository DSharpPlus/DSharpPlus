Reference for ``DiscordClient``
=================================

``DiscordClient`` is the heart of the library and your bot. It's the class that takes care of dispatching events, 
communicating on your bot's behalf, and performing other tasks.

Constructors
--------------

.. function:: DiscordClient()

	.. note::
	
		It is generally a better idea to use the constructor which allows you to specify a configuration.

	Initializes the client with default configuration.

.. function:: DiscordClient(config)

	Initializes the client with specified configuration.
	
	:param config: An instance of :doc:`</reference/DiscordConfig>`. Used to specify the configuration options for the 
	client.

Events
--------

Events are the key to making any bot work. All events are asynchronous, meaning that all event handlers must return a 
``Task`` instance. For lambda and function handlers marked ``async`` this is automatic. For non-``async`` lambdas and 
functions, you need to ``return Task.Delay(0)`` at the end of the handler, or make it ``async``, and begin with 
``await Task.Yield()``. If an event takes no argument, your handler cannot take any either, otherwise it takes one 
argument, which is an appropriate ``EventArgs`` instance.

Event usage
^^^^^^^^^^^^^

Events can be used in 2 ways. Via lambdas or functions. In C#, the handler needs to return ``Task``, and take 
appropriate arguments.

For events without arguments, following methods are acceptable: ::

	// lambda approach

	client.Event += async () =>
	{
		// do something async
		await client.SomethingAsync();
	};
	
	client.Event += async () =>
	{
		await Task.Yield();
		// do something non-async
	};
	
	client.Event += () =>
	{
		// do something non-async
		client.SomethingNonAsync();
		
		return Task.Delay(0);
	};
	
	// function approach
	
	client.Event += MyHandler;
	
	// later in the code
	
	public async Task MyHandler()
	{
		// do something async
		await client.SomethingAsync();
	}
	
	public async Task MyHandler()
	{
		await Task.Yield();
		// do something non-async
	}
	
	public Task MyHandler()
	{
		// do something non-async
		client.SomethingNonAsync();
		
		return Task.Delay(0);
	}

In Visual Basic, you can only use Function handlers, however the rest still applies. Additionally, the client instance 
need to be defined with the ``WithEvents`` keyword, for example ``Public WithEvents Client As DiscordClient``. 

Handlers in VB.NET can only be functions, but the rest still appplies: ::

	Public Async Function OnEvent() As Task Handles Client.Event
	
		' do something async
		Await Client.SomethingAsync()
	
	End Function
	
	Public Async Function OnEvent() As Task Handles Client.Event
	
		Await Task.Yield()
		' do something non-async
	
	End Function
	
	Public Function OnEvent() As Task Handles Client.Event
	
		' do something non-async
		Client.SomethingNonAsync()
		
		Return Task.Delay(0)
	
	End Function

For events that take arguments, you need to make your handler take arguments too, for instance:::

	// lambda approach

	client.Event += async e =>
	{
		// do something async
		await e.SomethingAsync();
	};
	
	client.Event += async e =>
	{
		await Task.Yield();
		// do something non-async
	};
	
	client.Event += e =>
	{
		// do something non-async
		e.SomethingNonAsync();
		
		return Task.Delay(0);
	};
	
	// function approach
	
	client.Event += MyHandler;
	
	// later in the code
	
	public async Task MyHandler(EventEventArgs e)
	{
		// do something async
		await e.SomethingAsync();
	}
	
	public async Task MyHandler(EventEventArgs e)
	{
		await Task.Yield();
		// do something non-async
	}
	
	public Task MyHandler(EventEventArgs e)
	{
		// do something non-async
		e.SomethingNonAsync();
		
		return Task.Delay(0);
	}

Similarly, in Visual Basic: ::

	Public Async Function OnEvent(ByVal e As EventEventArgs) As Task Handles Client.Event
	
		' do something async
		Await e.SomethingAsync()
	
	End Function
	
	Public Async Function OnEvent(ByVal e As EventEventArgs) As Task Handles Client.Event
	
		Await Task.Yield()
		' do something non-async
	
	End Function
	
	Public Function OnEvent(ByVal e As EventEventArgs) As Task Handles Client.Event
	
		' do something non-async
		e.SomethingNonAsync()
		
		Return Task.Delay(0)
	
	End Function

Event reference
^^^^^^^^^^^^^^^^^

Below you can find complete event reference.

.. attribute:: SocketOpened

	Called when the WebSocket connection is established. Takes no arguments.

.. attribute:: SocketClosed

	Called when the WebSocket connection is closed. Takes no arguments.

.. attribute:: Ready

	Called when the client enters ready state. Takes no arguments.

.. attribute:: ChannelCreated

	Called when a new channel is created. Takes ``ChannelCreateEventArgs`` as an argument, with following parameters:
	
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just created.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the channel was created 
	in.

.. function DMChannelCreated

	Called when a new DM channel is created. Takes ``DMChannelCreateEventArgs`` as an argument, with following 
	parameters:
	
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just created.

.. attribute:: ChannelUpdated

	Called when an existing channel is updated. Takes ``ChannelUpdateEventArgs`` as an argument, with following 
	parameters:
	
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just updated.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the channel was updated 
	in.

.. attribute:: ChannelDeleted

	Called when an existing channel is deleted. Takes ``ChannelDeleteEventArgs`` as an argument, with following 
	parameters:
	
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just deleted.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the channel was deleted 
	in.

.. attribute:: DMChannelDeleted

	Called when an existing DM channel is deleted. Takes ``DMChannelDeleteEventArgs`` as an argument, with following 
	parameters:
	
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just deleted.

.. attribute:: GuildCreated

	Called when a new guild is created. Takes ``GuildCreateEventArgs`` as an argument, with following parameters:
	
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that was just created.

.. attribute:: GuildAvailable

	Called when a guild becomes available. Takes ``GuildCreateEventArgs`` as an argument, with following parameters:
	
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that has just become available.

.. attribute:: GuildUpdated

	Called when a guild is updated. Takes ``GuildUpdateEventArgs`` as an argument, with following parameters:
	
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that was just updated.

.. attribute:: GuildDeleted

	Called when a guild is deleted. Takes ``GuildDeleteEventArgs`` as an argument, with following parameters:
	
	:param ID: ID of the guild that was just deleted.
	:param Unavailable: Whether the guild is unavailable or not.

.. attribute:: GuildUnavailable

	Called when a guild becomes unavailable. Takes ``GuildDeleteEventArgs`` as an argument, with following parameters:
	
	:param ID: ID of the guild that has just become unavailable.
	:param Unavailable: Whether the guild is unavailable or not.

.. attribute:: MessageCreated

	Called when the client receives a new message. Takes ``MessageCreateEventArgs`` as an argument, with following 
	parameters:
	
	:param Message: The message (instance of :doc:`DiscordMessage </reference/DiscordMessage>`) that was received.
	:param MentionedUsers: A list of :doc:`DiscordMember </reference/DiscordMember>`s that were mentioned in this 
	message.
	:param MentionedRoles: A list of :doc:`DiscordRole </reference/DiscordRole>`s that were mentioned in this message.
	:param MentionedChannels: A list of :doc:`DiscordChannel </reference/DiscordChannel>`s that were mentioned in this 
	message.
	:param UsedEmojis: A list of :doc:`DiscordEmoji </reference/DiscordEmoji>`s that were used in this message.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the message was sent in.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the message was sent in. This 
	parameter is ``null`` for direct messages.
	:param Author: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that sent the message.

.. attribute:: PresenceUpdate

	Called when a presence update occurs. Takes ``PresenceUpdateEventArgs`` as an argument, with following parameters:
	
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) whose presence was updated.
	:param UserID: The ID of the user whose presence was updated.
	:param Game: Game the user is playing or streaming.
	:param Status: User's status (online, idle, do not disturb, or offline).
	:param GuildID: ID of the guild the presence update occured in.
	:param RoleIDs: IDs of user's roles in the given guild.

.. attribute:: GuildBanAdd

	Called whenever a user gets banned from a guild. Takes ``GuildBanAddEventArgs`` as an argument, with following 
	parameters:
	
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that got banned.
	:param GuildID: ID of the guild the user got banned from.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the user got banned from.

.. attribute:: GuildBanRemove

	Called whenever a user gets unbanned from a guild. Takes ``GuildBanRemoveEventArgs`` as an argument, with 
	following parameters:
	
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that got unbanned.
	:param GuildID: ID of the guild the user got unbanned from.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the user got unbanned from.

.. attribute:: GuildEmojisUpdate

	Called whenever a guild has its emoji updated. Takes ``GuildEmojisUpdateEventArgs`` as an argument, with the 
	following parameters:
	
	:param Emojis: A list of :doc:`DiscordEmoji </reference/DiscordEmoji>`s that got updated.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that had its emoji updated.

.. attribute:: GuildIntegrationsUpdate

	Called whenever a guild has its integrations updated. Takes ``GuildIntegrationsUpdateEventArgs`` as an argument, 
	with following parameters:
	
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that had its integrations 
	updated.

.. attribute:: GuildMemberAdd

	Called whenever a member joins a guild. Takes ``GuildMemberAddEventArgs`` as an argument, with following 
	parameters:
	
	:param Member: The member (instance of :doc:`DiscordMember </reference/DiscordMember>`)
	:param GuildID: ID of the guild the memeber joined.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the member joined.

.. attribute:: GuildMemberRemove

	Called whenever a member leaves a guild. Takes ``GuildMemberRemoveEventArgs`` as an argument, with following 
	parameters:
	
	:param GuildID: ID of the guild that the member left.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the member left.
	:param User: The member (instance of :doc:`DiscordUser </reference/DiscordUser>`) that left the guild.

.. attribute:: GuildMemberUpdate

	Called whenever a guild member is updated. Takes ``GuildMemberUpdateEventArgs`` as an argument, with following 
	parameters:
	
	:param GuildID: ID of the guild in which the update occured.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) in which the update occured.
	:param Roles: A list of role IDs for the member.
	:param Nickname: New nickname of the member.
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that got updated.

.. attribute:: GuildRoleCreate

	Called whenever a role is created in a guild. Takes ``GuildRoleCreateEventArgs`` as an argument, with following 
	parameters:
	
	:param GuildID: ID of the guild the role was created in.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the role was created in.
	:param Role: The role (instance of :doc:`DiscordRole </reference/DiscordRole>`) that was created.

.. attribute:: GuildRoleUpdate

	Called whenever a role is updated in a guild. Takes ``GuildRoleUpdateEventArgs`` as an argument, with following 
	parameters:
	
	:param GuildID: ID of the guild the role was updated in.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the role was updated in.
	:param Role: The role (instance of :doc:`DiscordRole </reference/DiscordRole>`) that was updated.

.. attribute:: GuildRoleDelete

	Called whenever a role is deleted in a guild. Takes ``GuildRoleDeleteEventArgs`` as an argument, with following 
	parameters:
	
	:param GuildID: ID of the guild the role was deleted in.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the role was deleted in.
	:param Role: The role (instance of :doc:`DiscordRole </reference/DiscordRole>`) that was deleted.

.. attribute:: MessageUpdate

	Called whenever a message is updated. Takes ``MessageUpdateEventArgs`` as an argument, with following parameters:
	
	:param Message: The message (instance of :doc:`DiscordMessage </reference/DiscordMessage>`) that was updated.
	:param MentionedUsers: A list of :doc:`DiscordMember </reference/DiscordMember>`s that were mentioned in this 
	message.
	:param MentionedRoles: A list of :doc:`DiscordRole </reference/DiscordRole>`s that were mentioned in this message.
	:param MentionedChannels: A list of :doc:`DiscordChannel </reference/DiscordChannel>`s that were mentioned in this 
	message.
	:param UsedEmojis: A list of :doc:`DiscordEmoji </reference/DiscordEmoji>`s that were used in this message.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the message was 
	updated in.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the message was updated in. This 
	parameter is ``null`` for direct messages.
	:param Author: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that updated the message.

.. attribute:: MessageDelete

	Called whenever a message is deleted. Takes ``MessageDeleteEventArgs`` as an argument, with following parameters:
	
	:param MessageID: ID of the message that was deleted.
	:param ChannelID: ID of the channel the message was deleted in.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the message was 
	deleted in.

.. attribute:: MessageBulkDelete

	Called whenever several messages are deleted at once. Takes ``MessageBulkDeleteEventArgs`` as an argument, with 
	following parameters:
	
	:param MessageIDs: A list of IDs of messages that were deleted.
	:param ChannelID: ID of the channel the messages were deleted in.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the messages were 
	deleted in.

.. attribute:: TypingStart

	Called whenever a user starts typing in a channel. Takes ``TypingStartEventArgs`` as an argument, with following 
	parameters:
	
	:param ChannelID: ID of the channel the user started typing in.
	:param UserID: ID of the user that started typing.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the user started 
	typing in.
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that started typing.

.. attribute:: UserSettingsUpdate

	Called whenever user's settings are updated. Takes ``UserSettingsUpdateEventArgs`` as an argument, with following 
	parameters:
	
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) whose settings were updated
	
.. attribute:: UserUpdate

	Called whenever a user is updated. Takes ``UserUpdateEventArgs`` as an argument, with following parameters:
	
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that was updated

.. attribute:: VoiceStateUpdate

	Called whenever a user's voice state is updated. Takes ``VoiceStateUpdateEventArgs`` as an argument, with 
	following parameters:
	
	:param UserID: ID of the user whose voice state was updated.
	:param GuildID: ID of the guild where the voice state update occured.
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) whose voice state was updated.
	:param SessionID: ID of the voice session for the user.

.. attribute:: VoiceServerUpdate

	.. note::
	
		This event is used when negotiating voice information with Discord. It shouldn't be used by bots.

	Called whenever voice connection data is sent to the client. Takes ``VoiceServerUpdateEventArgs`` as an argument, 
	with following parameters:
	
	:param VoiceToken: Token for the voice session.
	:param GuildID: ID of the guild the client is connecting to.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the client is connecting to.
	:param Endpoint: Voice endpoint to connect to.

.. attribute:: GuildMembersChunk

	.. note::
	
		This event is used when connecting to discord and requesting more members. It shouldn't be used by bots.
	
	Called whenever another batch of guild members is sent to client. Takes ``GuildMembersChunkEventArgs`` as an 
	argument, with following parameters:
	
	:param GuildID: ID of the guild for which the members were received.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) for which the members were 
	received.
	:param Members: A list of :doc:`DiscordMember </reference/DiscordMember>`s received in this chunk.

.. attribute:: UnknownEvent

	.. warning::
	
		This event indicates something went terribly wrong. If you ever see this event, please report it on the 
		`issue tracker <https://github.com/NaamloosDT/DSharpPlus/issues>`_ with details.
	
	Called whenever an unknown event is dispatched to the client. Takes ``UnknownEventArgs`` as an argument, with 
	following parameters:
	
	:param EventName: Event's name.
	:param Json: Event's payload.

.. attribute:: MessageReactionAdd

	Called whenever a message has a reaction added to it. Takes ``MessageReactionAddEventArgs`` as an argument, with 
	following parameters:
	
	:param UserID: ID of the user who added the reaction.
	:param MessageID: ID of the message the reaction was added to.
	:param ChannelID: ID of the channel the message is located in.
	:param Emoji: The emoji (instance of :doc:`DiscordEmoji </reference/DiscordEmoji>`) that was used to react to the 
	message.
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) who reacted to the message.
	:param Message: The message (instance of :doc:`DiscordMessage </reference/DiscordMessage>`) the reaction was added 
	to.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the message is located 
	in.

.. attribute:: MessageReactionRemove

	Called whenever a message has a reaction removed from it. Takes ``MessageReactionRemoveEventArgs`` as an argument, 
	with following parameters:
	
	:param UserID: ID of the user who removed the reaction.
	:param MessageID: ID of the message the reaction was removed from.
	:param ChannelID: ID of the channel the message is located in.
	:param Emoji: The emoji (instance of :doc:`DiscordEmoji </reference/DiscordEmoji>`) that was used to react to the 
	message.
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) who removed the reaction.
	:param Message: The message (instance of :doc:`DiscordMessage </reference/DiscordMessage>`) the reaction was 
	removed from.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the message is located 
	in.

.. attribute:: MessageReactionRemoveAll

	Called whenever a message has all of its reactions remvoed from it. Takes ``MessageReactionRemoveAllEventArgs`` as 
	an argument, with following parameters:
	
	:param MessageID: ID of the message the reactions were removed from.
	:param ChannelID: ID of the channel the message is located in.
	:param Message: The message (instance of :doc:`DiscordMessage </reference/DiscordMessage>`) the reactions were 
	removed from.
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the message is located 
	in.

.. attribute:: WebhooksUpdate

	Called whenever webhooks are updated. Takes ``WebhooksUpdateEventArgs`` as an argument, with following parameters:
	
	:param GuildID: ID of the guild the webhook was updated in.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the webhook was updated in.
	:param ChannelID: ID of the channel the webhook was updated in.
	:param Channe: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) the webhook was updated 
	in.

Members
---------

.. attribute:: DebugLogger

	An instance of :doc:`DebugLogger </reference/DebugLogger>` used to log messages from the library.

.. attribute:: GatewayVersion

	Version of the gateway used by the library.

.. attribute:: GatewayUrl

	URL of the gateway used by the library.

.. attribute:: Shards

	Recommended shard count for this bot.

.. attribute:: Me

	The user the bot is connected as (instance of :doc:`DiscordUser </reference/DiscordUser>`).

.. attribute:: PrivateChannels

	List of DM channels (instances of :doc:`DiscordDMChannel </reference/DiscordDMChannel>`).
	
.. attribute:: Guilds

	A dictionary of guilds (instances of :doc:`DiscordGuild </reference/DiscordGuild`) the bot is in.

Methods
---------

.. function:: Connect()
.. function:: Connect(tokenOverride, tokenType)

	.. info:: 
	
		This method is asynchronous. It needs to be awaited.
	
	.. info::
	
		If you did not specify a token or config when constructing the client, you will need to use the overload with 
		token overrides.
	
	Connects to Discord and begins dispatching events.
	
	:param tokenOverride: A string containing the token used to connect.
	:param tokenType: A :doc:`TokenType </reference/TokenType>` which defines the token's type.
	
.. function:: AddModule(module)

	Adds a module to the client, and returns it.
	
	:param module: An instance of a class implementing :doc:`IModule </reference/IModule>` interface.
	
.. function:: GetModule<T>(module)

	Finds and returns an instance of the module specified by the generic argument. ``T`` needs to be a class 
	implementing :doc:`IModule </reference/IModule>` interface.

.. function:: Reconnect()
.. function:: Reconnect(tokenOverride, tokenType, shard)

	.. info:: 
	
		This method is asynchronous. It needs to be awaited.
	
	.. info::
	
		If you did not specify a token or config when constructing the client, you will need to use the overload with 
		token overrides.

	Reconnects with Discord.
	
	:param tokenOverride: A string containing the token used to connect.
	:param tokenType: A :doc:`TokenType </reference/TokenType>` which defines the token's type.
	:param shard: Shard to connect.

.. function:: Disconnect()

	.. info:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Disconnects from Discord and stops dispatching events.
	
	