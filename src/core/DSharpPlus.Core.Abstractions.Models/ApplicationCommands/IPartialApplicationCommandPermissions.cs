// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a partial set of application command permissions.
/// </summary>
public interface IPartialApplicationCommandPermissions
{
    /// <summary>
    /// The snowflake identifier of this command.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The snowflake identifier of the application this command belongs to.
    /// </summary>
    public Optional<Snowflake> ApplicationId { get; }

    /// <summary>
    /// The snowflake identifier of the guild to which these permissions apply.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// The permission overrides for this command in this guild.
    /// </summary>
    public Optional<IReadOnlyList<IApplicationCommandPermission>> Permissions { get; }
}
