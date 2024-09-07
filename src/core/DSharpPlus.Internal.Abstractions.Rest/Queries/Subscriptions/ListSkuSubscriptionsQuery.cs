// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

public readonly record struct ListSkuSubscriptionsQuery : IPaginatedQuery
{

    /// <inheritdoc/>
    public Snowflake? Before { get; init; }

    /// <inheritdoc/>
    public Snowflake? After { get; init; }

    /// <inheritdoc/>
    public int? Limit { get; init; }

    /// <summary>
    /// Specifies a user ID for whom to query subscriptions. This is required except for OAuth2 queries.
    /// </summary>
    public Snowflake? UserId { get; init; }
}
