// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters to <c>IEntitlementsRestAPI.ListEntitlementsAsync</c>.
/// </summary>
public readonly record struct ListEntitlementsQuery : IPaginationQuery
{
    /// <inheritdoc/>
    public Snowflake? Before { get; init; }

    /// <inheritdoc/>
    public Snowflake? After { get; init; }

    /// <inheritdoc/>
    public int? Limit { get; init; }

    /// <summary>
    /// The snowflake identifier of the user to look up entitlements for.
    /// </summary>
    public Snowflake? UserId { get; init; }

    /// <summary>
    /// A comma-delimited set of snowflakes of SKUs to check entitlements for.
    /// </summary>
    public string? SkuIds { get; init; }

    /// <summary>
    /// The snowflake identifier of the guild to look up entitlements for.
    /// </summary>
    public Snowflake? GuildId { get; init; }

    /// <summary>
    /// Specifies whether or not to include ended entitlements.
    /// </summary>
    public bool? ExcludeEnded { get; init; }
}