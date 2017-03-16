Reference for ``DiscordClient``
=================================

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

.. function:: SocketOpened

	Called when the WebSocket connection is established. Takes no arguments.

.. function:: SocketClosed

	Called when the WebSocket connection is closed. Takes no arguments.

.. function:: Ready

	Called when the client enters ready state. Takes no arguments.

.. function:: ChannelCreated

	Called when a new channel is created. Takes ``ChannelCreateEventArgs`` as an argument, with following parameters:
	
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just created.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the channel was created 
	in.

.. function DMChannelCreated

	Called when a new DM channel is created. Takes ``DMChannelCreateEventArgs`` as an argument, with following 
	parameters:
	
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just created.

.. function:: ChannelUpdated

	Called when an existing channel is updated. Takes ``ChannelUpdateEventArgs`` as an argument, with following 
	parameters:
	
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just updated.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the channel was updated 
	in.

.. function:: ChannelDeleted

	Called when an existing channel is deleted. Takes ``ChannelDeleteEventArgs`` as an argument, with following 
	parameters:
	
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just deleted.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the channel was deleted 
	in.

.. function:: DMChannelDeleted

	Called when an existing DM channel is deleted. Takes ``DMChannelDeleteEventArgs`` as an argument, with following 
	parameters:
	
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just deleted.

.. function:: GuildCreated

	Called when a new guild is created. Takes ``GuildCreateEventArgs`` as an argument, with following parameters:
	
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that was just created.

.. function:: GuildAvailable

	Called when a guild becomes available. Takes ``GuildCreateEventArgs`` as an argument, with following parameters:
	
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that has just become available.

.. function:: GuildUpdated

	Called when a guild is updated. Takes ``GuildUpdateEventArgs`` as an argument, with following parameters:
	
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that was just updated.

.. function:: GuildDeleted

	Called when a guild is deleted. Takes ``GuildDeleteEventArgs`` as an argument, with following parameters:
	
	:param ID: ID of the guild that was just deleted.
	:param Unavailable: Whether the guild is unavailable or not.

.. function:: GuildUnavailable

	Called when a guild becomes unavailable. Takes ``GuildDeleteEventArgs`` as an argument, with following parameters:
	
	:param ID: ID of the guild that has just become unavailable.
	:param Unavailable: Whether the guild is unavailable or not.

.. function:: MessageCreated

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

.. function:: PresenceUpdate

	Called when a presence update occurs. Takes ``PresenceUpdateEventArgs`` as an argument, with following parameters:
	
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) whose presence was updated.
	:param UserID: The ID of the user whose presence was updated.
	:param Game: Game the user is playing or streaming.
	:param Status: User's status (online, idle, do not disturb, or offline).
	:param GuildID: ID of the guild the presence update occured in.
	:param RoleIDs: IDs of user's roles in the given guild.

.. function:: GuildBanAdd

	Called whenever a user gets banned from a guild. Takes ``GuildBanAddEventArgs`` as an argument, with following 
	parameters:
	
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that got banned.
	:param GuildID: ID of the guild the user got banned from.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the user got banned from.

.. function:: GuildBanRemove

	Called whenever a user gets unbanned from a guild. Takes ``GuildBanRemoveEventArgs`` as an argument, with 
	following parameters:
	
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that got unbanned.
	:param GuildID: ID of the guild the user got unbanned from.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the user got unbanned from.

.. function:: GuildEmojisUpdate

	Called whenever a guild has its emoji updated. Takes ``GuildEmojisUpdateEventArgs`` as an argument, with the 
	following parameters:
	
	:param Emojis: A list of :doc:`DiscordEmoji </reference/DiscordEmoji>`s that got updated.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that had its emoji updated.

.. function:: GuildIntegrationsUpdate

	Called whenever a guild has its integrations updated. Takes ``GuildIntegrationsUpdateEventArgs`` as an argument, 
	with following parameters:
	
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that had its integrations 
	updated.

.. function:: GuildMemberAdd

	Called whenever a member joins a guild. Takes ``GuildMemberAddEventArgs`` as an argument, with following 
	parameters:
	
	:param Member: The member (instance of :doc:`DiscordMember </reference/DiscordMember>`)
	:param GuildID: ID of the guild the memeber joined.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the member joined.

.. function:: GuildMemberRemove

	Called whenever a member leaves a guild. Takes ``GuildMemberRemoveEventArgs`` as an argument, with following 
	parameters:
	
	:param GuildID: ID of the guild that the member left.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the member left.
	:param User: The member (instance of :doc:`DiscordUser </reference/DiscordUser>`) that left the guild.

.. function:: GuildMemberUpdate

	Called whenever a guild member is updated. Takes ``GuildMemberUpdateEventArgs`` as an argument, with following 
	parameters:
	
	:param GuildID: ID of the guild in which the update occured.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) in which the update occured.
	:param Roles: A list of role IDs for the member.
	:param Nickname: New nickname of the member.
	:param User: The user (instance of :doc:`DiscordUser </reference/DiscordUser>`) that got updated.

.. function:: GuildRoleCreate

	Called whenever a role is created in a guild. Takes ``GuildRoleCreateEventArgs`` as an argument, with following 
	parameters:
	
	:param GuildID: ID of the guild the role was created in.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the role was created in.
	:param Role: The role (instance of :doc:`DiscordRole </reference/DiscordRole>`) that was created.

.. function:: GuildRoleUpdate

	Called whenever a role is updated in a guild. Takes ``GuildRoleUpdateEventArgs`` as an argument, with following 
	parameters:
	
	:param GuildID: ID of the guild the role was updated in.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the role was updated in.
	:param Role: The role (instance of :doc:`DiscordRole </reference/DiscordRole>`) that was updated.

.. function:: GuildRoleDelete

	Called whenever a role is deleted in a guild. Takes ``GuildRoleDeleteEventArgs`` as an argument, with following 
	parameters:
	
	:param GuildID: ID of the guild the role was deleted in.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) the role was deleted in.
	:param Role: The role (instance of :doc:`DiscordRole </reference/DiscordRole>`) that was deleted.

.. function:: MessageUpdate

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

