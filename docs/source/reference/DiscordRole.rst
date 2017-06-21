Reference for ``DiscordRole``
=============================

Represents a role that can be assigned to members in a :doc:`DiscordGuild </reference/DiscordGuild>`.

Members
-------

.. attribute:: Id

	This role's ID.

.. attribute:: Name

	This role's name.

.. attribute:: Color

	This role's color.

.. attribute:: Hoist

	Whether or not this role is hoisted.

.. attribute:: Position

	This role's position in the role hierarchy.

.. attribute:: Permissions

	This role's permissions. Enum of type :doc:`Permission </reference/misc/Permission>`.

.. attribute:: Managed

	.. note::
	
		If this value is true, this role's properties other than color and permissions cannot be modified.

	Whether or not this role is managed by integrations.

.. attribute:: Mentionable

	Whether or not this role can be mentioned by others.

.. attribute:: Mention

	This role's mention string.

Methods
-------

.. function:: CheckPermission(permission)

	Checks whether this role has a specified permission. Returns a :doc:`PermissionLevel </reference/misc/PermissionLevel>` 
	enum. Value is ``Allowed`` if the permission is present, otherwise ``Unset`` is returned.
	
	:param permission: An instance of :doc:`Permission </reference/misc/Permission>` enum to check for. Note that this method may not work on combined permissions.