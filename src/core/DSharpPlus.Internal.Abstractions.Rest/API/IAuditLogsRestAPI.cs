// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Abstractions.Rest.API;

/// <summary>
/// Provides access to audit log related rest API calls.
/// </summary>
public interface IAuditLogsRestAPI
{
    /// <summary>
    /// Lists audit log entries for a given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild to list audit logs for.</param>
    /// <param name="query">
    /// Contains the query parameters for this request, comprised of request pagination and log entry
    /// filtering options.
    /// </param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>An audit log object, containing all relevant other entities and the main audit log.</returns>
    public ValueTask<Result<IAuditLog>> ListGuildAuditLogEntriesAsync
    (
        Snowflake guildId,
        ListGuildAuditLogEntriesQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
