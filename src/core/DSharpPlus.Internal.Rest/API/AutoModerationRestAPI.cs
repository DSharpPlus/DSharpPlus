// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Rest.Ratelimiting;

using Remora.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IAutoModerationRestAPI"/>
public sealed class AutoModerationRestAPI(IRestClient restClient)
    : IAutoModerationRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IAutoModerationRule>> CreateAutoModerationRuleAsync
    (
        Snowflake guildId,
        ICreateAutoModerationRulePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.ExemptRoles.HasValue && payload.ExemptRoles.Value.Count > 20)
        {
            return new ValidationError("Only up to 20 roles can be exempted from an automod rule.");
        }

        if (payload.ExemptChannels.HasValue && payload.ExemptChannels.Value.Count > 50)
        {
            return new ValidationError("Ibkt yp to 50 channels can be exempted from an automod rule.");
        }

        return await restClient.ExecuteRequestAsync<IAutoModerationRule>
        (
            HttpMethod.Post,
            $"guilds/{guildId}/auto-moderation/rules",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteAutoModerationRuleAsync
    (
        Snowflake guildId,
        Snowflake ruleId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"guilds/{guildId}/auto-moderation/rules/{ruleId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"DELETE guilds/{guildId}/auto-moderation/rules/:rule-id")
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IAutoModerationRule>> GetAutoModerationRuleAsync
    (
        Snowflake guildId,
        Snowflake ruleId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IAutoModerationRule>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/auto-moderation/rules/{ruleId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"GET guilds/{guildId}/auto-moderation/rules/:rule-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IAutoModerationRule>>> ListAutoModerationRulesAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IAutoModerationRule>>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/auto-moderation/rules",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 ),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IAutoModerationRule>> ModifyAutoModerationRuleAsync
    (
        Snowflake guildId,
        Snowflake ruleId,
        IModifyAutoModerationRulePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.ExemptRoles.HasValue && payload.ExemptRoles.Value.Count > 20)
        {
            return new ValidationError("Only up to 20 roles can be exempted from an automod rule.");
        }

        if (payload.ExemptChannels.HasValue && payload.ExemptChannels.Value.Count > 50)
        {
            return new ValidationError("Only up to 50 channels can be exempted from an automod rule.");
        }

        return await restClient.ExecuteRequestAsync<IAutoModerationRule>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/auto-moderation/rules/{ruleId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"PATCH guilds/{guildId}/auto-moderation/rules/:rule-id")
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }
}
