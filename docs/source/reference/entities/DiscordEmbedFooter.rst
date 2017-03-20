Reference for ``DiscordEmbedFooter``
====================================

Represents a footer inside a :doc:`DiscordEmbed </refernece/entities/DiscordEmbed>`.

Members
-------

.. attribute:: Text

	.. note::
	
		This property has a limit of 2048 characters of length.
	
	This embed footer's text.

.. attribute:: IconUrl

	.. note::
	
		Allowed uri schemas are ``http``, ``https``, and ``attachment``.
	
	This embed footer's icon url.

.. attribute:: ProxyIconUrl

	.. note::
	
		This property cannot be set by clients, and, as such, is ignored by Discord.
	
	The embed's footer icon uri, proxied by Discord.