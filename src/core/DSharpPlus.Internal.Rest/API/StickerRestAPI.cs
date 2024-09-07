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
using DSharpPlus.Internal.Abstractions.Rest.Responses;
using DSharpPlus.Internal.Rest.Ratelimiting;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IStickerRestAPI"/>
public sealed class StickerRestAPI(IRestClient restClient)
    : IStickerRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<ISticker>> CreateGuildStickerAsync
    (
        Snowflake guildId,
        ICreateGuildStickerPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.Length is < 2 or > 30)
        {
            return new ValidationError("Sticker names must be between 2 and 30 characters in length.");
        }

        if (!string.IsNullOrWhiteSpace(payload.Description) && payload.Description.Length is < 2 or > 100)
        {
            return new ValidationError("Sticker descriptions must be either empty or between 2 and 100 characters in length.");
        }

        if (payload.Tags.Length > 200)
        {
            return new ValidationError("Sticker tags must not exceed 200 characters in length.");
        }

        return await restClient.ExecuteMultipartPayloadRequestAsync<ISticker>
        (
            HttpMethod.Post,
            $"guilds/{guildId}/stickers",
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId)
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteGuildStickerAsync
    (
        Snowflake guildId,
        Snowflake stickerId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"guilds/{guildId}/stickers/{stickerId}",
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId)
                 .WithRoute($"DELETE guilds/{guildId}/stickers/:sticker-id")
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ISticker>> GetGuildStickerAsync
    (
        Snowflake guildId,
        Snowflake stickerId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<ISticker>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/stickers/{stickerId}",
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId)
                 .WithRoute($"GET guilds/{guildId}/stickers/:sticker-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ISticker>> GetStickerAsync
    (
        Snowflake stickerId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<ISticker>
        (
            HttpMethod.Get,
            $"stickers/{stickerId}",
            b => b.WithRoute("GET stickers/:sticker-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IStickerPack>> GetStickerPackAsync
    (
        Snowflake packId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IStickerPack>
        (
            HttpMethod.Get,
            $"sticker-packs/{packId}",
            b => b.WithRoute("GET sticker-packs/:pack-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<ISticker>>> ListGuildStickersAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<ISticker>>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/stickers",
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId)
                 .WithRoute($"GET guilds/{guildId}/stickers"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ListStickerPacksResponse>> ListStickerPacksAsync
    (
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<ListStickerPacksResponse>
        (
            HttpMethod.Get,
            "sticker-packs",
            b => b.WithRoute("GET sticker-packs"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ISticker>> ModifyGuildStickerAsync
    (
        Snowflake guildId,
        Snowflake stickerId,
        IModifyGuildStickerPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.HasValue && payload.Name.Value.Length is < 2 or > 30)
        {
            return new ValidationError("Sticker names must be between 2 and 30 characters in length.");
        }

        if
        (
            payload.Description.TryGetNonNullValue(out string? description)
            && !string.IsNullOrWhiteSpace(description)
            && description.Length is < 2 or > 100
        )
        {
            return new ValidationError("Sticker descriptions must be either empty or between 2 and 100 characters in length.");
        }

        if (payload.Tags.HasValue && payload.Tags.Value.Length > 200)
        {
            return new ValidationError("Sticker tags must not exceed 200 characters in length.");
        }

        return await restClient.ExecuteRequestAsync<ISticker>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/stickers/{stickerId}",
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId)
                 .WithRoute($"PATCH guilds/{guildId}/stickers/:sticker-id")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }
}
