Reference for ``DiscordEmbedAuthor``
====================================

Represents an author of a :doc:`DiscordEmbed </refernece/entities/DiscordEmbed>`.

Members
-------

.. attribute:: Name
	
	This author's name.

.. attribute:: Url

	This author's url.

.. attribute:: IconUrl

	.. note::
	
		Allowed uri schemas are ``http``, ``https``, and ``attachment``.
	
	This author's icon url.

.. attribute:: ProxyIconUrl

	.. note::
	
		This property cannot be set by clients, and, as such, is ignored by Discord.
	
	The author's icon uri, proxied by Discord.