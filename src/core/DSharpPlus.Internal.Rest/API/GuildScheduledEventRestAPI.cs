// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Internal.Rest.Ratelimiting;

using Remora.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IGuildScheduledEventRestAPI"/>
public sealed class GuildScheduledEventRestAPI
(
    IRestClient restClient
)
    : IGuildScheduledEventRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IScheduledEvent>> CreateGuildScheduledEventAsync
    (
        Snowflake guildId,
        ICreateGuildScheduledEventPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IScheduledEvent>
        (
            HttpMethod.Post,
            $"guilds/{guildId}/scheduled-events",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/scheduled-events")
                 .WithFullRatelimit($"POST guilds/{guildId}/scheduled-events")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteScheduledEventAsync
    (
        Snowflake guildId,
        Snowflake eventId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"guilds/{guildId}/scheduled-events/{eventId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/scheduled-events/:event-id")
                 .WithFullRatelimit($"DELETE guilds/{guildId}/scheduled-events/:event-id"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IScheduledEvent>> GetScheduledEventAsync
    (
        Snowflake guildId,
        Snowflake eventId,
        WithUserCountQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new()
        {
            RootUri = $"guilds/{guildId}/scheduled-events/{eventId}"
        };

        if (query.WithUserCount is not null)
        {
            builder.AddParameter("with_user_count", query.WithUserCount.Value.ToString().ToLowerInvariant());
        }

        return await restClient.ExecuteRequestAsync<IScheduledEvent>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/scheduled-events/:event-id")
                 .WithFullRatelimit($"GET guilds/{guildId}/scheduled-events/:event-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IScheduledEventUser>>> GetScheduledEventUsersAsync
    (
        Snowflake guildId,
        Snowflake eventId,
        GetScheduledEventUsersQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new()
        {
            RootUri = $"guilds/{guildId}/scheduled-events/{eventId}/users"
        };

        if (query.Limit is not null)
        {
            if (query.Limit is < 1 or > 100)
            {
                return new ValidationError("The amount of scheduled event users to request must be between 1 and 100.");
            }

            builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.WithMember is not null)
        {
            builder.AddParameter("with_member", query.WithMember.Value.ToString().ToLowerInvariant());
        }

        if (query.Before is not null)
        {
            builder.AddParameter("before", query.Before.Value.ToString());
        }

        if (query.After is not null)
        {
            builder.AddParameter("after", query.After.Value.ToString());
        }

        return await restClient.ExecuteRequestAsync<IReadOnlyList<IScheduledEventUser>>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/scheduled-events/:event-id/users")
                 .WithFullRatelimit($"GET guilds/{guildId}/scheduled-events/:event-id/users"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IScheduledEvent>>> ListScheduledEventsForGuildAsync
    (
        Snowflake guildId,
        WithUserCountQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new()
        {
            RootUri = $"guilds/{guildId}/scheduled-events"
        };

        if (query.WithUserCount is not null)
        {
            builder.AddParameter("with_user_count", query.WithUserCount.Value.ToString().ToLowerInvariant());
        }

        return await restClient.ExecuteRequestAsync<IReadOnlyList<IScheduledEvent>>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/scheduled-events")
                 .WithFullRatelimit($"GET guilds/{guildId}/scheduled-events"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IScheduledEvent>> ModifyScheduledEventAsync
    (
        Snowflake guildId,
        Snowflake eventId,
        IModifyGuildScheduledEventPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IScheduledEvent>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/scheduled-events/{eventId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/scheduled-events/:event-id")
                 .WithFullRatelimit($"POST guilds/{guildId}/scheduled-events/:event-id")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }
}
