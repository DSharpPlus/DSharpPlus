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
using DSharpPlus.Internal.Abstractions.Rest.Responses;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Rest.API;

// as per https://discord.com/developers/docs/topics/rate-limits, emojis are handled separately from other
// ratelimits. we therefore never count them towards the simple guild limit, and instead specify them as
// 'other' -> emojis/:guild-id so that their erratic behaviour doesn't mess with the rest of our ratelimits.
//
// application emojis kinda muddy the water here, but we'll just assume they're well-behaved for lack of actual docs.

/// <inheritdoc cref="IEmojiRestAPI"/>
public sealed class EmojiRestAPI(IRestClient restClient)
    : IEmojiRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IEmoji>> CreateApplicationEmojiAsync
    (
        Snowflake applicationId,
        ICreateApplicationEmojiPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IEmoji>
        (
            HttpMethod.Post,
            $"applications/{applicationId}/emojis",
            b => b.WithRoute($"POST applications/:application-id/emojis")
                 .WithPayload(payload),
            info,
            ct
        );
    }

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
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteApplicationEmojiAsync
    (
        Snowflake applicationId,
        Snowflake emojiId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"applications/{applicationId}/emojis/{emojiId}",
            b => b.WithRoute($"DELETE applications/:application-id/emojis/:emoji-id"),
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
        return await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"guilds/{guildId}/emojis/{emojiId}",
            b => b.WithRoute($"DELETE emojis/{guildId}")
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IEmoji>> GetApplicationEmojiAsync
    (
        Snowflake applicationId,
        Snowflake emojiId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IEmoji>
        (
            HttpMethod.Get,
            $"applications/{applicationId}/emojis/{emojiId}",
            b => b.WithRoute($"GET applications/:application-id/emojis/:emoji-id"),
            info,
            ct
        );
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
    public async ValueTask<Result<ListApplicationEmojisResponse>> ListApplicationEmojisAsync
    (
        Snowflake applicationId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<ListApplicationEmojisResponse>
        (
            HttpMethod.Get,
            $"applications/{applicationId}/emojis",
            b => b.WithRoute($"GET applications/:application-id/emojis"),
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
    public async ValueTask<Result<IEmoji>> ModifyApplicationEmojiAsync
    (
        Snowflake applicationId,
        Snowflake emojiId,
        IModifyApplicationEmojiPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IEmoji>
        (
            HttpMethod.Patch,
            $"applications/{applicationId}/emojis/{emojiId}",
            b => b.WithRoute($"PATCH applications/:application-id/emojis/:emoji-id")
                 .WithPayload(payload),
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
            b => b.WithRoute($"PATCH emojis/{guildId}")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }
}
