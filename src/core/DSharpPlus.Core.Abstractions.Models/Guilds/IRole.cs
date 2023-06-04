// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a role within a guild.
/// </summary>
public interface IRole
{
    /// <summary>
    /// The snowflake identifier of this role.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The name of this role.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The RGB color code of this role, #000000 represents a transparent role.
    /// </summary>
    public int Color { get; }

    /// <summary>
    /// Indicates whether users with this role are hoisted in the member list.
    /// </summary>
    public bool Hoist { get; }

    /// <summary>
    /// This role's role icon hash, if applicable.
    /// </summary>
    public Optional<string?> Hash { get; }

    /// <summary>
    /// The unicode emoji serving as this role's role icon, if applicable.
    /// </summary>
    public Optional<string?> UnicodeEmoji { get; }

    /// <summary>
    /// The position of this role in the role list.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// The permissions associated with this role.
    /// </summary>
    public DiscordPermissions Permissions { get; }

    /// <summary>
    /// Indicates whether this role is managed by an integration.
    /// </summary>
    public bool Managed { get; }

    /// <summary>
    /// Indicates whether this role can be mentioned by users without the permission.
    /// </summary>
    public bool Mentionable { get; }

    /// <summary>
    /// Additional tags added to this role.
    /// </summary>
    public Optional<IRoleTags> Tags { get; }
}
