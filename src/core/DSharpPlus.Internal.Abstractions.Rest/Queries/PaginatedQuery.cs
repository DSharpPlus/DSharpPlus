// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters for all purely snowflake-paginated requests.
/// </summary>
public readonly record struct PaginatedQuery : IPaginatedQuery
{
    /// <inheritdoc/>
    public Snowflake? Before { get; init; }

    /// <inheritdoc/>
    public Snowflake? After { get; init; }

    /// <inheritdoc/>
    public int? Limit { get; init; }
}