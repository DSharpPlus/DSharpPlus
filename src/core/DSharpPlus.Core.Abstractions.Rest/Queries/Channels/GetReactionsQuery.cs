// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters for <c>IChannelRestAPI.GetReactionsAsync</c>.
/// </summary>
public readonly record struct GetReactionsQuery
{
    /// <summary>
    /// If specified, only request reactions from users with an ID greater than this ID.
    /// </summary>
    public Snowflake? After { get; init; }

    /// <summary>
    /// The maximum number of reactions for this request, up to 25.
    /// </summary>
    public int? Limit { get; init; }
}