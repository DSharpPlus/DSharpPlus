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

/// <inheritdoc cref="IGuildTemplateRestAPI"/>
public sealed class GuildTemplateRestAPI
(
    IRestClient restClient
)
    : IGuildTemplateRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IGuild>> CreateGuildFromGuildTemplateAsync
    (
        string templateCode,
        ICreateGuildFromGuildTemplatePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.Length is < 2 or > 100)
        {
            return new ValidationError("The length of a guild name must be between 2 and 100 characters.");
        }

        return await restClient.ExecuteRequestAsync<IGuild>
        (
            HttpMethod.Post,
            $"guilds/templates/{templateCode}",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = true,
                        Resource = TopLevelResource.Other,
                        Route = $"guilds/templates/{templateCode}"
                    }
                 )
                 .WithFullRatelimit($"POST guilds/templates/{templateCode}")
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ITemplate>> CreateGuildTemplateAsync
    (
        Snowflake guildId,
        ICreateGuildTemplatePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.Length > 100)
        {
            return new ValidationError("The length of a template name must be between 2 and 100 characters.");
        }

        if (payload.Description.TryGetNonNullValue(out string? description) && description.Length > 120)
        {
            return new ValidationError("The description of a template name must not exceed 120 characters.");
        }

        return await restClient.ExecuteRequestAsync<ITemplate>
        (
            HttpMethod.Post,
            $"guilds/{guildId}/templates",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/templates")
                 .WithFullRatelimit($"POST guilds/{guildId}/templates")
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ITemplate>> DeleteGuildTemplateAsync
    (
        Snowflake guildId,
        string templateCode,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<ITemplate>
        (
            HttpMethod.Delete,
            $"guilds/{guildId}/templates/{templateCode}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/templates/:template-code")
                 .WithFullRatelimit($"DELETE guilds/{guildId}/templates/:template-code"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ITemplate>> GetGuildTemplateAsync
    (
        string templateCode,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<ITemplate>
        (
            HttpMethod.Get,
            $"guilds/templates/{templateCode}",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = true,
                        Resource = TopLevelResource.Other,
                        Route = $"guilds/templates/{templateCode}"
                    }
                 )
                 .WithFullRatelimit($"POST guilds/templates/{templateCode}"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<ITemplate>>> GetGuildTemplatesAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<ITemplate>>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/templates",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/templates")
                 .WithFullRatelimit($"DELETE guilds/{guildId}/templates"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ITemplate>> ModifyGuildTemplateAsync
    (
        Snowflake guildId,
        string templateCode,
        IModifyGuildTemplatePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.HasValue && payload.Name.Value.Length > 100)
        {
            return new ValidationError("The length of a template name must be between 2 and 100 characters.");
        }

        if (payload.Description.TryGetNonNullValue(out string? description) && description.Length > 120)
        {
            return new ValidationError("The description of a template name must not exceed 120 characters.");
        }

        return await restClient.ExecuteRequestAsync<ITemplate>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/templates/{templateCode}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/templates/:template-code")
                 .WithFullRatelimit($"PATCH guilds/{guildId}/templates/:template-code")
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ITemplate>> SyncGuildTemplateAsync
    (
        Snowflake guildId,
        string templateCode,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<ITemplate>
        (
            HttpMethod.Put,
            $"guilds/{guildId}/templates/{templateCode}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"guilds/{guildId}/templates/:template-code")
                 .WithFullRatelimit($"PUT guilds/{guildId}/templates/:template-code"),
            info,
            ct
        );
    }
}
