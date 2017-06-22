Reference for ``DiscordDMChannel``
==================================

Represents a Discord direct message channel. This class represents direct channels only. 

This class inherits from :doc:`DiscordChannel </reference/DiscordChannel>`, and thus, it shares its methods and 
properties, and you can pass it around as such. Due to member inheritance, only new or overriden members are 
documented below.

Members
-------

.. attribute:: Recipient

	This DM channel's recipient. Instance of :doc:`DiscordUser </reference/DiscordUser>`.

Methods
-------

.. function:: AddDmRecipientAsync(user_id, access_token, nickname)
	
	.. note::
	
		This method cannot be used by bots. It requires an OAuth token.
	
	Adds a user to a group direct message.
	
	:param user_id: ID of the user to add.
	:param access_token: OAuth token with ``gdm.join`` scope.
	:param nickname: Nickname to set for the added user.

.. function:: RemoveDmRecipientAsync(user_id, access_token)
	
	.. note::
	
		This method cannot be used by bots. It requires an OAuth token.
	
	Removes a user from a group direct message.
	
	:param user_id: ID of the user to remove.
	:param access_token: OAuth token with ``gdm.join`` scope.