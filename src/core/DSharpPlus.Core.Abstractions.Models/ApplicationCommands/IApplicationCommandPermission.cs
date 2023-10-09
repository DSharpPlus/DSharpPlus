// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a single permission override for an application command inside a guild.
/// </summary>
public interface IApplicationCommandPermission
{
    /// <summary>
    /// The snowflake identifier of the target of this override, or a permission constant.
    /// </summary>
    /// <remarks>
    /// The snowflake identifier of the current guild targets the @everyone role, the
    /// snowflake identifier - 1 targets all channels in the guild.
    /// </remarks>
    public Snowflake Id { get; }

    /// <summary>
    /// The type of the entity this override targets.
    /// </summary>
    public DiscordApplicationCommandPermissionType Type { get; }

    /// <summary>
    /// Indicates whether this command is allowed or not.
    /// </summary>
    public bool Permission { get; }
}
