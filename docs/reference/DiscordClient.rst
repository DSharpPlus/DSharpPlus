Reference for ``DiscordClient``
=================================

Events
--------

Events are the key to making any bot work. All events are asynchronous, meaning that all event handlers must return a 
``Task`` instance. For lambda and function handlers marked ``async`` this is automatic. For non-``async`` lambdas and 
functions, you need to ``return Task.Delay(0)`` at the end of the handler, or make it ``async``, and begin with 
``await Task.Yield()``. If an event takes no argument, your handler cannot take any either, otherwise it takes one 
argument, which is an appropriate ``EventArgs`` instance.

Events can be used in 2 ways. Via lambdas or functions. In C#, the handler needs to return ``Task``, and take 
appropriate arguments.

For events without arguments, the following methods are acceptable: ::

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

	Called when a new DM channel is created. Takes ``ChannelCreate