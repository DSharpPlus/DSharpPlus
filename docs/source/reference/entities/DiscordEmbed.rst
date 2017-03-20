Reference for ``DiscordEmbed``
==============================

Represents an embed, attached to a message. These can be used to represent certain kinds of information, that cannot 
be otherwise displayed in regular messages.

.. note::

	Embeds are limited to a total of 4000 characters.

An embed looks more or less like this:

.. image:: https://cdn.discordapp.com/attachments/84319995256905728/252292324967710721/embed.png

Members
-------

.. attribute:: Title

	.. note::
	
		This property has a limit of 256 characters of length.

	This embed's title

.. attribute:: Type

	.. note::
	
		When sending an embed, this property is always set to ``rich``, regardless of what it's actual set to.

	This embed's type.

.. attribute:: Description

	.. note::
	
		This property has a limit of 2048 characters of length.

	This embed's description.

.. attribute:: Url

	This embed's URL.

.. attribute:: Timestamp

	This embed's timestamp.

.. attribute:: Color

	.. note::
	
		C# supports hexadecimal integer notation (i.e. ``0x0123ABCD``). You can write colors down as ``0xRRGGBB``.

	This embed's color.

.. attribute:: Footer

	This embed's footer. Instance of :doc:`DiscordEmbedFooter </reference/entities/DiscordEmbedFooter>`.

.. attribute:: Image

	This embed's image. Instance of :doc:`DiscordEmbedImage </reference/entities/DiscordEmbedImage>`.

.. attribute:: Thumbnail

	This embed's thumbnail. Instance of :doc:`DiscordEmbedThumbnail </reference/entities/DiscordEmbedThumbnail>`.

.. attribute:: Video

	.. note::
	
		This property cannot be set by clients, and, as such, is ignored by Discord.

	This embed's video. Instance of :doc:`DiscordEmbedVideo </reference/entities/DiscordEmbedVideo>`.

.. attribute:: Provider

	This embed's provider.

.. attribute:: Author

	This embed's author. Instance of :doc:`DiscordEmbedAuthor </reference/entities/DiscordEmbedAuthor>`.

.. attribute:: Fields

	This embed's fields. List of :doc:`DiscordEmbedField </reference/entities/DiscordEmbedField>` instances.