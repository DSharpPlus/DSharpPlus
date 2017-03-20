Reference for ``DiscordDMChannel``
==================================

Represents a Discord DM channel. This class represents direct channels only. 

This class inherits from :doc:`DiscordChannel </reference/DiscordChannel>`, and thus, it shares its methods and 
properties, and you can pass it around as such.

Members
-------

.. attribute:: ID

	This channel's ID.

.. attribute:: Recipient

	This DM channel's recipient. Instance of :doc:`DiscordUser </reference/DiscordUser>`.

.. attribute:: GuildID

	.. note::
	
		As this class represents non-guild channels, this value is always ``0``.

	ID of the guild this channel belons to.

.. attribute:: Name

	.. note::
	
		As this class represents non-guild channels, this value is always ``null``.

	This channel's name.

.. attribute:: Type

	This channel's type. Used to distinguish voice and text channels. Instance of :doc:`ChannelType </reference/misc/ChannelType>`.

.. attribute:: Position

	.. note::
	
		As this class represents non-guild channels, this value is always ``0``.

	This channel's position on the channel list.

.. attribute:: IsPrivate

	.. note::
	
		As this class represents private channels, this value is always ``true``.

	Used to determine whether the channel is private or not.

.. attribute:: Parent

	.. note::
	
		As this class represents non-guild channels, this value is always ``null``.

	The guild this channel belongs to. ``null`` if this is not a guild channel (e.g. DM channel).

.. attribute:: PermissionOverwrites

	.. note::
	
		As this class represents non-guild channels, this value is always ``null``.

	This channel's permission overwrites. List of :doc:`DiscordOverwrite </reference/entities/DiscordOverwrite>` instances.

.. attribute:: Topic

	.. note::
	
		As this class represents non-guild channels, this value is always ``null``.

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
	
		As this class represents non-guild channels, this value is always ``null``.
	
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
	
		As this class represents non-guild channels, this method does not apply to this class.
	
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
	
		As this class represents non-guild channels, this method does not apply to this class.
	
	Gets and returns a list of ``DiscordInvite </reference/entities/DiscordInvite>` for this channel.

.. function:: DeleteChannelPermission(overwrite_id)

	.. note::
	
		This method is asynchronous. It needs to be awaited.

	.. note::
	
		As this class represents non-guild channels, this method does not apply to this class.
	
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
	
		As this class represents non-guild channels, this method does not apply to this class.
	
	Gets and returns this channel's webhooks as a list of :doc:`DiscordWebhook </reference/entities/DiscordWebhook>` instances.

.. function:: PlaceMember(member_id)

	.. note::
	
		This method is asynchronous. It needs to be awaited.

	.. note::
	
		As this class represents non-guild channels, this method does not apply to this class.
	
	Moves a specified member to this voice channel.
	
	:param member_id: ID of the guild member to move.

.. function:: UpdateOverwrite(overwrite)

	.. note::
	
		This method is asynchronous. It needs to be awaited.

	.. note::
	
		As this class represents non-guild channels, this method does not apply to this class.
	
	Updates a permission overwrite for this channel.
	
	:param overwrite: An instance of :doc:`PermissionOverwrite </reference/entities/DiscordOverwrite>` to update the channel with.

.. function:: AddDMRecipient(user_id, access_token)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	.. note::
	
		This method cannot be used by bots. It requires an OAuth token.
	
	Adds a user to a group DM.
	
	:param user_id: ID of the user to add.
	:param access_token: OAuth token with ``gdm.join`` scope.

.. function:: RemoveDMRecipient(user_id, access_token)

	.. note::
	
		This method is asynchronous. It needs to be awaited.
	
	.. note::
	
		This method cannot be used by bots. It requires an OAuth token.
	
	Removes a user from a group DM.
	
	:param user_id: ID of the user to remove.
	:param access_token: OAuth token with ``gdm.join`` scope.