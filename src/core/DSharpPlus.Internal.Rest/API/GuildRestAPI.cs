// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

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
    public async ValueTask<Result<BeginGuildPruneResponse>> BeginGuildPruneAsync(Snowflake guildId, BeginGuildPruneQuery query = default, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IGuild>> CreateGuildAsync(ICreateGuildPayload payload, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IChannel>> CreateGuildChannelAsync(Snowflake guildId, ICreateGuildChannelPayload payload, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IRole>> CreateGuildRoleAsync(Snowflake guildId, ICreateGuildRolePayload payload, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteGuildAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteGuildIntegrationAsync(Snowflake guildId, Snowflake integrationId, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteRoleAsync(Snowflake guildId, Snowflake roleId, string? reason, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IGuild>> GetGuildAsync(Snowflake guildId, GetGuildQuery query = default, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IBan>> GetGuildBanAsync(Snowflake guildId, Snowflake userId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IBan>>> GetGuildBansAsync(Snowflake guildId, PaginatedQuery query = default, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IChannel>>> GetGuildChannelsAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IIntegration>>> GetGuildIntegrationsAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IInvite>>> GetGuildInvitesAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildMember>> GetGuildMemberAsync(Snowflake guildId, Snowflake userId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IOnboarding>> GetGuildOnboardingAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildPreview>> GetGuildPreviewAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<int>> GetGuildPruneCountAsync(Snowflake guildId, GetGuildPruneCountQuery query = default, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IRole>>> GetGuildRolesAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IInvite>> GetGuildVanityUrlAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IVoiceRegion>>> GetGuildVoiceRegionsAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IWelcomeScreen>> GetGuildWelcomeScreenAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildWidget>> GetGuildWidgetAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<Stream>> GetGuildWidgetImageAsync(Snowflake guildId, string? style = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildWidgetSettings>> GetGuildWidgetSettingsAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<ListActiveGuildThreadsResponse>> ListActiveGuildThreadsAsync(Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IGuildMember>>> ListGuildMembersAsync(Snowflake guildId, ForwardsPaginatedQuery query = default, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildMember>> ModifyCurrentMemberAsync(Snowflake guildId, IModifyCurrentMemberPayload payload, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result> ModifyCurrentUserVoiceStateAsync(Snowflake guildId, IModifyCurrentUserVoiceStatePayload payload, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IGuild>> ModifyGuildAsync(Snowflake guildId, IModifyGuildPayload payload, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result> ModifyGuildChannelPositionsAsync(Snowflake guildId, IReadOnlyList<IModifyGuildChannelPositionsPayload> payload, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildMember>> ModifyGuildMemberAsync(Snowflake guildId, Snowflake userId, IModifyGuildMemberPayload payload, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<DiscordMfaLevel>> ModifyGuildMFALevelAsync(Snowflake guildId, IModifyGuildMfaLevelPayload payload, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IOnboarding>> ModifyGuildOnboardingAsync(Snowflake guildId, IModifyGuildOnboardingPayload payload, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IRole>> ModifyGuildRoleAsync(Snowflake guildId, Snowflake roleId, IModifyGuildRolePayload payload, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IRole>>> ModifyGuildRolePositionsAsync(Snowflake guildId, IReadOnlyList<IModifyGuildRolePositionsPayload> payload, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IWelcomeScreen>> ModifyGuildWelcomeScreenAsync(Snowflake guildId, IModifyGuildWelcomeScreenPayload payload, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildWidget>> ModifyGuildWidgetSettingsAsync(Snowflake guildId, IGuildWidgetSettings settings, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result> ModifyUserVoiceStateAsync(Snowflake guildId, Snowflake userId, IModifyUserVoiceStatePayload payload, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result> RemoveGuildBanAsync(Snowflake guildId, Snowflake userId, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result> RemoveGuildMemberAsync(Snowflake guildId, Snowflake userId, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result> RemoveGuildMemberRoleAsync(Snowflake guildId, Snowflake userId, Snowflake roleId, string? reason = null, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IGuildMember>>> SearchGuildMembersAsync(Snowflake guildId, string queryString, SearchGuildMembersQuery query = default, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
}
