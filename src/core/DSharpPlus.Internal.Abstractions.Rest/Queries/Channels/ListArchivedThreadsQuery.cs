// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters for endpoints listing archived threads.
/// </summary>
public readonly record struct ListArchivedThreadsQuery
{
    /// <summary>
    /// A timestamp to filter threads by: only threads archived before this timestamp will be returned.
    /// </summary>
    public DateTimeOffset? Before { get; init; }

    /// <summary>
    /// The maximum number of threads to return from this request.
    /// </summary>
    public int? Limit { get; init; }
}
