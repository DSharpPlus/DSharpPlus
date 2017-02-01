DiscordIntegration
=================
Represents a guild's integration

## Members

`ulong ID`: ID for this object

`DateTime CreationDate`: When this was created


`string Name`: integration name

`string Type`: integration type

`bool enabled`: Wether this integration has been enabled

`bool Syncing`: Wether this integration is syncing

`ulong RoleID`: ID that this integration uses for "subscribers"

`int ExpireBehavior`: The behavior of expiring subscribers

`int ExpireGracePeriod`: The grace period before expiring subscribers

`DiscordUser User`: User for this integration

`DiscordIntegrationAccount Account`: Integration account information

`DateTime SyncedAt`: When this integration was last synced
