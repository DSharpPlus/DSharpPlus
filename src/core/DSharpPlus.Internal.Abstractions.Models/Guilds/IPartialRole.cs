// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a partially populated role object.
/// </summary>
public interface IPartialRole
{
    /// <summary>
    /// The snowflake identifier of this role.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The name of this role.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The RGB color code of this role, #000000 represents a transparent role.
    /// </summary>
    public Optional<int> Color { get; }

    /// <summary>
    /// Indicates whether users with this role are hoisted in the member list.
    /// </summary>
    public Optional<bool> Hoist { get; }

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
    public Optional<int> Position { get; }

    /// <summary>
    /// The permissions associated with this role.
    /// </summary>
    public Optional<DiscordPermissions> Permissions { get; }

    /// <summary>
    /// Indicates whether this role is managed by an integration.
    /// </summary>
    public Optional<bool> Managed { get; }

    /// <summary>
    /// Indicates whether this role can be mentioned by users without the permission.
    /// </summary>
    public Optional<bool> Mentionable { get; }

    /// <summary>
    /// Additional tags added to this role.
    /// </summary>
    public Optional<IRoleTags> Tags { get; }

    /// <summary>
    /// Flags for this role, combined as a bitfield.
    /// </summary>
    public Optional<DiscordRoleFlags> Flags { get; }
}
