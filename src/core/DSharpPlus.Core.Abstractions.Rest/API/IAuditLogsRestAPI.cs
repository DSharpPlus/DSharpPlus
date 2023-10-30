// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Entities;

using Remora.Results;

namespace DSharpPlus.Core.Abstractions.Rest.API;

/// <summary>
/// Provides access to audit log related rest API calls.
/// </summary>
public interface IAuditLogsRestAPI
{
    /// <summary>
    /// Lists audit log entries for a given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild to list audit logs for.</param>
    /// <param name="userId">If specified, only lists entries from this user ID.</param>
    /// <param name="actionType">If specified, only lists entries of this audit log event type.</param>
    /// <param name="before">If specified, only lists entries with an ID less than this ID.</param>
    /// <param name="after">If specified, only lists entries with an ID greater than this ID.</param>
    /// <param name="limit">The maximum number of entries, between 1 and 100.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>An audit log object, containing all relevant other entities and the main audit log.</returns>
    public ValueTask<Result<IAuditLog>> ListGuildAuditLogEntriesAsync
    (
        Snowflake guildId,
        Snowflake? userId = null,
        DiscordAuditLogEvent? actionType = null,
        Snowflake? before = null,
        Snowflake? after = null,
        int? limit = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
