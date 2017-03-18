Reference for ``DiscordApplication``
====================================

DiscordApplication describes the your discord application. This is used primarily for development of OAuth-based 
applications, but it can also be used to determine any application's properties.

Members
-------

.. attribute:: ID

	Application's ID.

.. attribute:: Description

	Application's description. This is usually used to describe the application's purpose.

.. attribute:: Icon

	Application's icon.

.. attribute:: Name

	Application's name.

.. attribute:: RpcOrigins

	Application's RPC origins. This is used by RPC-based clients to determine which URLs are allowed to control the RPC client.

.. attribute:: Flags

	Application's flags, such as require token grant, etc.

.. attribute:: Owner

	Application's owner.