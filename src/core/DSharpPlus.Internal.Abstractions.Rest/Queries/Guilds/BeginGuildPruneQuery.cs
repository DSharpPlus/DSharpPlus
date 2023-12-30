// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters for <c>IGuildRestAPI.BeginGuildPruneAsync</c>.
/// </summary>
public readonly record struct BeginGuildPruneQuery
{
    /// <summary>
    /// The days of inactivity to measure, from 0 to 30.
    /// </summary>
    public int? Days { get; init; }

    /// <summary>
    /// A comma-separated list of snowflake identifiers of roles to include in the prune.
    /// </summary>
    /// <remarks>
    /// Any user with a subset of these roles will be considered for the prune. Any user with any role 
    /// not listed here will not be included in the count.
    /// </remarks>
    public string? IncludeRoles { get; init; }

    /// <summary>
    /// Specifies whether the amount of users pruned should be computed and returned.
    /// </summary>
    public bool? ComputeCount { get; init; }
}