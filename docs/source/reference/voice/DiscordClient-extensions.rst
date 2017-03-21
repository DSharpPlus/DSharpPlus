VoiceNext extensions for ``DiscordClient``
==========================================

VoiceNext extension defines several extension methods to :doc:`DiscordClient </reference/DiscordClient>`, which allow 
for interactions with both Discord Voice, and VoiceNext APIs.

Methods
-------

.. function:: UseVoiceNext()
.. function:: UseVoiceNext(config)

	.. note::
	
		It is recommended to use the overload which allows to specify a configuration, to allow for better tuning of 
		voice encoding.
	
	Creates a new :doc:`VoiceNextClient </reference/voice/VoiceNextClient>` with specified (or default) configuration.
	
	:param config: An instance of ``VoiceNextConfiguration``. If not provided, a default one is used, which optimizes encoding for music.

.. function:: GetVoiceNextClient()

	Returns the existing :doc:`VoiceNextClient </reference/voice/VoiceNextClient>` instance, if one was created. Returns ``null`` otherwise.