Reference for ``DiscordMember``
===============================

Discord member represents a :doc:`DiscordUser </reference/DiscordUser>` that belongs to a :doc:`DiscordGuild </reference/DiscordGuild>`.

Members
-------

.. attribute:: User

	The user represented by this member.

.. attribute:: Nickname

	This member's nickname. ``null`` if nickname is not set.

.. attribute:: Roles

	List of this member's role IDs.

.. attribute:: JoinedAt

	Date and time this member joined associated guild.

.. attribute:: IsDeafened

	Whether the user is deafened in voice.

.. attribute:: IsMuted

	Whether the user is muted in voice.

Methods
-------

.. function:: SendDM()

	.. note::
	
		This method is asynchronous. It needs to be awaited.

	Creates a DM channel to this user. Returns a :doc:`DiscordDMChannel </reference/DiscordDMChannel>`.