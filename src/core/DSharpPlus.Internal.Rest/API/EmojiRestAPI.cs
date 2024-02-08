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

using Remora.Results;

namespace DSharpPlus.Internal.Rest.API;

// as per https://discord.com/developers/docs/topics/rate-limits, emojis are handled separately from other
// ratelimits. we therefore never count them towards the simple guild limit, and instead specify them as
// 'other' -> emojis/:guild-id so that their erratic behaviour doesn't mess with the rest of our ratelimits.

/// <inheritdoc cref="IEmojiRestAPI"/>
public sealed class EmojiRestAPI(IRestClient restClient)
    : IEmojiRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IEmoji>> CreateGuildEmojiAsync
    (
        Snowflake guildId,
        ICreateGuildEmojiPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IEmoji>
        (
            HttpMethod.Post,
            $"guilds/{guildId}/emojis",
            b => b.WithRoute($"POST emojis/{guildId}")
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteGuildEmojiAsync
    (
        Snowflake guildId,
        Snowflake emojiId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"guilds/{guildId}/emojis/{emojiId}",
            b => b.WithRoute($"DELETE emojis/{guildId}")
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IEmoji>> GetGuildEmojiAsync
    (
        Snowflake guildId,
        Snowflake emojiId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IEmoji>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/emojis/{emojiId}",
            b => b.WithRoute($"GET emojis/{guildId}"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IEmoji>>> ListGuildEmojisAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IEmoji>>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/emojis",
            b => b.WithRoute($"GET emojis/{guildId}"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IEmoji>> ModifyGuildEmojiAsync
    (
        Snowflake guildId,
        Snowflake emojiId,
        IModifyGuildEmojiPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IEmoji>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/emojis/{emojiId}",
            b => b.WithRoute($"GET emojis/{guildId}")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }
}
