Reference for ``VoiceNextClient``
=================================

Creates a voice client, which can be used to establish individual voice connections (as instances of `VoiceNextConnection </reference/voice/VoiceNextConnection>`).

Members
-------

.. attribute:: Client

	Instance of :doc:`DiscordClient </reference/DiscordClient>` used by this voice client.

Methods
-------

.. function:: ConnectAsync(channel)

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Connects to specified voice channel. Returns an instance of :doc:`VoiceNextConnection </reference/voice/VoiceNextConnection>`.
	
	:param channel: An instance of :doc:`DiscordChannel </reference/DiscordChannel>` to connect to. This must be a voice channel.

.. function:: GetConnection(guild)

	Retrieves a voice connection for specified guild.
	
	:param guild: An instance of :doc:`DiscordGuild </reference/DiscordGuild>` for which to retrieve the connection for. Returns an instance of :doc:`VoiceNextConnection </reference/voice/VoiceNextConnection>` or ``null`` if specified guild has no active voice connection.