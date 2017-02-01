DiscordOverwrite
================
Represents a permission overwrite

## Members

`ulong ID`: ID for this object

`DateTime CreationDate`: When this was created

`string Type`: Type for this overwrite. Either "Role" or "Member".

`int Allow`: bit set for allowed permissions

`int Deny`: bit set for denied permissions
