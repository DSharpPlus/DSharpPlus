DiscordOverwrite
================
Represents a permission overwrite

## Members

`ulong ID`: ID for this object

`DateTime CreationDate`: When this was created

`string Type`: Type for this overwrite. Either "Role" or "Member".

`Permission Allow`: bit set for allowed permissions

`Permission Deny`: bit set for denied permissions

## Methods

#### CheckPermission
Checks the permission level for a given permission

`Permission permission`: Permission to check

Returns: `PermissionLevel`

#### DenyPermission
Denies a permission

`Permission p`: Permission to deny

Returns: Nothing. Update with `DiscordChannel.UpdateOverwrite`

#### UndenyPermission
Undenies a permission

`Permission p`: Permission to undeny

Returns: Nothing. Update with `DiscordChannel.UpdateOverwrite`

#### AllowPermission
Allows a permission

`Permission p`: Permission to allow

Returns: Nothing. Update with `DiscordChannel.UpdateOverwrite`

#### UndenyPermission
Unallows a permission

`Permission p`: Permission to unallow

Returns: Nothing. Update with `DiscordChannel.UpdateOverwrite`
