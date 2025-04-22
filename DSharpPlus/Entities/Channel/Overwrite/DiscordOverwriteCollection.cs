using System.Collections.Generic;

namespace DSharpPlus.Entities;

/// <summary>
/// Provides a simple and consistent way to manage permission overwrites.
/// </summary>
public class DiscordOverwriteCollection
{
    protected DiscordClient? client;
    protected DiscordChannel? channel;

    protected readonly Dictionary<ulong, DiscordOverwrite> overwrites;

    public TargetedDiscordOverwriteCollection For(DiscordMember member)
        => new MemberTargetedOverwriteCollection(this, member);

    public TargetedDiscordOverwriteCollection For(DiscordRole role)
        => new RoleTargetedOverwriteCollection(this, role);

    /// <summary>
    /// Grants a user a permission via explicit overwrite.
    /// </summary>
    /// <param name="user">The specified user.</param>
    /// <param name="permission">The permission to grant to this user.</param>
    public void GrantUserPermission(DiscordUser user, DiscordPermission permission)
        => GrantUserPermissions(user.Id, [permission]);

    /// <summary>
    /// Grants a user a set of permissions via explicit overwrite.
    /// </summary>
    /// <param name="user">The specified user.</param>
    /// <param name="permissions">The set of permissions to grant to this user.</param>
    public void GrantUserPermissions(DiscordUser user, DiscordPermissions permissions)
        => GrantUserPermissions(user.Id, permissions);

    /// <summary>
    /// Grants a user a permission via explicit overwrite.
    /// </summary>
    /// <param name="userId">The snowflake identifier of the user.</param>
    /// <param name="permission">The permission to grant to this user.</param>
    public void GrantUserPermission(ulong userId, DiscordPermission permission)
        => GrantUserPermissions(userId, [permission]);

    /// <summary>
    /// Grants a user a set of permissions via explicit overwrite.
    /// </summary>
    /// <param name="userId">The snowflake identifier of the user.</param>
    /// <param name="permissions">The set of permissions to grant to this user.</param>
    public void GrantUserPermissions(ulong userId, DiscordPermissions permissions)
    {
        if (this.overwrites.TryGetValue(userId, out DiscordOverwrite? value))
        {
            value.Denied -= permissions;
            value.Allowed += permissions;
        }
        else
        {
            DiscordOverwrite overwrite = new()
            {
                Id = userId,
                Type = DiscordOverwriteType.Member,
                Allowed = permissions
            };

            this.overwrites.Add(userId, overwrite);
        }
    }

    /// <summary>
    /// Grants a role a permission via explicit overwrite.
    /// </summary>
    /// <param name="role">The specified role.</param>
    /// <param name="permission">The permission to grant to this role.</param>
    public void GrantRolePermission(DiscordRole role, DiscordPermission permission)
        => GrantRolePermissions(role.Id, [permission]);

    /// <summary>
    /// Grants a role a set of permissions via explicit overwrite.
    /// </summary>
    /// <param name="role">The specified role.</param>
    /// <param name="permissions">The set of permissions to grant to this role.</param>
    public void GrantRolePermissions(DiscordRole role, DiscordPermissions permissions)
        => GrantRolePermissions(role.Id, permissions);

    /// <summary>
    /// Grants a role a permission via explicit overwrite.
    /// </summary>
    /// <param name="roleId">The snowflake identifier of the role.</param>
    /// <param name="permission">The permission to grant to this role.</param>
    public void GrantRolePermission(ulong roleId, DiscordPermission permission)
        => GrantRolePermissions(roleId, [permission]);

    /// <summary>
    /// Grants a role a set of permissions via explicit overwrite.
    /// </summary>
    /// <param name="roleId">The snowflake identifier of the role.</param>
    /// <param name="permissions">The set of permissions to grant to this role.</param>
    public void GrantRolePermissions(ulong roleId, DiscordPermissions permissions)
    {
        if (this.overwrites.TryGetValue(roleId, out DiscordOverwrite? value))
        {
            value.Denied -= permissions;
            value.Allowed += permissions;
        }
        else
        {
            DiscordOverwrite overwrite = new()
            {
                Id = roleId,
                Type = DiscordOverwriteType.Role,
                Allowed = permissions
            };

            this.overwrites.Add(roleId, overwrite);
        }
    }

    /// <summary>
    /// Denies a user a permission via explicit overwrite.
    /// </summary>
    /// <remarks>
    /// Note that this may not impact their ability to perform actions controlled by this permission if it is otherwise granted by another overwrite. 
    /// To ascertain this, please use <see cref="For(DiscordMember)"/> instead, which ensures the user is denied a permission unless they are an administrator.
    /// </remarks>
    /// <param name="user">The specified user.</param>
    /// <param name="permission">The permission to deny to this user.</param>
    public void DenyUserPermission(DiscordUser user, DiscordPermission permission)
        => DenyUserPermissions(user.Id, [permission]);

    /// <summary>
    /// Denies a user a set of permissions via explicit overwrite.
    /// </summary>
    /// <remarks>
    /// Note that this may not impact their ability to perform actions controlled by this permission if it is otherwise granted by another overwrite. 
    /// To ascertain this, please use <see cref="For(DiscordMember)"/> instead, which ensures the user is denied a permission unless they are an administrator.
    /// </remarks>
    /// <param name="user">The specified user.</param>
    /// <param name="permissions">The set of permissions to deny to this user.</param>
    public void DenyUserPermissions(DiscordUser user, DiscordPermissions permissions)
        => DenyUserPermissions(user.Id, permissions);

    /// <summary>
    /// Denies a user a permission via explicit overwrite.
    /// </summary>
    /// <remarks>
    /// Note that this may not impact their ability to perform actions controlled by this permission if it is otherwise granted by another overwrite. 
    /// To ascertain this, please use <see cref="For(DiscordMember)"/> instead, which ensures the user is denied a permission unless they are an administrator.
    /// </remarks>
    /// <param name="userId">The snowflake identifier of the user.</param>
    /// <param name="permission">The permission to deny to this user.</param>
    public void DenyUserPermission(ulong userId, DiscordPermission permission)
        => DenyUserPermissions(userId, [permission]);

    /// <summary>
    /// Denies a user a set of permissions via explicit overwrite.
    /// </summary>
    /// <remarks>
    /// Note that this may not impact their ability to perform actions controlled by this permission if it is otherwise granted by another overwrite. 
    /// To ascertain this, please use <see cref="For(DiscordMember)"/> instead, which ensures the user is denied a permission unless they are an administrator.
    /// </remarks>
    /// <param name="userId">The snowflake identifier of the user.</param>
    /// <param name="permissions">The set of permissions to deny to this user.</param>
    public void DenyUserPermissions(ulong userId, DiscordPermissions permissions)
    {
        if (this.overwrites.TryGetValue(userId, out DiscordOverwrite? value))
        {
            value.Denied += permissions;
            value.Allowed -= permissions;
        }
        else
        {
            DiscordOverwrite overwrite = new()
            {
                Id = userId,
                Type = DiscordOverwriteType.Member,
                Denied = permissions
            };

            this.overwrites.Add(userId, overwrite);
        }
    }

    /// <summary>
    /// Denies a role a permission via explicit overwrite.
    /// </summary>
    /// <remarks>
    /// Depending on their other roles, denying a role a permission may not impact user's ability to perform actions controlled by that permission.
    /// To ensure users are denied a permission, please use <see cref="For(DiscordMember)"/> instead, which ensures they are denied a permission unless they are an administrator.
    /// </remarks>
    /// <param name="role">The specified role.</param>
    /// <param name="permission">The permission to deny to this role.</param>
    public void DenyRolePermission(DiscordRole role, DiscordPermission permission)
        => DenyRolePermissions(role.Id, [permission]);

    /// <summary>
    /// Denies a role a set of permissions via explicit overwrite.
    /// </summary>
    /// <remarks>
    /// Depending on their other roles, denying a role a permission may not impact user's ability to perform actions controlled by that permission.
    /// To ensure users are denied a permission, please use <see cref="For(DiscordMember)"/> instead, which ensures they are denied a permission unless they are an administrator.
    /// </remarks>
    /// <param name="role">The specified role.</param>
    /// <param name="permissions">The set of permissions to deny to this role.</param>
    public void DenyRolePermissions(DiscordRole role, DiscordPermissions permissions)
        => DenyRolePermissions(role.Id, permissions);

    /// <summary>
    /// Denies a role a permission via explicit overwrite.
    /// </summary>
    /// <remarks>
    /// Depending on their other roles, denying a role a permission may not impact user's ability to perform actions controlled by that permission.
    /// To ensure users are denied a permission, please use <see cref="For(DiscordMember)"/> instead, which ensures they are denied a permission unless they are an administrator.
    /// </remarks>
    /// <param name="roleId">The snowflake identifier of the role.</param>
    /// <param name="permission">The permission to deny to this role.</param>
    public void DenyRolePermission(ulong roleId, DiscordPermission permission)
        => DenyRolePermissions(roleId, [permission]);

    /// <summary>
    /// Denies a role a set of permissions via explicit overwrite.
    /// </summary>
    /// <remarks>
    /// Depending on their other roles, denying a role a permission may not impact user's ability to perform actions controlled by that permission.
    /// To ensure users are denied a permission, please use <see cref="For(DiscordMember)"/> instead, which ensures they are denied a permission unless they are an administrator.
    /// </remarks>
    /// <param name="roleId">The snowflake identifier of the role.</param>
    /// <param name="permissions">The set of permissions to deny to this role.</param>
    public void DenyRolePermissions(ulong roleId, DiscordPermissions permissions)
    {
        if (this.overwrites.TryGetValue(roleId, out DiscordOverwrite? value))
        {
            value.Denied += permissions;
            value.Allowed -= permissions;
        }
        else
        {
            DiscordOverwrite overwrite = new()
            {
                Id = roleId,
                Type = DiscordOverwriteType.Role,
                Denied = permissions
            };

            this.overwrites.Add(roleId, overwrite);
        }
    }

    /// <summary>
    /// Unsets a user a permission from an explicit overwrite.
    /// </summary>
    /// <param name="user">The specified user.</param>
    /// <param name="permission">The permission to unset from this user's overwrite.</param>
    public void UnsetUserPermission(DiscordUser user, DiscordPermission permission)
        => UnsetUserPermissions(user.Id, [permission]);

    /// <summary>
    /// Unsets a user a set of permissions from an explicit overwrite.
    /// </summary>
    /// <param name="user">The specified user.</param>
    /// <param name="permissions">The set of permissions to unset from this user's overwrite.</param>
    public void UnsetUserPermissions(DiscordUser user, DiscordPermissions permissions)
        => UnsetUserPermissions(user.Id, permissions);

    /// <summary>
    /// Unsets a user a permission from an explicit overwrite.
    /// </summary>
    /// <param name="userId">The snowflake identifier of the user.</param>
    /// <param name="permission">The permission to unset from this user's overwrite.</param>
    public void UnsetUserPermission(ulong userId, DiscordPermission permission)
        => UnsetUserPermissions(userId, [permission]);

    /// <summary>
    /// Unsets a user a set of permissions from an explicit overwrite.
    /// </summary>
    /// <param name="userId">The snowflake identifier of the user.</param>
    /// <param name="permissions">The set of permissions to unset from this user's overwrite.</param>
    public void UnsetUserPermissions(ulong userId, DiscordPermissions permissions)
    {
        if (this.overwrites.TryGetValue(userId, out DiscordOverwrite? value))
        {
            value.Denied -= permissions;
            value.Allowed += permissions;
        }
        else
        {
            DiscordOverwrite overwrite = new()
            {
                Id = userId,
                Type = DiscordOverwriteType.Member,
                Allowed = permissions
            };

            this.overwrites.Add(userId, overwrite);
        }
    }

    /// <summary>
    /// Unsets a role a permission from an explicit overwrite.
    /// </summary>
    /// <param name="role">The specified role.</param>
    /// <param name="permission">The permission to unset from this role's overwrite.</param>
    public void UnsetRolePermission(DiscordRole role, DiscordPermission permission)
        => UnsetRolePermissions(role.Id, [permission]);

    /// <summary>
    /// Unsets a role a set of permissions from an explicit overwrite.
    /// </summary>
    /// <param name="role">The specified role.</param>
    /// <param name="permissions">The set of permissions to unset from this role's overwrite.</param>
    public void UnsetRolePermissions(DiscordRole role, DiscordPermissions permissions)
        => UnsetRolePermissions(role.Id, permissions);

    /// <summary>
    /// Unsets a role a permission from an explicit overwrite.
    /// </summary>
    /// <param name="roleId">The snowflake identifier of the role.</param>
    /// <param name="permission">The permission to unset from this role's overwrite.</param>
    public void UnsetRolePermission(ulong roleId, DiscordPermission permission)
        => UnsetRolePermissions(roleId, [permission]);

    /// <summary>
    /// Unsets a role a set of permissions from an explicit overwrite.
    /// </summary>
    /// <param name="roleId">The snowflake identifier of the role.</param>
    /// <param name="permissions">The set of permissions to unset from this role's overwrite.</param>
    public void UnsetRolePermissions(ulong roleId, DiscordPermissions permissions)
    {
        if (this.overwrites.TryGetValue(roleId, out DiscordOverwrite? value))
        {
            value.Denied -= permissions;
            value.Allowed += permissions;
        }
        else
        {
            DiscordOverwrite overwrite = new()
            {
                Id = roleId,
                Type = DiscordOverwriteType.Role,
                Allowed = permissions
            };

            this.overwrites.Add(roleId, overwrite);
        }
    }

    // stop people from inheriting this. it's not sealed because we inherit from it.
    internal DiscordOverwriteCollection() { }
}
