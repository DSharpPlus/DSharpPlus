// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


namespace DSharpPlus.Core.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters to <c>IUserRestAPI.GetCurrentUserGuildsAsync</c>.
/// </summary>
public readonly record struct GetCurrentUserGuildsQuery : IPaginatedQuery
{
    /// <inheritdoc/>
    public Snowflake? Before { get; init; }

    /// <inheritdoc/>
    public Snowflake? After { get; init; }

    /// <inheritdoc/>
    public int? Limit { get; init; }

    /// <summary>
    /// Specifies whether the returned guild objects should include approximate member counts.
    /// </summary>
    public bool? WithCounts { get; init; }
}