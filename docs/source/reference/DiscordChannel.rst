Reference for ``DiscordChannel``
================================

Represents a Discord channel. This class encompasses direct and guild channels, both text and voice.

Members
-------

.. attribute:: Id

	This channel's ID.

.. attribute:: Guild

	Guild this channel belongs to.

.. attribute:: Name

	This channel's name.

.. attribute:: Type

	This channel's type. Used to distinguish voice and text channels. Instance of :doc:`ChannelType </reference/misc/ChannelType>`.

.. attribute:: Position

	This channel's position on the channel list.

.. attribute:: IsPrivate

	Used to determine whether the channel is private or not.

.. attribute:: PermissionOverwrites

	This channel's permission overwrites. List of :doc:`DiscordOverwrite </reference/entities/DiscordOverwrite>` instances.

.. attribute:: Topic

	.. note::
	
		This is applicable to guild text channels only.

	This channel's topic.

.. attribute:: LastMessageId

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

.. attribute:: IsNSFW

	.. note::
	
		This is applicable to guild text channels only.

	Whether or not the channel is considered NSFW by Discord.

Methods
-------

.. function:: SendMessageAsync(content, tts, embed)

	.. note::
	
		This is applicable to text channels only.
	
	Sends a message to this channel. Returns the sent message as an instance of :doc:`DiscordMessage </reference/DiscordMessage>`.
	
	:param content: Message's contents.
	:param tts: Whether or not the message contents are to be spoken. Optional, defaults to ``false``.
	:param embed: An instance of :doc:`DiscordEmbed </reference/entities/DiscordEmbed>` to attach to this message. Optional, defaults to ``null``.

.. function:: SendFileAsync(file_data, file_name, content, tts, embed)

	.. note::
	
		This is applicable to text channels only. Returns the sent message.
	
	.. note::
	
		This method will not rewind the data stream before sending. Make sure the stream's position is correct before you pass it.
	
	Sends a file to specified channel. Returns the sent message as an instance of :doc:`DiscordMessage </reference/DiscordMessage>`.
	
	:param file_data: Stream containing the data to send.
	:param file_name: Name of the file to send. This is used by discord to display the file name.
	:param content: Message contents to send with the file. Optional, defaults to emtpy string.
	:param tts: Whether or not the message contents are to be spoken. Optional, defaults to ``false``.
	:param embed: An instance of :doc:`DiscordEmbed </reference/entities/DiscordEmbed>` to attach to this message. Optional, defaults to ``null``.

.. function:: SendMultipleFilesAsync(files, content, tts, embed)

	.. note::
	
		This is applicable to text channels only. Returns the sent message.
	
	.. note::
	
		This method will not rewind the data streams before sending. Make sure the streams' positions are correct before you pass them.
	
	Sends several files to specified channel. Returns the sent message as an instance of :doc:`DiscordMessage </reference/DiscordMessage>`.
	
	:param files: A ``Dictionary<string, Stream>``, where file names are the keys, and data streams are the keys.
	:param content: Message contents to send with the file. Optional, defaults to emtpy string.
	:param tts: Whether or not the message contents are to be spoken. Optional, defaults to ``false``.
	:param embed: An instance of :doc:`DiscordEmbed </reference/entities/DiscordEmbed>` to attach to this message. Optional, defaults to ``null``.

.. function:: DeleteAsync(reason)
	
	Deletes this channel.
	
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: GetMessageAsync(id)

	.. note::
	
		This is applicable to text channels only.
	
	Gets a message by its ID from this channel. Returns the message as an instance of :doc:`DiscordMessage </reference/DiscordMessage>`.
	
	If message cache is enabled, it will be searched first.
	
	:param id: ID of the message to get.

.. function:: ModifyPositionAsync(position, reason)
	
	.. note::
	
		This is applicable to guild channels only.
	
	Changes this channel's position in the guild's channel list.
	
	:param position: New position of this channel.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: GetMessagesAsync(limit, before, after, around)

	.. note::
	
		This method is applicable to text channels only.
	
	.. warning::
	
		``around``, ``before``, and ``after`` parameters are mutually exclusive. If more than one of these is specified, the request will fail!
	
	Gets messages from this channel. Returns a list of :doc:`DiscordMessage </reference/DiscordMessage>` instances.
	
	:param limit: Maximum number of messages to download. This number cannot exceed 100. Optional, defaults to ``100``.
	:param before: Pivot message ID from before which to download messages. Optional, defaults to ``null``.
	:param around: Pivot message ID around which to download messages. Optional, defaults to ``null``.
	:param after: Pivot message ID after which to download messages. Optional, defaults to ``null``.

.. function:: DeleteMessagesAsync(messages, reason)
	
	.. note::
	
		This function is applicable to text channels only.
	
	.. warning::
	
		This method cannot be used to delete messages older than 2 weeks. If any specified message ID is older than 2 weeks, the request will fail!
	
	Bulk deletes messages from this channel.
	
	:param messages: Enumerable of :doc:`DiscordMessage </reference/DiscordMessage>` instances to delete.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: DeleteMessageAsync(message, reason)
	
	.. note::
	
		This function is applicable to text channels only.
	
	Deletes a message from this channel.
	
	:param message: Instance of :doc:`DiscordMessage </reference/DiscordMessage>` to delete.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: GetInvitesAsync()
	
	.. note::
	
		This method is applicable to guild channels only.
	
	Gets and returns a list of ``DiscordInvite </reference/entities/DiscordInvite>` for this channel.

.. function:: CreateInviteAsync(max_age, max_uses, temporary, unique, reason)

	.. note::
	
		This method is applicable to guild channels only.
	
	Creates a new invite to this channel.
	
	:param max_age: Time after which the invite expires in seconds. Optional, defaults to ``86400`` (24h).
	:param max_uses: Maximum number of uses for the invite. Specify ``0`` for unlimited. Optional, defaults to ``0``.
	:param temporary: Whether the invite grants temporary membership. This kind of membership removes the user from the guild after they go offline, unless you assign them a role. Optional, defaults to ``false``.
	:param unique: Whether or not to reuse existing invites with existing parameters. Specifying ``true`` will create a new invite rather than reusing an existing one. Optional, defaults to ``false``.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: DeleteOverwriteAsync(overwrite, reason)
	
	.. note::
	
		This method is applicable to guild channels only.
	
	Deletes a specified set of permission overwrites for this channel.
	
	:param overwrite: An instance of :doc:`PermissionOverwrite </reference/entities/DiscordOverwrite>` to delete.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: UpdateOverwriteAsync(overwrite, reason)

	.. note::
	
		This method is applicable to guild channels only.
	
	Updates a permission overwrite for this channel.
	
	:param overwrite: An instance of :doc:`PermissionOverwrite </reference/entities/DiscordOverwrite>` to update the channel with.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: AddOverwriteAsync(member, allow, deny, reason)
              AddOverwriteAsync(role, allow, deny, reason)

	Creates a permissions overwrite in this channel.
	
	:param member: Member to which the overwrite applies.
	:param role: Role to which the overwrite applies.
	:param allow: Instance of :doc:`Permissions </reference/misc/Permissions>` enum specifying which permissions are explicitly allowed for the user.
	:param deny: Instance of :doc:`Permissions </reference/misc/Permissions>` enum specifying which permissions are explicitly denied for the user.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: TriggerTypingAsync()
	
	.. note::
	
		This method is applicable to text channels only.
	
	Sends a typing indicator to this channel. This lasts for 10 seconds.

.. function:: GetPinnedMessagesAsync()
	
	.. note::
	
		This method is applicable to text channels only.
	
	Gets and returns this channel's pinned messages as a list of :doc:`DiscordMessage </reference/DiscordMessage>` instances.

.. function:: CreateWebhookAsync(name, avatar, avatar_format, reason)

	.. note::
	
		This method is applicable to guild text channels only.
	
	Creates and returns a new :doc:`DiscordWebhook </reference/DiscordWebhook>`.
	
	:param name: Name of the webhook.
	:param avatar: Stream containing avatar data for the webhook. Must be valid PNG, JPG, or GIF image. Optional, defaults to ``null``. If this is specified, ``avatar_format`` must also be specified.
	:param avatar_format: Instance of :doc:`ImageFormat </reference/misc/ImageFormat>` specifying the format of attached data. Optional, defaults to ``null``.

.. function:: GetWebhooksAsync()
	
	.. note::
	
		This method is applicable to guild text channels only.
	
	Gets and returns this channel's webhooks as a list of :doc:`DiscordWebhook </reference/entities/DiscordWebhook>` instances.

.. function:: PlaceMemberAsync(member)
	
	.. note::
	
		This method is applicable to guild voice channels only.
	
	Moves a specified member to this voice channel.
	
	:param member: Guild member (instance of :doc:`DiscordMember </reference/DiscordMember>`) to move.