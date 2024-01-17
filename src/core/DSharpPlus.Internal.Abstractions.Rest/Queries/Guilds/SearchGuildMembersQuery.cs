// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters to <c>IGuildRestAPI.SearchGuildMembersAsync</c>. This request cannot
/// be paginated.
/// </summary>
public readonly record struct SearchGuildMembersQuery
{
    /// <summary>
    /// The query string to match usernames and nicknames against.
    /// </summary>
    public required string Query { get; init; }

    /// <summary>
    /// The amount of guild members to return.
    /// </summary>
    public int? Limit { get; init; }
}
