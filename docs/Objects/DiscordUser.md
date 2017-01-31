DiscordUser
===========
Represents a User

## Members

`ulong ID`: ID for this object

`DateTime CreationDate`: When this was created

`string Username`: This User's username

`int Discriminator`: This User's Discriminator

`string AvatarHash`: This User's Avatar Hash

`string AvatarUrl`: This User's Avatar URL

`bool IsBot`: Whether this user is a bot

`bool? MFAEnabled`: Whether the user has two factor authentication enabled

`bool? Verified`: Whether the email on this account has been verified

`string Email`: The user's email

`string Mention`: Mentions the user similar to how a client would

`DiscordPresence Presence`: Gets this user's presence. (cached)
