#pragma warning disable IDE0040

using System;

namespace DSharpPlus.Entities;

partial struct DiscordPermissions
{
    /// <summary>
    /// Toggles the specified permission between states.
    /// </summary>
    public readonly DiscordPermissions Toggle(DiscordPermission permission)
        => this ^ permission;

    /// <summary>
    /// Toggles all of the specified permissions between states.   
    /// </summary>
    public readonly DiscordPermissions Toggle(params ReadOnlySpan<DiscordPermission> permissions)
        => this ^ new DiscordPermissions(permissions);

    /// <summary>
    /// Returns whether the specified permission is set explicitly.
    /// </summary>
    public readonly bool HasFlag(DiscordPermission flag)
        => GetFlag((int)flag);

    /// <summary>
    /// Returns whether the specified permission is granted, either directly or through Administrator permissions.
    /// </summary>
    public readonly bool HasPermission(DiscordPermission permission)
        => HasFlag(DiscordPermission.Administrator) || HasFlag(permission);

    /// <summary>
    /// Returns whether any of the specified permissions are granted, either directly or through Administrator permissions.
    /// </summary>
    public readonly bool HasAnyPermission(DiscordPermissions permissions)
        => HasFlag(DiscordPermission.Administrator) || (this & permissions) != None;

    /// <summary>
    /// Returns whether all of the specified permissions are granted, either directly or through Administrator permissions.
    /// </summary>
    public readonly bool HasAllPermissions(DiscordPermissions expected)
        => HasFlag(DiscordPermission.Administrator) || (this & expected) == expected;
}
