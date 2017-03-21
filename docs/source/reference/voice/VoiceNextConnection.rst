Reference for ``VoiceNextConnection``
=====================================

Represents an individual, two-way voice connection to Discord.

Events
------

.. attribute:: UserSpeaking

	Called when user speaks in a connected voice channel. Takes ``UserSpeakingEventArgs`` as an argument, with 
	following parameters:
	
	:param UserID: ID of the user who triggered the event.
	:param SSRC: SSRC of the user who triggered the event.
	:param Speaking: Whether the user who triggered the event is speaking.

.. attribute:: VoiceReceived

	Called when voice data is received. Takes ``VoiceReceivedEventArgs`` as an argument, with following parameters:
	
	:param SSRC: SSRC of the voice source.
	:param UserID: ID of the user whose data is being received.
	:param Voice: Received voice data.
	:param VoiceLength: Length of the data received.

Methods
-------

.. function:: SendAsync(pcm_data, pcm_block_size, bitrate)

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	.. note::
	
		Input data needs to be 48kHz, 2-channel PCM data. It is recommended it's 16-bit, however other bitrates are also acceptable.
	
	Encodes, encrypts, and sends given PCM data to Discord voice server.
	
	:param pcm_data: PCM data to send.
	:param pcm_block_size: Millisecond size of the sample. Usually 20ms.
	:param bitrate: Bitrate of given sample. Optional, defaults to 16.

.. function:: SendSpeakingAsync(speaking)

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Sends a speaking status for this voice connection.
	
	:param speaking: Whether the current user should appear as speaking. Optional, defaults to true.

.. function:: Disconnect()

	Disposes and disconnects this connection.

.. function:: Dispose()

	Disposes and disconnects this connection.