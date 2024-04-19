// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IInviteRestAPI"/>
public sealed class InviteRestAPI(IRestClient restClient)
    : IInviteRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IInvite>> DeleteInviteAsync
    (
        string inviteCode,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IInvite>
        (
            HttpMethod.Delete,
            $"invites/{inviteCode}",
            b => b.WithRoute($"DELETE invites/:invite-code")
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IInvite>> GetInviteAsync
    (
        string inviteCode,
        GetInviteQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new($"invites/{inviteCode}");

        if (query.WithCounts is not null)
        {
            _ = builder.AddParameter("with_counts", query.WithCounts.Value.ToString().ToLowerInvariant());
        }

        if (query.WithExpiration is not null)
        {
            _ = builder.AddParameter("with_expiration", query.WithExpiration.Value.ToString().ToLowerInvariant());
        }

        if (query.GuildScheduledEventId is not null)
        {
            _ = builder.AddParameter("guild_scheduled_event_id", query.GuildScheduledEventId.Value.ToString());
        }

        return await restClient.ExecuteRequestAsync<IInvite>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithRoute($"GET invites/:invite-code"),
            info,
            ct
        );
    }
}
