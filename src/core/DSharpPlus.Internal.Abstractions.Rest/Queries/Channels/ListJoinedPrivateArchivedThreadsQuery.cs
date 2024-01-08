// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters for listing joined private archived threads, specifically.
/// </summary>
public readonly record struct ListJoinedPrivateArchivedThreadsQuery
{
    /// <summary>
    /// A snowflake to filter threads by: only threads archived before this timestamp will be returned.
    /// </summary>
    public Snowflake? Before { get; init; }

    /// <summary>
    /// The maximum number of threads to return from this request.
    /// </summary>
    public int? Limit { get; init; }
}
