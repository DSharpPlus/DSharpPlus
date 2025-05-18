using System.Collections.Generic;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an overwrite collection variant that can answer questions about a member or role's exact situation.
/// </summary>
public abstract class TargetedDiscordOverwriteCollection : DiscordOverwriteCollection
{
    /// <summary>
    /// Gets the total set of permissions the target is able to wield.
    /// </summary>
    public abstract DiscordPermissions TotalPermissions { get; protected set; }

    /// <summary>
    /// Grants a permission to the target.
    /// </summary>
    /// <param name="permission">The permission to grant.</param>
    public abstract void GrantPermission(DiscordPermission permission);

    /// <summary>
    /// Grants a set of permissions to the target.
    /// </summary>
    /// <param name="permissions">The permissions to grant.</param>
    public abstract void GrantPermissions(DiscordPermissions permissions);

    /// <summary>
    /// Denies a permission from the target, including removing all possible allowing overwrites. This may be a destructive action.
    /// </summary>
    /// <param name="permission">The permission to deny.</param>
    public abstract void DenyPermission(DiscordPermission permission);

    /// <summary>
    /// Denies a set of permissions from the target, including removing all possible allowing overwrites. This may be a destructive action.
    /// </summary>
    /// <param name="permissions">The permissions to deny.</param>
    public abstract void DenyPermissions(DiscordPermissions permissions);

    /// <summary>
    /// Unsets a permission from all overwrites affecting the target, deferring back to their guild-wide permissions. This may be a destructive action.
    /// </summary>
    /// <param name="permission">The permission to unset.</param>
    public abstract void UnsetPermission(DiscordPermission permission);

    /// <summary>
    /// Unsets a set of permissions from all overwrites affecting the target, deferring back to their guild-wide permissions. This may be a destructive action.
    /// </summary>
    /// <param name="permissions">The permissions to unset.</param>
    public abstract void UnsetPermissions(DiscordPermissions permissions);

    /// <summary>
    /// Gets a list of roles whose overwrites deny the target a permission. Empty if the permission is not affected by overwrites.
    /// </summary>
    public abstract IReadOnlyList<ulong> GetDenyingOverwrites(DiscordPermission permission);

    /// <summary>
    /// Gets a list of roles whose overwrites deny the target all of a set of permissions. Empty if no overwrites control the entire set.
    /// </summary>
    public abstract IReadOnlyList<ulong> GetAllDenyingOverwrites(DiscordPermissions permissions);

    /// <summary>
    /// Gets a list of roles whose overwrites deny the target any of a set of permissions. Empty if the permissions are not affected by overwrites.
    /// </summary>
    public abstract IReadOnlyList<ulong> GetAnyDenyingOverwrites(DiscordPermissions permissions);

    /// <summary>
    /// Gets a list of roles whose overwrites grant the target a permission. Empty if the permission is not affected by overwrites.
    /// </summary>
    public abstract IReadOnlyList<ulong> GetGrantingOverwrites(DiscordPermission permission);

    /// <summary>
    /// Gets a list of roles whose overwrites grant the target all of a set of permissions. Empty if no overwrites control the entire set.
    /// </summary>
    public abstract IReadOnlyList<ulong> GetAllGrantingOverwrites(DiscordPermissions permissions);

    /// <summary>
    /// Gets a list of roles whose overwrites grant the target any of a set of permissions. Empty if the permissions are not affected by overwrites.
    /// </summary>
    public abstract IReadOnlyList<ulong> GetAnyGrantingOverwrites(DiscordPermissions permissions);

    /// <summary>
    /// Checks whether the target has a given permission.
    /// </summary>
    public abstract bool HasPermission(DiscordPermission permission);

    /// <summary>
    /// Checks whether the target has any of a given set of permissions.
    /// </summary>
    public abstract bool HasAnyPermission(DiscordPermissions permissions);

    /// <summary>
    /// Checks whether the target has all of a given set of permissions.
    /// </summary>
    public abstract bool HasAllPermissions(DiscordPermissions permissions);
}
