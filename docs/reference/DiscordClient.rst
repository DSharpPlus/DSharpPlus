Reference for ``DiscordClient``
=================================

Events
--------

Events are the key to making any bot work. All events are asynchronous, meaning that all event handlers must return a 
``Task`` instance. For lambda and function handlers marked ``async`` this is automatic. For non-``async`` lambdas and 
functions, you need to ``return Task.Delay(0)`` at the end of the handler, or make it ``async``, and begin with 
``await Task.Yield()``. If an event takes no argument, your handler cannot take any either, otherwise it takes one 
argument, which is an appropriate ``EventArgs`` instance.

Below you can find complete event reference.

.. function:: SocketOpened

	Called when the WebSocket connection is established. Takes no arguments.

.. function:: SocketClosed

	Called when the WebSocket connection is closed. Takes no arguments.

.. function:: Ready

	Called when the client enters ready state. Takes no arguments.

.. function:: ChannelCreated

	Called when a new channel is created. Takes ``ChannelCreateEventArgs`` as an argument, with following arguments:
	
	:param Channel: The channel (instance of :doc:`DiscordChannel </reference/DiscordChannel>`) that was just created.
	:param Guild: The guild (instance of :doc:`DiscordGuild </reference/DiscordGuild>`) that the channel was created 
	in.