Reference for ``DiscordMessage``
================================

Represents a message sent in a message channel.

Members
-------

.. attribute:: ID

	This message's ID.

.. attribute:: ChannelID

	ID of the channel this message was sent in.

.. attribute:: Parent

	The :doc:`DiscordChannel </reference/DiscordChannel>` this message was sent in.

.. attribute:: Author

	The :doc:`DiscordUser </reference/DiscordUser>` that sent this message.

.. attribute:: Content

	This message's contents.

.. attribute:: TimestampRaw

	This message's raw timestamp, as sent by the Discord API.

.. attribute:: Timestamp

	This message's timestamp.

.. attribute:: EditedTimestampRaw

	This message's raw last edit timestamp, as sent by the Discord API.

.. attribute:: EditedTimestamp

	This message's last edit timestamp.

.. attribute:: TTS

	Whether this message is to be spoken aloud.

.. attribute:: MentionEveryone

	Whether this message mentions @everyone.

.. attribute:: Mentions

	List of :doc:`DiscordUser </reference/DiscordUser>` this message mentions, if any.

.. attribute:: MentionedRoles

	List of :doc:`DiscordRole </reference/DiscordRole>` this message mentions, if any.

.. attribute:: Attachments

	List of :doc:`DiscordAttachment </reference/entities/DiscordAttachment>` attached to this message.

.. attribute:: Embeds

	List of :doc:`DiscordEmbed </reference/entities/DiscordEmbed>` attached to this message.

.. attribute:: Reactions

	List of :doc:`DiscordReaction </reference/entities/DiscordReaction>` used to react to this message.

.. attribute:: Nonce

	This message's nonce.

.. attribute:: Pinned

	Whether or not this message is pinned.

.. attribute:: WebhookID

	ID of the :doc:`DiscordWebhook </reference/entities/DiscordWebhook>` used to send this message.

Methods
-------

.. function:: Edit(contents = null, embed = null)

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Edits the contents of this message.
	
	:param contents: New contents of the message.
	:param embed: New embed of the message.

.. function:: Delete()

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Deletes this message.

.. function:: Pin()

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Pins this message in its channel.

.. function:: Unpin()

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Unpins this message in its channel.

.. function:: Respond(content, tts, embed)

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Posts a reply to this message in the channel it came from.
	
	:param content: Contents of the message to send.
	:param tts: Whether this message is to be spoken using TTS. Optional, defaults to ``false``.
	:param embed: Embed to attach to this message. Optional, defaults to ``null``.

.. function:: CreateReaction(emoji)

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Reacts to this message using specified emoji.
	
	:param emoji: Emoji to react with. When using standard emoji, this needs to be the unicode entity (such as 👍 or 🤔). When using guild emoji, this needs to be a formatted emoji string (such as ``<:FeelsBadMan:229765516204441600>``).

.. function:: DeleteOwnReaction(emoji)

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Removes your reaction from this message.
	
	:param emoji: Reaction to remove. When using standard emoji, this needs to be the unicode entity (such as 👍 or 🤔). When using guild emoji, this needs to be a formatted emoji string (such as ``<:FeelsBadMan:229765516204441600>``).

.. function:: DeleteReaction(emoji, member)
.. function:: DeleteReaction(emoji, user_id)

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Removes another member's reaction from this message.
	
	:param emoji: Reaction to remove. When using standard emoji, this needs to be the unicode entity (such as 👍 or 🤔). When using guild emoji, this needs to be a formatted emoji string (such as ``<:FeelsBadMan:229765516204441600>``).
	:param member: Member whose reaction to remove.
	:param user_id: ID of a user whose reaction to remove.

.. function:: GetReactions(emoji)

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Gets all users who reacted with specified emoji. Returns a list of :doc:`DiscordUser </reference/DiscordUser>`.
	
	:param emoji: Reaction to check for. When using standard emoji, this needs to be the unicode entity (such as 👍 or 🤔). When using guild emoji, this needs to be a formatted emoji string (such as ``<:FeelsBadMan:229765516204441600>``).

.. function:: DeleteAllReactions()

	.. note:: 
	
		This method is asynchronous. It needs to be awaited.
	
	Deletes all reactions from this message.