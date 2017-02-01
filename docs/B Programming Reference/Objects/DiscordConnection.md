DiscordConnection
=================
Represents a Connection object

## Members

`ulong ID`: ID for this object

`DateTime CreationDate`: When this was created

`string name`: The username of the connection's account

`string Type`: The service of this connection

`bool Revoked`: Wether the connection is revoked

`List<DiscordIntegration> Integrations`: List of partial server integrations
