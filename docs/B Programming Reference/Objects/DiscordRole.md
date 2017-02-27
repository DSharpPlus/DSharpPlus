DiscordRole
===========
Represents a Role

## Members

`ulong ID`: ID for this object

`DateTime CreationDate`: When this was created

`string Name`: Role name

`int color`: Integer representation of a hexadecimal color code

`bool Hoist`: Wether this role is pinned

`int Position`: Position for this role

`Permission Permissions`: Permission bit set

`bool Managed`: Whether this role is managed by an integration

`bool Mentionable`: Whether this role is mentionable

`string Mention`: Mentions the role similar to how a client would, if the role is mentionable

## Methods

#### CheckPermission
Checks the permission level for a given permission

`Permission permission`: Permission to check

Returns: `PermissionLevel`

#### AddPermission
Adds a permission

`Permission p`: Permission to add

Returns: Nothing. Update with `DiscordGuild.UpdateRole`

#### RemovePermission
Removes a permission

`Permission p`: Permission to remove

Returns: Nothing. Update with `DiscordGuild.UpdateRole`
