// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Rest.Ratelimiting;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc/>
public sealed class VoiceRestAPI(IRestClient restClient)
    : IVoiceRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IVoiceState>> GetCurrentUserVoiceStateAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IVoiceState>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/voice-states/@me",
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IVoiceState>> GetUserVoiceStateAsync
    (
        Snowflake guildId,
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IVoiceState>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/voice-states/{userId}",
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId)
                 .WithRoute($"GET guilds/{guildId}/voice-states/:user-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IVoiceRegion>>> ListVoiceRegionsAsync
    (
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IVoiceRegion>>
        (
            HttpMethod.Get,
            $"voice/regions",
            b => b.WithRoute("GET voice/regions"),
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
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId)
                 .WithPayload(payload),
            info,
            ct
        );

        return (Result)response;
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
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId)
                 .WithRoute($"PATCH guilds/{guildId}/voice-states/:user-id")
                 .WithPayload(payload),
            info,
            ct
        );

        return (Result)response;
    }
}
