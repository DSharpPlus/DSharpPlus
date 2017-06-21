Reference for ``DiscordMember``
===============================

Discord member represents a :doc:`DiscordUser </reference/DiscordUser>` that belongs to a :doc:`DiscordGuild </reference/DiscordGuild>`.

This class inherits from :doc:`DiscordUser </reference/DiscordUser>`, and thus, it shares its methods and 
properties, and you can pass it around as such. Due to member inheritance, only new or overriden are
documented below.

Members
-------

.. attribute:: Nickname

	This member's nickname. ``null`` if nickname is not set.

.. attribute:: DisplayName

	This member's display name. It returns a nickname if one is set, otherwise returns the username.

.. attribute:: Roles

	List of this member's roles.

.. attribute:: Color

	Color given to this member by his highest color-giving role.

.. attribute:: JoinedAt

	Date and time this member joined associated guild.

.. attribute:: IsDeafened

	Whether the user is deafened in voice.

.. attribute:: IsMuted

	Whether the user is muted in voice.

.. attribute:: VoiceState

	This member's voice state. Instance of :doc:`DiscordVoiceState </reference/entities/DiscordVoiceState`.

.. attribute:: Presence

	This member's guild presence. Instance of :doc:`DiscordPresence </reference/entities/DiscordPresence`.

.. attribute:: UserPresence

	This member underlying user's presence. Instance of :doc:`DiscordPresence </reference/entities/DiscordPresence`.

.. attribute:: Guild

	The guild this member is associated with.

Methods
-------

.. function:: CreateDmChannelAsync()

	Creates a direct message channel to this user. If one exists already, gets it. Returns a :doc:`DiscordDMChannel </reference/DiscordDmChannel>`.

.. function:: SetMuteAsync(mute, reason)

	Mutes or unmutes the member in voice.
	
	:param mute: Whether or not the member is to be muted.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: SetDeafAsync(deaf, reason)

	Deafens or undeafens the member in voice.
	
	:param deaf: Whether or not the member is to be deafened.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: GrantRoleAsync(role, reason)

	Grants a role to the member.

	:param role: Role to grant to the member.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: TakeRoleAsync(role, reason)

	Takes the role from a member.

	:param role: Role to take from the member.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: ReplaceRolesAsync(roles, reason)

	Replaces member's roles with ones specified.

	:param roles: Roles to replace the member's roles with.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.

.. function:: ModifyAsync(nickname, roles, mute, deaf, voice_channel, reason)

	Modifies this guild member.
	
	:param nickname: New nickname for the member. Optional, defaults to ``null``.
	:param roles: Roles to replace this member's roles with. Optional, defaults to ``null``.
	:param mute: Whether or not the member is to be muted in voice. Optional, defaults to ``null``.
	:param deaf: Whether or not the member is to be deafed in voice. Optional, defaults to ``null``.
	:param voice_channel: Voice channel to move the member into. Optional, defaults to ``null``.
	:param reason: Reason for audit logs. Optional, defaults to ``null``.