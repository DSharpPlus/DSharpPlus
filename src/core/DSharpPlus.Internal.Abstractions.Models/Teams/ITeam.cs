// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a developer team on Discord. Teams can collectively own applications
/// and thereby bots.
/// </summary>
public interface ITeam
{
    /// <summary>
    /// The icon hash for this team.
    /// </summary>
    public string? Icon { get; }

    /// <summary>
    /// The snowflake identifier of this team.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The members of this team.
    /// </summary>
    public IReadOnlyList<ITeamMember> Members { get; }

    /// <summary>
    /// The name of this team.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The user ID of the current team owner.
    /// </summary>
    public Snowflake OwnerUserId { get; }
}
