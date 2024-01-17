// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Internal.Abstractions.Rest.Responses;
using DSharpPlus.Internal.Rest.Ratelimiting;

using Remora.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IGuildRestAPI"/>
public sealed class GuildRestAPI
(
    IRestClient restClient
)
    : IGuildRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IGuildMember?>> AddGuildMemberAsync
    (
        Snowflake guildId,
        Snowflake userId,
        IAddGuildMemberPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IGuildMember?>
        (
            HttpMethod.Put,
            $"guilds/{guildId}/members/{userId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/members/:user-id")
                 .WithFullRatelimit($"PUT guilds/{guildId}/members/:user-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> AddGuildMemberRoleAsync
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake roleId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Put,
            $"guilds/{guildId}/members/{userId}/roles/{roleId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/members/:user-id/roles/:role-id")
                 .WithFullRatelimit($"PUT guilds/{guildId}/members/:user-id/roles/:role-id")
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> CreateGuildBanAsync
    (
        Snowflake guildId,
        Snowflake userId,
        CreateGuildBanQuery query = default,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new()
        {
            RootUri = $"guilds/{guildId}/bans/{userId}"
        };

        if (query.DeleteMessageSeconds is not null)
        {
            builder.AddParameter("delete_message_seconds", query.DeleteMessageSeconds.Value.ToString(CultureInfo.InvariantCulture));
        }

        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Put,
            builder.Build(),
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/bans/:user-id")
                 .WithFullRatelimit($"PUT guilds/{guildId}/bans/:user-id")
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<BeginGuildPruneResponse>> BeginGuildPruneAsync
    (
        Snowflake guildId,
        IBeginGuildPrunePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Days is < 1 or > 30)
        {
            return new ValidationError("The number of days to prune must be between 1 and 30.");
        }

        return await restClient.ExecuteRequestAsync<BeginGuildPruneResponse>
        (
            HttpMethod.Post,
            $"guilds/{guildId}/prune",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/prune")
                 .WithFullRatelimit($"PUT guilds/{guildId}/prune")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IGuild>> CreateGuildAsync
    (
        ICreateGuildPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.Length is < 2 or > 100)
        {
            return new ValidationError("The name of a guild must be between 2 and 100 characters long.");
        }

        if (payload.AfkTimeout.HasValue && payload.AfkTimeout.Value is not (60 or 300 or 900 or 1800 or 3600))
        {
            return new ValidationError("The AFK timeout of a guild must be either 60, 300, 900, 1800 or 3600 seconds.");
        }

        return await restClient.ExecuteRequestAsync<IGuild>
        (
            HttpMethod.Post,
            $"guilds",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = false,
                        Resource = TopLevelResource.Other,
                        Route = "guilds"
                    }
                 )
                 .WithFullRatelimit($"POST guilds")
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IChannel>> CreateGuildChannelAsync
    (
        Snowflake guildId,
        ICreateGuildChannelPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.Length is < 1 or > 100)
        {
            return new ValidationError("The channel name must be between 1 and 100 characters in length.");
        }

        if (payload.Topic.TryGetNonNullValue(out string? topic) && topic.Length > 1024)
        {
            return new ValidationError("The channel topic must not exceed 1024 characters in length.");
        }

        if (payload.Bitrate.TryGetNonNullValue(out int? bitrate) && bitrate < 8000)
        {
            return new ValidationError("The bitrate of a voice channel must not be below 8000 bits.");
        }

        if (payload.RateLimitPerUser.TryGetNonNullValue(out int? slowmode) && slowmode is < 0 or > 21600)
        {
            return new ValidationError("The slowmode (rate limit per user) of a channel must be between 0 and 21600 seconds.");
        }

        if (payload.DefaultThreadRateLimitPerUser.TryGetNonNullValue(out int? threadSlowmode) && threadSlowmode is < 0 or > 21600)
        {
            return new ValidationError
            (
                "The slowmode (rate limit per user) of threads created in channel must be between 0 and 21600 seconds."
            );
        }

        return await restClient.ExecuteRequestAsync<IChannel>
        (
            HttpMethod.Post,
            $"guilds/{guildId}/channels",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/channels")
                 .WithFullRatelimit($"POST guilds/{guildId}/channels")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IRole>> CreateGuildRoleAsync
    (
        Snowflake guildId,
        ICreateGuildRolePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.HasValue && payload.Name.Value.Length > 100)
        {
            return new ValidationError("A role name cannot exceed 100 characters in length.");
        }

        if (payload.Color.HasValue && payload.Color.Value is < 0 or > 0xFFFFFF)
        {
            return new ValidationError("The role color must be a valid RGB color code.");
        }

        return await restClient.ExecuteRequestAsync<IRole>
        (
            HttpMethod.Post,
            $"guilds/{guildId}/roles",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/roles")
                 .WithFullRatelimit($"POST guilds/{guildId}/roles")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteGuildAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"guilds/{guildId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithFullRatelimit($"DELETE guilds/{guildId}"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteGuildIntegrationAsync
    (
        Snowflake guildId,
        Snowflake integrationId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"guilds/{guildId}/integrations/{integrationId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/integrations/:integration-id")
                 .WithFullRatelimit($"DELETE guilds/{guildId}/integrations/:integration-id")
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteGuildRoleAsync
    (
        Snowflake guildId,
        Snowflake roleId,
        string? reason,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"guilds/{guildId}/roles/{roleId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/roles/:role-id")
                 .WithFullRatelimit($"DELETE guilds/{guildId}/roles/:role-id")
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IGuild>> GetGuildAsync
    (
        Snowflake guildId,
        GetGuildQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new()
        {
            RootUri = $"guilds/{guildId}"
        };

        if (query.WithCounts.HasValue)
        {
            builder.AddParameter("with_counts", query.WithCounts.Value.ToString().ToLowerInvariant());
        }

        return await restClient.ExecuteRequestAsync<IGuild>
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
                 .WithFullRatelimit($"GET guilds/{guildId}"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IBan>> GetGuildBanAsync
    (
        Snowflake guildId,
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IBan>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/bans/{userId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/bans/:user-id")
                 .WithFullRatelimit($"GET guilds/{guildId}/bans/:user-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IBan>>> GetGuildBansAsync
    (
        Snowflake guildId,
        PaginatedQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new()
        {
            RootUri = $"guilds/{guildId}/bans"
        };

        if (query.Limit.HasValue)
        {
            if (query.Limit.Value is < 1 or > 1000)
            {
                return new ValidationError("The amount of bans to request must be between 1 and 1000.");
            }

            builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.Before.HasValue)
        {
            builder.AddParameter("before", query.Before.Value.ToString());
        }

        if (query.After.HasValue)
        {
            builder.AddParameter("after", query.After.Value.ToString());
        }

        return await restClient.ExecuteRequestAsync<IReadOnlyList<IBan>>
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
                 .WithRoute($"guilds/{guildId}/bans")
                 .WithFullRatelimit($"GET guilds/{guildId}/bans"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IChannel>>> GetGuildChannelsAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IChannel>>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/channels",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/channels")
                 .WithFullRatelimit($"GET guilds/{guildId}/channels"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IIntegration>>> GetGuildIntegrationsAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IIntegration>>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/integrations",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/integrations")
                 .WithFullRatelimit($"GET guilds/{guildId}/integrations"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IInvite>>> GetGuildInvitesAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IInvite>>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/invites",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/invites")
                 .WithFullRatelimit($"GET guilds/{guildId}/invites"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildMember>> GetGuildMemberAsync
    (
        Snowflake guildId,
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IGuildMember>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/members/{userId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/members/:user-id")
                 .WithFullRatelimit($"GET guilds/{guildId}/members/:user-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IOnboarding>> GetGuildOnboardingAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IOnboarding>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/onboarding",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/onboarding")
                 .WithFullRatelimit($"GET guilds/{guildId}/onboarding"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildPreview>> GetGuildPreviewAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IGuildPreview>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/preview",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/preview")
                 .WithFullRatelimit($"GET guilds/{guildId}/preview"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<GetGuildPruneCountResponse>> GetGuildPruneCountAsync
    (
        Snowflake guildId,
        GetGuildPruneCountQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new()
        {
            RootUri = $"guilds/{guildId}/prune"
        };

        if (query.Days.HasValue)
        {
            if (query.Days is < 1 or > 30)
            {
                return new ValidationError("The number of days to prune must be between 1 and 30.");
            }

            builder.AddParameter("days", query.Days.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.IncludeRoles is not null)
        {
            builder.AddParameter("include_roles", query.IncludeRoles);
        }

        return await restClient.ExecuteRequestAsync<GetGuildPruneCountResponse>
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
                 .WithRoute($"guilds/{guildId}/prune")
                 .WithFullRatelimit($"GET guilds/{guildId}/prune"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IRole>>> GetGuildRolesAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IRole>>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/roles",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/roles")
                 .WithFullRatelimit($"GET guilds/{guildId}/roles"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IInvite>> GetGuildVanityUrlAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IInvite>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/vanity-url",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/vanity-url")
                 .WithFullRatelimit($"GET guilds/{guildId}/vanity-url"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IVoiceRegion>>> GetGuildVoiceRegionsAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IVoiceRegion>>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/regions",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/regions")
                 .WithFullRatelimit($"GET guilds/{guildId}/regions"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IWelcomeScreen>> GetGuildWelcomeScreenAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IWelcomeScreen>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/welcome-screen",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/welcome-screen")
                 .WithFullRatelimit($"GET guilds/{guildId}/welcome-screen"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildWidget>> GetGuildWidgetAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IGuildWidget>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/widget.json",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/widget.json")
                 .WithFullRatelimit($"GET guilds/{guildId}/widget.json"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<Stream>> GetGuildWidgetImageAsync
    (
        Snowflake guildId,
        GetGuildWidgetImageQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Get,
            $"guilds/{guildId}/widget.json",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/widget.json")
                 .WithFullRatelimit($"GET guilds/{guildId}/widget.json"),
            info,
            ct
        );

        return response.Map(message => message.Content.ReadAsStream());
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildWidgetSettings>> GetGuildWidgetSettingsAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IGuildWidgetSettings>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/widget",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/widget")
                 .WithFullRatelimit($"GET guilds/{guildId}/widget"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ListActiveGuildThreadsResponse>> ListActiveGuildThreadsAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<ListActiveGuildThreadsResponse>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/threads/active",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/threads/active")
                 .WithFullRatelimit($"GET guilds/{guildId}/threads/active"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IGuildMember>>> ListGuildMembersAsync
    (
        Snowflake guildId,
        ForwardsPaginatedQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new()
        {
            RootUri = $"guilds/{guildId}/members"
        };

        if (query.Limit is not null)
        {
            if (query.Limit is < 1 or > 1000)
            {
                return new ValidationError("The amount of members to query at once must be between 1 and 1000.");
            }

            builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.After is not null)
        {
            builder.AddParameter("after", query.After.Value.ToString());
        }

        return await restClient.ExecuteRequestAsync<IReadOnlyList<IGuildMember>>
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
                 .WithRoute($"guilds/{guildId}/members")
                 .WithFullRatelimit($"GET guilds/{guildId}/members"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildMember>> ModifyCurrentMemberAsync
    (
        Snowflake guildId,
        IModifyCurrentMemberPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IGuildMember>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/members/@me",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/members/@me")
                 .WithFullRatelimit($"PATCH guilds/{guildId}/members/@me")
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> ModifyCurrentUserVoiceStateAsync
    (
        Snowflake guildId,
        IModifyCurrentUserVoiceStatePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/voice-states/@me",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/voice-states/@me")
                 .WithFullRatelimit($"PATCH guilds/{guildId}/voice-states/@me")
                 .WithPayload(payload),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IGuild>> ModifyGuildAsync
    (
        Snowflake guildId,
        IModifyGuildPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.HasValue && payload.Name.Value.Length is < 2 or > 100)
        {
            return new ValidationError("The name of a guild must be between 2 and 100 characters long.");
        }

        if (payload.AfkTimeout.HasValue && payload.AfkTimeout.Value is not (60 or 300 or 900 or 1800 or 3600))
        {
            return new ValidationError("The AFK timeout of a guild must be either 60, 300, 900, 1800 or 3600 seconds.");
        }

        return await restClient.ExecuteRequestAsync<IGuild>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithFullRatelimit($"POST guilds")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> ModifyGuildChannelPositionsAsync
    (
        Snowflake guildId,
        IReadOnlyList<IModifyGuildChannelPositionsPayload> payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/channels",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/channels")
                 .WithFullRatelimit($"PATCH guilds/{guildId}/channels")
                 .WithPayload(payload),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildMember>> ModifyGuildMemberAsync
    (
        Snowflake guildId,
        Snowflake userId,
        IModifyGuildMemberPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Nickname.TryGetNonNullValue(out string? nick) && nick.Length > 32)
        {
            return new ValidationError("Nicknames of guild members cannot be longer than 32 characters.");
        }

        return await restClient.ExecuteRequestAsync<IGuildMember>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/members/{userId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/members/:user-id")
                 .WithFullRatelimit($"PATCH guilds/{guildId}/members/:user-id")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<DiscordMfaLevel>> ModifyGuildMFALevelAsync
    (
        Snowflake guildId,
        IModifyGuildMfaLevelPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<DiscordMfaLevel>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/mfa",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/mfa")
                 .WithFullRatelimit($"PATCH guilds/{guildId}/mfa")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IOnboarding>> ModifyGuildOnboardingAsync
    (
        Snowflake guildId,
        IModifyGuildOnboardingPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IOnboarding>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/onboarding",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/onboarding")
                 .WithFullRatelimit($"PATCH guilds/{guildId}/onboarding")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IRole>> ModifyGuildRoleAsync
    (
        Snowflake guildId,
        Snowflake roleId,
        IModifyGuildRolePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.TryGetNonNullValue(out string? name) && name.Length > 100)
        {
            return new ValidationError("The name of a role cannot exceed 100 characters in length.");
        }

        if (payload.Color.TryGetNonNullValue(out int? value) && value is < 0 or > 0xFFFFFF)
        {
            return new ValidationError("The color of a role must be a valid RGB color code.");
        }

        return await restClient.ExecuteRequestAsync<IRole>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/roles/{roleId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/roles/:role-id")
                 .WithFullRatelimit($"PATCH guilds/{guildId}/roles/:role-id")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IRole>>> ModifyGuildRolePositionsAsync
    (
        Snowflake guildId,
        IReadOnlyList<IModifyGuildRolePositionsPayload> payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IRole>>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/roles",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/roles")
                 .WithFullRatelimit($"PATCH guilds/{guildId}/roles")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IWelcomeScreen>> ModifyGuildWelcomeScreenAsync
    (
        Snowflake guildId,
        IModifyGuildWelcomeScreenPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IWelcomeScreen>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/welcome-screen",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/welcome-screen")
                 .WithFullRatelimit($"PATCH guilds/{guildId}/welcome-screen")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildWidget>> ModifyGuildWidgetAsync
    (
        Snowflake guildId,
        IGuildWidgetSettings settings,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IGuildWidget>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/widget",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/widget")
                 .WithFullRatelimit($"PATCH guilds/{guildId}/widget")
                 .WithPayload(settings)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> ModifyUserVoiceStateAsync
    (
        Snowflake guildId,
        Snowflake userId,
        IModifyUserVoiceStatePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/voice-states/{userId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/voice-states/:user-id")
                 .WithFullRatelimit($"PATCH guilds/{guildId}/voice-states/:user-id")
                 .WithPayload(payload),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> RemoveGuildBanAsync
    (
        Snowflake guildId,
        Snowflake userId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"guilds/{guildId}/bans/{userId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/bans/:user-id")
                 .WithFullRatelimit($"DELETE guilds/{guildId}/bans/:user-id")
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> RemoveGuildMemberAsync
    (
        Snowflake guildId,
        Snowflake userId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"guilds/{guildId}/members/{userId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/members/:user-id")
                 .WithFullRatelimit($"DELETE guilds/{guildId}/members/:user-id")
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> RemoveGuildMemberRoleAsync
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake roleId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"guilds/{guildId}/members/{userId}/roles/{roleId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/members/:user-id/roles/:role-id")
                 .WithFullRatelimit($"DELETE guilds/{guildId}/members/:user-id/roles/:role-id")
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IGuildMember>>> SearchGuildMembersAsync
    (
        Snowflake guildId,
        SearchGuildMembersQuery query,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new()
        {
            RootUri = $"guilds/{guildId}/members/search"
        };

        builder.AddParameter("query", query.Query);

        if (query.Limit is not null)
        {
            if (query.Limit is < 1 or > 1000)
            {
                return new ValidationError("The amount of members to request must be between 1 and 1000.");
            }

            builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        return await restClient.ExecuteRequestAsync<IReadOnlyList<IGuildMember>>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/members/search",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/members/search")
                 .WithFullRatelimit($"DELETE guilds/{guildId}/members/search"),
            info,
            ct
        );
    }
}
