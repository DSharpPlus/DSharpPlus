Reference for ``DiscordEmoji``
==============================

This class represents an emoji in Discord. It can be used to represent both unicode and custom emotes.

Members
-------

.. attribute:: Id

	ID of the emoji. This is set to ``0`` for unicode emojis.

.. attribute:: Name

	The emoji's name. For unicode emojis, this contains the unicode version of the emoji.

.. attribute:: Roles

	List of roles (instances of :doc:`DiscordRole </reference/DiscordRole>`) allowed to use a given emoji. This is a legacy property.

.. attribute:: RequireColons

	Whether or not the emoji requires colons to use. This is a legacy property.
	
	A colon-requiring emoji needs to be used as ``:name:`` by users, whereas one that doesn't require can be used as ``name``.

.. attribute:: Managed

	Whether or not the emoji comes from an integration.

Methods
-------

.. function:: ToString()

	Returns a string representation of a given emoji. This is ``<:name:id>`` for custom emojis, and ``name`` for unicode ones.

.. function:: FromUnicode(client, unicode_entity)

	.. note::
	
		This method is static.

	Creates a ``DiscordEmoji`` object from a unicode_entity.
	
	:param client: :doc:`DiscordClient </reference/DiscordClient>` to attach the emote to.
	:param unicode_entity: Unicode emoji to create the object from.

.. function:: FromGuildEmote(client, id)

	.. note::
	
		This method is static.
	
	Returns a custom ``DiscordEmoji`` with given ID.
	
	:param client: :doc:`DiscordClient </reference/DiscordClient>` to attach the emote to.
	:param id: ID of the emote to return.

.. function:: FromName(client, name)

	.. note::
	
		This method is static.
	
	Returns a ``DiscordEmoji`` of any kind from given emoji name.
	
	:param client: :doc:`DiscordClient </reference/DiscordClient>` to attach the emote to.
	:param name: Name of the emoji, including colons. Examples: ``:thinking:``, ``:ok_hand::skin-tone-2:``, ``:D``, ``:FeelsBadMan:``.