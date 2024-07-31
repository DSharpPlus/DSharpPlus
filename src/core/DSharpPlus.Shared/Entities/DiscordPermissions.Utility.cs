// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Entities;

partial struct DiscordPermissions
{
    /// <summary>
    /// Toggles the specified permission between states.
    /// </summary>
    public DiscordPermissions Toggle(DiscordPermission permission)
        => this ^ permission;

    /// <summary>
    /// Toggles all of the specified permissions between states.   
    /// </summary>
    public DiscordPermissions Toggle(params ReadOnlySpan<DiscordPermission> permissions)
        => this ^ new DiscordPermissions(permissions);

    /// <summary>
    /// Returns whether the specified permission is set explicitly.
    /// </summary>
    public bool HasFlag(DiscordPermission flag)
        => GetFlag((int)flag);

    /// <summary>
    /// Returns whether the specified permission is granted, either directly or through Administrator permissions.
    /// </summary>
    public bool HasPermission(DiscordPermission permission)
        => this.HasFlag(DiscordPermission.Administrator) || this.HasFlag(permission);

    /// <summary>
    /// Returns whether any of the specified permissions are granted, either directly or through Administrator permissions.
    /// </summary>
    public bool HasAnyPermission(params ReadOnlySpan<DiscordPermission> permissions)
        => this.HasFlag(DiscordPermission.Administrator) || (this & new DiscordPermissions(permissions)) != None;

    /// <summary>
    /// Returns whether all of the specified permissions are gratned, either directly or through Administrator permissions.
    /// </summary>
    public bool HasAllPermissions(params ReadOnlySpan<DiscordPermission> permissions)
    {
        if (this.HasFlag(DiscordPermission.Administrator))
        {
            return true;
        }

        DiscordPermissions expected = new(permissions);
        return (this & expected) == expected;
    }
}
