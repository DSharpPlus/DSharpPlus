// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a single member of a <seealso cref="ITeam"/>.
/// </summary>
public interface ITeamMember
{
    /// <summary>
    /// This user's membership state on the team.
    /// </summary>
    public DiscordTeamMembershipState MembershipState { get; }

    /// <summary>
    /// This will always be a single string; <c>"*"</c>.
    /// </summary>
    public IReadOnlyList<string> Permissions { get; }

    /// <summary>
    /// The snowflake identifier of the parent team.
    /// </summary>
    public Snowflake TeamId { get; }

    /// <summary>
    /// The snowflake identifier, username and avatar of this user's discord account.
    /// </summary>
    public IPartialUser User { get; }
}
