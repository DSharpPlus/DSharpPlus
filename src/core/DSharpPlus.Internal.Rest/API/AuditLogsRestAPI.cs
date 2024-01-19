// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Internal.Rest.Ratelimiting;

using Remora.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IAuditLogsRestAPI"/>
public sealed class AuditLogsRestAPI
(
    IRestClient restClient
)
    : IAuditLogsRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IAuditLog>> ListGuildAuditLogEntriesAsync
    (
        Snowflake guildId,
        ListGuildAuditLogEntriesQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (query.Limit is not null and (< 1 or > 1000))
        {
            return new ValidationError("The limit of entries to return must be between 1 and 1000.");
        }

        QueryBuilder builder = new()
        {
            RootUri = $"guilds/{guildId}/audit-logs"
        };

        if (query.ActionType is not null)
        {
            int value = (int)query.ActionType.Value;
            builder.AddParameter("action_type", value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.After is not null)
        {
            builder.AddParameter("after", query.After.Value.ToString());
        }

        if (query.Before is not null)
        {
            builder.AddParameter("before", query.Before.Value.ToString());
        }

        if (query.Limit is not null)
        {
            builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.UserId is not null)
        {
            builder.AddParameter("user_id", query.UserId.Value.ToString());
        }

        return await restClient.ExecuteRequestAsync<IAuditLog>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId)
                 .WithRoute($"GET guilds/{guildId}/audit-logs"),
            info,
            ct
        );
    }
}
