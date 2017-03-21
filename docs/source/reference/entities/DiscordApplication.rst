Reference for ``DiscordApplication``
====================================

``DiscordApplication`` describes the your discord application. This is used primarily for development of OAuth-based 
applications, but it can also be used to determine any application's properties.

Members
-------

.. attribute:: ID

	This application's ID.

.. attribute:: Description

	This application's description. This is usually used to describe the application's purpose.

.. attribute:: Icon

	This application's icon.

.. attribute:: Name

	This application's name.

.. attribute:: RpcOrigins

	This application's allowed RPC origins. This is used by RPC-based clients to determine which URLs are allowed to 
	control the RPC client.

.. attribute:: Flags

	This application's flags, such as require token grant, etc.

.. attribute:: Owner

	This application's owner.