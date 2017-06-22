Reference for ``DiscordMessage``
================================

Represents a message sent in a message channel.

Members
-------

.. attribute:: Id

	This message's ID.

.. attribute:: Channel

	Channel this message was sent in.

.. attribute:: Author

	The :doc:`DiscordUser </reference/DiscordUser>` that sent this message.

.. attribute:: Content

	This message's contents.

.. attribute:: Timestamp

	This message's timestamp.

.. attribute:: EditedTimestamp

	This message's last edit timestamp.

.. attribute:: IsTTS

	Whether this message is to be spoken aloud.

.. attribute:: MentionEveryone

	Whether this message mentions @everyone.

.. attribute:: MentionedUsers

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

.. attribute:: WebhookId

	ID of the :doc:`DiscordWebhook </reference/entities/DiscordWebhook>` used to send this message.

Methods
-------

.. function:: EditAsync(contents, embed)
	
	Edits the contents of this message.
	
	:param contents: New contents of the message. Optional, defaults to ``null``.
	:param embed: New embed of the message. Optional, defaults to ``null``.

.. function:: DeleteAsync(reason)
	
	Deletes this message.
	
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: PinAsync()
	
	Pins this message in its channel.

.. function:: UnpinAsync()
	
	Unpins this message in its channel.

.. function:: RespondAsync(content, tts, embed)
              RespondAsync(file_data, file_name, content, tts, embed)
			  RespondAsync(files, content, tts, embed)
	
	.. note::
	
		This method will not rewind the data streams before sending. Make sure the streams' positions are correct before you pass them.
	
	Posts a reply to this message in the channel it came from.
	
	:param content: Contents of the message to send. For file overloads, this parameter is optional and defaults to ``null``.
	:param file_data: Stream containing the data to send.
	:param file_name: Name of the file to send. This is used by discord to display the file name.
	:param files: A ``Dictionary<string, Stream>``, where file names are the keys, and data streams are the keys.
	:param tts: Whether this message is to be spoken using TTS. Optional, defaults to ``false``.
	:param embed: Embed to attach to this message. Optional, defaults to ``null``.

.. function:: CreateReactionAsync(emoji)
	
	Reacts to this message using specified emoji.
	
	:param emoji: Emoji (instance of :doc:`DiscordEmoji </reference/DiscordEmoji>`) to react with.

.. function:: DeleteOwnReactionAsync(emoji)
	
	Removes your reaction from this message.
	
	:param emoji: Emoji (instance of :doc:`DiscordEmoji </reference/DiscordEmoji>`) to remove reaction for.

.. function:: DeleteReactionAsync(emoji, user, reason)
	
	Removes another member's reaction from this message.
	
	:param emoji: Emoji (instance of :doc:`DiscordEmoji </reference/DiscordEmoji>`) to remove reaction for.
	:param member: Member whose reaction to remove.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: GetReactionsAsync(emoji)
	
	Gets all users who reacted with specified emoji. Returns a list of :doc:`DiscordUser </reference/DiscordUser>`.
	
	:param emoji: Emoji (instance of :doc:`DiscordEmoji </reference/DiscordEmoji>`) to check for.

.. function:: DeleteAllReactionsAsync(reason)
	
	Deletes all reactions from this message.
	
	:param reason: Reason for audit logs. Optional, defaults to ``null``.