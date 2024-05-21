// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

public readonly record struct GetReactionsQuery
{
    /// <summary>
    /// The type of reactions to query for.
    /// </summary>
    public ReactionType? Type { get; init; }

    /// <summary>
    /// If specified, only request entities with an ID greater than this ID.
    /// </summary>
    public Snowflake? After { get; init; }

    /// <summary>
    /// The maximum number of entities to return from this request.
    /// </summary>
    public int? Limit { get; init; }
}
