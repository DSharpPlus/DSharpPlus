// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters to <c>IGuildScheduledEventRestAPI.GetScheduledEventUsersAsync</c>.
/// </summary>
public readonly record struct GetScheduledEventUsersQuery : IPaginatedQuery
{
    /// <inheritdoc/>
    public Snowflake? Before { get; init; }

    /// <inheritdoc/>
    public Snowflake? After { get; init; }

    /// <inheritdoc/>
    public int? Limit { get; init; }

    /// <summary>
    /// Specifies whether the response should include guild member data.
    /// </summary>
    public bool? WithMember { get; init; }
}