namespace DSharpPlus.Entities;

/// <summary>
/// Specifies whether a permission in an overwrite is allowed, denied or unset.
/// </summary>
public enum DiscordPermissionLevel
{
    /// <summary>
    /// The specified permission is allowed. This supersedes all other overwrites.
    /// </summary>
    Allowed,

    /// <summary>
    /// The specified permission is denied.
    /// </summary>
    Denied,

    /// <summary>
    /// The specified permission is unset and falls back to role permissions.
    /// </summary>
    Unset
}
