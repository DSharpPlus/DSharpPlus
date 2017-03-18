Reference for ``DiscordEmbed``
==============================

Represents an embed, attached to a message. These can be used to represent certain kinds of information, that cannot 
be otherwise displayed in regular messages.

.. note::

	Embeds are limited to a total of 4000 characters.

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

`string Url`: Embed URL

`DateTime Timestamp`: Embed Timestamp

`int Color`: Embed Color

`DiscordEmbedFooter Footer`: Embed Footer

`DiscordEmbedImage Image`: Embed Image

`DiscordEmbedThumbnail Thumbnail`: Embed Thumbnail

`DiscordEmbedVideo Video`: Embed Video

`DiscordEmbedProvider Provider`: Embed Provider

`DiscordEmbedAuthor Author`: Embed Author

`List<DiscordEmbedField> Fields`: Embed Fields

