Reference for ``DiscordChannel``
================================

Represents a Discord channel. This class encompasses direct and guild channels, both text and voice.

Members
-------

.. attribute:: ID

	This channel's ID.

.. attribute:: GuildID

	ID of the guild this channel belons to.

.. attribute:: Name

	This channel's name.

.. attribute:: Type

	This channel's type. Used to distinguish voice and text channels. Instance of :doc:`ChannelType </reference/misc/ChannelType>`.

.. attribute:: Position

	This channel's position on the channel list.

.. attribute:: IsPrivate

	Used to determine whether the channel is private or not.

.. attribute:: Parent

	The guild this channel belongs to. ``null`` if this is not a guild channel (e.g. DM channel).

.. attribute:: PermissionOverwrites

	This channel's permission overwrites. List of :doc:`DiscordOverwrite </reference/entities/DiscordOverwrite>` instances.

.. attribute:: Topic

	.. note::
	
		This is applicable to guild text channels only.

	This channel's topic.

.. attribute:: LastMessageID

	ID of last message created in this channel.

.. attribute:: Bitrate

	.. note::
	
		This is applicable to voice channels only.

	This channel's voice bitrate.

.. attribute:: UserLimit

	.. note::
	
		This is applicable to voice channels only.
	
.. attribute:: Mention

	.. note::
	
		This is applicable to guild text channels only.
	
	This channel's mention.

Methods
-------

.. function:: SendMessage(content, tts, embed)

	.. note::
	
		This method is asynchronous. It needs to be awaited.

	.. note::
	
		This is applicable to text channels only.
	
	Sends a message to this channel. Returns the sent message as an instance of :doc:`DiscordMessage </reference/DiscordMessage>`.
	
	:param content: Message's contents.
	:param tts: Whether or not the message contents are to be spoken. Optional, defaults to ``false``.
	:param embed: An instance of :doc:`DiscordEmbed </reference/entities/DiscordEmbed>` to attach to this message.

.. function:: SendFile(filepath, filename, content, tts)

	.. note::
	
		This method is asynchronous. It needs to be awaited.

	.. note::
	
		This is applicable to text channels only. Returns the sent message.
	
	Sends a file to specified channel. Returns the sent message as an instance of :doc:`DiscordMessage </reference/DiscordMessage>`.
	
	:param filepath: Path to the file to send.
	:param filename: Name of the file to send. This is used by discord to display the file name.
	:param content: Message contents to send with the file. Optional, defaults to emtpy string.
	:param tts: Whether or not the message contents are to be spoken. Optional, defaults to ``false``.

.. function:: Delete()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	Deletes this channel.

.. function:: GetMessage(id)

	.. note::
	
		This method is asynchronous. It needs to be awaited.

	.. note::
	
		This is applicable to text channels only.
	
	Gets a message by its ID from this channel. Returns the message as an instance of :doc:`DiscordMessage </reference/DiscordMessage>`.
	
	:param id: ID of the message to get.

.. function:: ModifyPosition(position)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	.. note::
	
		This is applicable to guild channels only.
	
	Changes this channel's position in the guild's channel list.
	
	:param position: New position of this channel.

.. function:: GetMessages(around_id, before_id, after_id, limit)

	.. note::
	
		This method is asynchronous. It needs to be awaited.

	.. note::
	
		This method is applicable to text channels only.
	
	.. warning::
	
		``around_id``, ``before_id``, and ``after_id`` parameters are mutually exclusive. If more than one of these is specified, the request will fail!
	
	Gets messages from this channel. Returns a list of :doc:`DiscordMessage </reference/DiscordMessage>` instances.
	
	:param around_id: Pivot message ID around which to download messages. Optional, defaults to ``0``.
	:param before_id: Pivot message ID from before which to download messages. Optional, defaults to ``0``.
	:param after_id: Pivot message ID after which to download messages. Optional, defaults to ``0``.
	:param limit: Maximum number of messages to download. This number cannot exceed 100. Optional, defaults to ``50``.

.. function:: BulkDeleteMessages(message_ids)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	.. note::
	
		This function is applicable to text channels only.
	
	.. warning::
	
		This method cannot be used to delete messages older than 2 weeks. If any specified message ID is older than 2 weeks, the request will fail!
	
	Bulk deletes messages from this channel.
	
	:param message_ids: IDs of messages to delete.

.. function:: GetInvites()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	.. note::
	
		This method is applicable to text channels only.
	
	Gets and returns a list of ``DiscordInvite </reference/entities/DiscordInvite>` for this channel.

.. function:: DeleteChannelPermission(overwrite_id)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	.. note::
	
		This method is applicable to guild channels only.
	
	Deletes a specified set of permission overwrites.
	
	:param overwrite_id: Permission overwrite to delete.

.. function:: TriggerTyping()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	.. note::
	
		This method is applicable to text channels only.
	
	Sends a typing indicator to this channel. This lasts for 10 seconds.

.. function:: GetPinnedMessages()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	.. note::
	
		This method is applicable to text channels only.
	
	Gets and returns this channel's pinned messages as a list of :doc:`DiscordMessage </reference/DiscordMessage>` instances.

.. function:: GetWebhooks()

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	.. note::
	
		This method is applicable to guild text channels only.
	
	Gets and returns this channel's webhooks as a list of :doc:`DiscordWebhook </reference/entities/DiscordWebhook>` instances.

.. function:: PlaceMember(member_id)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	.. note::
	
		This method is applicable to guild voice channels only.
	
	Moves a specified member to this voice channel.
	
	:param member_id: ID of the guild member to move.

.. function:: UpdateOverwrite(overwrite)

	.. note::
	
		This method is asynchronous. It needs to be awaited.

	.. note::
	
		This method is applicable to guild channels only.
	
	Updates a permission overwrite for this channel.
	
	:param overwrite: An instance of :doc:`PermissionOverwrite </reference/entities/DiscordOverwrite>` to update the channel with.