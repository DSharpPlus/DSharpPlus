// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters to <c>IAuditLogsRestAPI.ListGuildAuditLogEntriesAsync</c>.
/// </summary>
public readonly record struct ListGuildAuditLogEntriesQuery : IPaginationQuery
{
    /// <summary>
    /// If specified, only list entries from this user ID.
    /// </summary>
    public Snowflake? UserId { get; init; }

    /// <summary>
    /// If specified, only list entries of this audit log event type.
    /// </summary>
    public DiscordAuditLogEvent? ActionType { get; init; }

    /// <summary>
    /// If specified, only list entries with an ID less than this ID.
    /// </summary>
    public Snowflake? Before { get; init; }

    /// <summary>
    /// If specified, only list entries with an ID greater than this ID.
    /// </summary>
    public Snowflake? After { get; init; }

    /// <summary>
    /// The maximum number of entries, between 1 and 100.
    /// </summary>
    public int? Limit { get; init; }
}