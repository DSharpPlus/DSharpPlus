// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a role within a guild.
/// </summary>
public interface IRole : IPartialRole
{
    /// <inheritdoc cref="IPartialRole.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialRole.Name"/>
    public new string Name { get; }

    /// <inheritdoc cref="IPartialRole.Color"/>
    public new int Color { get; }

    /// <inheritdoc cref="IPartialRole.Hoist"/>
    public new bool Hoist { get; }

    /// <inheritdoc cref="IPartialRole.Position"/>
    public new int Position { get; }

    /// <inheritdoc cref="IPartialRole.Permissions"/>
    public new DiscordPermissions Permissions { get; }

    /// <inheritdoc cref="IPartialRole.Managed"/>
    public new bool Managed { get; }

    /// <inheritdoc cref="IPartialRole.Mentionable"/>
    public new bool Mentionable { get; }

    /// <inheritdoc cref="IPartialRole.Flags"/>
    public new DiscordRoleFlags Flags { get; }

    // partial access routes

    /// <inheritdoc/>
    Optional<Snowflake> IPartialRole.Id => this.Id;

    /// <inheritdoc/>
    Optional<string> IPartialRole.Name => this.Name;

    /// <inheritdoc/>
    Optional<int> IPartialRole.Color => this.Color;

    /// <inheritdoc/>
    Optional<bool> IPartialRole.Hoist => this.Hoist;

    /// <inheritdoc/>
    Optional<int> IPartialRole.Position => this.Position;

    /// <inheritdoc/>
    Optional<DiscordPermissions> IPartialRole.Permissions => this.Permissions;

    /// <inheritdoc/>
    Optional<bool> IPartialRole.Managed => this.Managed;

    /// <inheritdoc/>
    Optional<bool> IPartialRole.Mentionable => this.Mentionable;

    /// <inheritdoc/>
    Optional<DiscordRoleFlags> IPartialRole.Flags => this.Flags;
}
