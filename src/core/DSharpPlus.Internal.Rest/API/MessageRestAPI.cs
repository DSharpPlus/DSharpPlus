// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IMessageRestAPI"/>
public sealed class MessageRestAPI(IRestClient restClient)
    : IMessageRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result> BulkDeleteMessagesAsync
    (
        Snowflake channelId,
        IBulkDeleteMessagesPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Post,
            $"channels/{channelId}/messages/bulk-delete",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IMessage>> CreateMessageAsync
     (
        Snowflake channelId,
        ICreateMessagePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if
        (
            !(payload.Content.HasValue || payload.Embeds.HasValue || payload.StickerIds.HasValue
                || payload.Components.HasValue || payload.Files is not null)
        )
        {
            return new ValidationError
            (
                "At least one of Content, Embeds, StickerIds, Components or Files must be sent."
            );
        }

        if (payload.Content.HasValue && payload.Content.Value.Length > 2000)
        {
            return new ValidationError("The content of a message cannot exceed 2000 characters.");
        }

        if (payload.Nonce.HasValue && payload.Nonce.Value.Length > 25)
        {
            return new ValidationError("The nonce of a message cannot exceed 25 characters.");
        }

        if (payload.Embeds.HasValue && payload.Embeds.Value.Count > 10)
        {
            return new ValidationError("Only up to 10 embeds can be sent with a message.");
        }

        if (payload.StickerIds.HasValue && payload.StickerIds.Value.Count > 3)
        {
            return new ValidationError("Only up to 3 stickers can be sent with a message.");
        }

        if (payload.Poll.TryGetNonNullValue(out ICreatePoll? poll))
        {
            if (poll.Question.Text is { HasValue: true, Value.Length: > 300 })
            {
                return new ValidationError("The poll must specify a question that cannot exceed 300 characters.");
            }

            if (poll.Answers.Any(answer => answer.PollMedia.Text is { HasValue: true, Value.Length: > 55 }))
            {
                return new ValidationError("The answer text of a poll cannot exceed 55 characters.");
            }
        }

        return await restClient.ExecuteRequestAsync<IMessage>
        (
            HttpMethod.Post,
            $"channels/{channelId}/messages",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> CreateReactionAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        string emoji,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Put,
            $"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"PUT channels/{channelId}/messages/:message-id/reactions/:emoji/@me"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IMessage>> CrosspostMessageAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IMessage>
        (
            HttpMethod.Post,
            $"channels/{channelId}/messages/{messageId}/crosspost",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"POST channels/{channelId}/messages/:message-id/crosspost"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteAllReactionsAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"channels/{channelId}/messages/{messageId}/reactions",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"DELETE channels/{channelId}/messages/:message-id/reactions"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteAllReactionsForEmojiAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        string emoji,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"channels/{channelId}/messages/{messageId}/reactions/{emoji}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"DELETE channels/{channelId}/messages/:message-id/reactions/:emoji"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteMessageAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"channels/{channelId}/messages/{messageId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"DELETE channels/{channelId}/messages/:message-id")
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteOwnReactionAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        string emoji,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"DELETE channels/{channelId}/messages/:message-id/reactions/:emoji/@me"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteUserReactionAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        Snowflake userId,
        string emoji,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"channels/{channelId}/messages/{messageId}/reactions/{emoji}/{userId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"DELETE channels/{channelId}/messages/:message-id/reactions/:emoji/:user-id"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IMessage>> EditMessageAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        IEditMessagePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Content.TryGetNonNullValue(out string? content) && content.Length > 2000)
        {
            return new ValidationError("The content of a message cannot exceed 2000 characters.");
        }

        if (payload.Embeds.TryGetNonNullValue(out IReadOnlyList<IEmbed>? embeds) && embeds.Count > 10)
        {
            return new ValidationError("A message can only have up to 10 embeds.");
        }

        return await restClient.ExecuteRequestAsync<IMessage>
        (
            HttpMethod.Patch,
            $"channels/{channelId}/messages/{messageId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"PATCH channels/{channelId}/messages/:message-id")
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IMessage>> GetChannelMessageAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IMessage>
        (
            HttpMethod.Get,
            $"channels/{channelId}/messages/{messageId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"GET channels/{channelId}/messages/:message-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IMessage>>> GetChannelMessagesAsync
    (
        Snowflake channelId,
        GetChannelMessagesQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (query.Limit is not null and (< 1 or > 100))
        {
            return new ValidationError("The limit for messages to request at once must be between 1 and 100.");
        }

        QueryBuilder builder = new($"channels/{channelId}/messages");

        if (query.Limit is not null)
        {
            _ = builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.Around is not null)
        {
            _ = builder.AddParameter("around", query.Around.Value.ToString());
        }
        else if (query.Before is not null)
        {
            _ = builder.AddParameter("before", query.Before.Value.ToString());
        }
        else if (query.After is not null)
        {
            _ = builder.AddParameter("after", query.After.Value.ToString());
        }

        return await restClient.ExecuteRequestAsync<IReadOnlyList<IMessage>>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IUser>>> GetReactionsAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        string emoji,
        GetReactionsQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (query.Limit is not null and (< 1 or > 100))
        {
            return new ValidationError("The limit of reactions to request must be between 1 and 100.");
        }

        QueryBuilder builder = new($"channels/{channelId}/messages/{messageId}/reactions/{emoji}");

        if (query.After is not null)
        {
            _ = builder.AddParameter("after", query.After.Value.ToString());
        }

        if (query.Limit is not null)
        {
            _ = builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.Type is not null)
        {
            _ = builder.AddParameter("type", ((int)query.Type.Value).ToString(CultureInfo.InvariantCulture));
        }

        return await restClient.ExecuteRequestAsync<IReadOnlyList<IUser>>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"GET channels/{channelId}/messages/:message-id/reactions/:emoji"),
            info,
            ct
        );
    }
}
