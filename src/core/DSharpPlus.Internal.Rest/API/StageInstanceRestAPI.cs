// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Rest.Ratelimiting;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IStageInstanceRestAPI"/>
public sealed class StageInstanceRestAPI(IRestClient restClient)
    : IStageInstanceRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IStageInstance>> CreateStageInstanceAsync
    (
        ICreateStageInstancePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Topic.Length > 120)
        {
            return new ValidationError("The length of a stage topic cannot exceed 120 characters.");
        }

        return await restClient.ExecuteRequestAsync<IStageInstance>
        (
            HttpMethod.Post,
            $"stage-instances",
            b => b.WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteStageInstanceAsync
    (
        Snowflake channelId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"stage-instances/{channelId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"DELETE stage-instances/{channelId}")
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IStageInstance>> GetStageInstanceAsync
    (
        Snowflake channelId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IStageInstance>
        (
            HttpMethod.Get,
            $"stage-instances/{channelId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"GET stage-instances/{channelId}"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IStageInstance>> ModifyStageInstanceAsync
    (
        Snowflake channelId,
        IModifyStageInstancePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Topic.HasValue && payload.Topic.Value.Length > 120)
        {
            return new ValidationError("The length of a stage topic cannot exceed 120 characters.");
        }

        return await restClient.ExecuteRequestAsync<IStageInstance>
        (
            HttpMethod.Patch,
            $"stage-instances/{channelId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"PATCH stage-instances/{channelId}")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }
}
