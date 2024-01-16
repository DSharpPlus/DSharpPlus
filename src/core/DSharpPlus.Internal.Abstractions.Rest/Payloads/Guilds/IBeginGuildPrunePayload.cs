// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST guilds/:guild-id/prune</c>.
/// </summary>
public interface IBeginGuildPrunePayload
{
    /// <summary>
    /// The days of inactivity to measure, from 0 to 30.
    /// </summary>
    public int? Days { get; }

    /// <summary>
    /// A comma-separated list of snowflake identifiers of roles to include in the prune.
    /// </summary>
    /// <remarks>
    /// Any user with a subset of these roles will be considered for the prune. Any user with any role 
    /// not listed here will not be included in the count.
    /// </remarks>
    public string? IncludeRoles { get; }

    /// <summary>
    /// Specifies whether the amount of users pruned should be computed and returned.
    /// </summary>
    public bool? ComputeCount { get; }
}
