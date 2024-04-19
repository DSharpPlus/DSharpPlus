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

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Internal.Abstractions.Rest.Responses;
using DSharpPlus.Internal.Rest.Ratelimiting;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IChannelRestAPI"/>
public sealed class ChannelRestAPI(IRestClient restClient)
    : IChannelRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result> AddThreadMemberAsync
    (
        Snowflake threadId,
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Put,
            $"channels/{threadId}/thread-members/{userId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, threadId)
                 .WithRoute($"PUT channels/{threadId}/thread-members/:user-id"),
            info,
            ct
        );

        return (Result)response;
    }

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
    public async ValueTask<Result<IInvite>> CreateChannelInviteAsync
    (
        Snowflake channelId,
        ICreateChannelInvitePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.MaxAge.HasValue && payload.MaxAge.Value is < 0 or > 604800)
        {
            return new ValidationError
            (
                "The maximum age of an invite must be between 0 and 604800 seconds. A maximum age of 0 makes it never expire."
            );
        }

        if (payload.MaxUses.HasValue && payload.MaxUses.Value is < 0 or > 100)
        {
            return new ValidationError
            (
                "The maximum use count of an invite must be between 0 and 100. A value of 0 makes it unlimited."
            );
        }

        if (payload.TargetType.TryGetNonNullValue(out DiscordInviteTargetType type))
        {
            if (type == DiscordInviteTargetType.Stream && !payload.TargetUserId.HasValue)
            {
                return new ValidationError("A target type of Stream requires a streaming user as target.");
            }

            if (type == DiscordInviteTargetType.EmbeddedApplication && !payload.TargetApplicationId.HasValue)
            {
                return new ValidationError("A target type of EmbeddedApplication requires an application as target.");
            }
        }

        return await restClient.ExecuteRequestAsync<IInvite>
        (
            HttpMethod.Post,
            $"channels/{channelId}/invites",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
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
    public async ValueTask<Result<IChannel>> DeleteChannelAsync
    (
        Snowflake channelId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IChannel>
        (
            HttpMethod.Delete,
            $"channels/{channelId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteChannelPermissionAsync
    (
        Snowflake channelId,
        Snowflake overwriteId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"channels/{channelId}/permissions/{overwriteId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithAuditLogReason(reason),
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
    public async ValueTask<Result> EditChannelPermissionsAsync
    (
        Snowflake channelId,
        Snowflake overwriteId,
        IEditChannelPermissionsPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Put,
            $"channels/{channelId}/permissions/{overwriteId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"PUT channels/{channelId}/permissions/:overwrite-id")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
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
    public async ValueTask<Result<IFollowedChannel>> FollowAnnouncementChannelAsync
    (
        Snowflake channelId,
        IFollowAnnouncementChannelPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IFollowedChannel>
        (
            HttpMethod.Post,
            $"channels/{channelId}/followers",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IChannel>> GetChannelAsync
    (
        Snowflake channelId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IChannel>
        (
            HttpMethod.Get,
            $"channels/{channelId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IInvite>>> GetChannelInvitesAsync
    (
        Snowflake channelId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IInvite>>
        (
            HttpMethod.Get,
            $"channels/{channelId}/invites",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId),
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
    public async ValueTask<Result<IReadOnlyList<IMessage>>> GetPinnedMessagesAsync
    (
        Snowflake channelId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IMessage>>
        (
            HttpMethod.Get,
            $"channels/{channelId}/pins",
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
        ForwardsPaginatedQuery query = default,
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

    /// <inheritdoc/>
    public async ValueTask<Result<IThreadMember>> GetThreadMemberAsync
    (
        Snowflake threadId,
        Snowflake userId,
        GetThreadMemberQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new($"channels/{threadId}/thread-members/{userId}");

        if (query.WithMember is not null)
        {
            _ = builder.AddParameter("after", query.WithMember.Value.ToString().ToLowerInvariant());
        }

        return await restClient.ExecuteRequestAsync<IThreadMember>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithSimpleRoute(TopLevelResource.Channel, threadId)
                 .WithRoute($"GET channels/{threadId}/thread-members/:user-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> GroupDMAddRecipientAsync
    (
        Snowflake channelId,
        Snowflake userId,
        IGroupDMAddRecipientPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Put,
            $"channels/{channelId}/recipients/{userId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"PUT channels/{channelId}/recipients/:user-id")
                 .WithPayload(payload),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> GroupDMRemoveRecipientAsync
    (
        Snowflake channelId,
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"channels/{channelId}/recipients/{userId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"DELETE channels/{channelId}/recipients/:user-id"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> JoinThreadAsync
    (
        Snowflake threadId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Put,
            $"channels/{threadId}/recipients/@me",
            b => b.WithSimpleRoute(TopLevelResource.Channel, threadId),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> LeaveThreadAsync
    (
        Snowflake threadId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"channels/{threadId}/recipients/@me",
            b => b.WithSimpleRoute(TopLevelResource.Channel, threadId),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ListArchivedThreadsResponse>> ListJoinedPrivateArchivedThreadsAsync
    (
        Snowflake channelId,
        ListJoinedPrivateArchivedThreadsQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (query.Limit is not null and (< 1 or > 100))
        {
            return new ValidationError("The limit of threads to return must be between 1 and 100.");
        }

        QueryBuilder builder = new($"channels/{channelId}/users/@me/threads/archived/private");

        if (query.Limit is not null)
        {
            _ = builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.Before is not null)
        {
            _ = builder.AddParameter("before", query.Before.Value.ToString());
        }

        return await restClient.ExecuteRequestAsync<ListArchivedThreadsResponse>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ListArchivedThreadsResponse>> ListPrivateArchivedThreadsAsync
    (
        Snowflake channelId,
        ListArchivedThreadsQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (query.Limit is not null and (< 1 or > 100))
        {
            return new ValidationError("The limit of threads to return must be between 1 and 100.");
        }

        QueryBuilder builder = new($"channels/{channelId}/users/@me/threads/archived/private");

        if (query.Limit is not null)
        {
            _ = builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.Before is not null)
        {
            _ = builder.AddParameter("before", query.Before.Value.ToString("o", CultureInfo.InvariantCulture));
        }

        return await restClient.ExecuteRequestAsync<ListArchivedThreadsResponse>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ListArchivedThreadsResponse>> ListPublicArchivedThreadsAsync
    (
        Snowflake channelId,
        ListArchivedThreadsQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (query.Limit is not null and (< 1 or > 100))
        {
            return new ValidationError("The limit of threads to return must be between 1 and 100.");
        }

        QueryBuilder builder = new($"channels/{channelId}/threads/archived/public");

        if (query.Limit is not null)
        {
            _ = builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.Before is not null)
        {
            _ = builder.AddParameter("before", query.Before.Value.ToString("o", CultureInfo.InvariantCulture));
        }

        return await restClient.ExecuteRequestAsync<ListArchivedThreadsResponse>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IThreadMember>>> ListThreadMembersAsync
    (
        Snowflake threadId,
        ListThreadMembersQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (query.Limit is not null and (< 1 or > 100))
        {
            return new ValidationError("The limit of threads to return must be between 1 and 100.");
        }

        QueryBuilder builder = new($"channels/{threadId}/thread-members");

        if (query.Limit is not null)
        {
            _ = builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.After is not null)
        {
            _ = builder.AddParameter("after", query.After.Value.ToString());
        }

        if (query.WithMember is not null)
        {
            _ = builder.AddParameter("with_member", query.WithMember.Value.ToString().ToLowerInvariant());
        }

        return await restClient.ExecuteRequestAsync<IReadOnlyList<IThreadMember>>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithSimpleRoute(TopLevelResource.Channel, threadId),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IChannel>> ModifyChannelAsync
    (
        Snowflake channelId,
        IModifyGroupDMPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.HasValue && payload.Name.Value.Length > 100)
        {
            return new ValidationError("The name of a channel cannot exceed 100 characters.");
        }

        return await restClient.ExecuteRequestAsync<IChannel>
        (
            HttpMethod.Patch,
            $"channels/{channelId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IChannel>> ModifyChannelAsync
    (
        Snowflake channelId,
        IModifyGuildChannelPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.HasValue && payload.Name.Value.Length > 100)
        {
            return new ValidationError("The name of a channel cannot exceed 100 characters.");
        }

        if (payload.RateLimitPerUser.HasValue && payload.RateLimitPerUser.Value is < 0 or > 21600)
        {
            return new ValidationError
            (
                "The slowmode (rate limit per user) in a channel must be between 0 and 6 hours, or 21600 seconds."
            );
        }

        if (payload.Bitrate.TryGetNonNullValue(out int? value) && value < 8000)
        {
            return new ValidationError("The bitrate of a voice channel cannot be below 8000.");
        }

        if (payload.UserLimit.TryGetNonNullValue(out int? userLimit))
        {
            if (payload.Type.HasValue && payload.Type == DiscordChannelType.GuildVoice && userLimit > 99)
            {
                return new ValidationError("The user limit of a voice channel cannot exceed 99.");
            }

            if (userLimit > 10000)
            {
                return new ValidationError("The user limit of a stage channel cannot exceed 10,000.");
            }
        }

        if (payload.DefaultThreadRateLimitPerUser.HasValue && payload.DefaultThreadRateLimitPerUser.Value is < 0 or > 21600)
        {
            return new ValidationError
            (
                "The default slowmode (rate limit per user) for threads must be between 0 and 6 hours, or 21600 seconds."
            );
        }

        return await restClient.ExecuteRequestAsync<IChannel>
        (
            HttpMethod.Patch,
            $"channels/{channelId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IChannel>> ModifyChannelAsync
    (
        Snowflake channelId,
        IModifyThreadChannelPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.HasValue && payload.Name.Value.Length > 100)
        {
            return new ValidationError("The name of a channel cannot exceed 100 characters.");
        }

        if (payload.RateLimitPerUser.HasValue && payload.RateLimitPerUser.Value is < 0 or > 21600)
        {
            return new ValidationError
            (
                "The slowmode (rate limit per user) in a channel must be between 0 and 6 hours, or 21600 seconds."
            );
        }

        if (payload.AutoArchiveDuration.HasValue && !(payload.AutoArchiveDuration.Value is 60 or 1440 or 4320 or 10080))
        {
            return new ValidationError
            (
                "The auto-archive duration of a thread must be either 60, 1440, 4320 or 10080 minutes."
            );
        }

        if (payload.AppliedTags.HasValue && payload.AppliedTags.Value.Count > 5)
        {
            return new ValidationError("A thread can only have up to five tags applied.");
        }

        return await restClient.ExecuteRequestAsync<IChannel>
        (
            HttpMethod.Patch,
            $"channels/{channelId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> PinMessageAsync
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
            HttpMethod.Put,
            $"channels/{channelId}/pins/{messageId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithAuditLogReason(reason)
                 .WithRoute($"PUT channels/{channelId}/pins/:message-id"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> RemoveThreadMemberAsync
    (
        Snowflake threadId,
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"channels/{threadId}/thread-members/{userId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, threadId)
                 .WithRoute($"DELETE channels/{threadId}/thread-members/:user-id"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IChannel>> StartThreadFromMessageAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        IStartThreadFromMessagePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.Length > 100)
        {
            return new ValidationError("The name of a channel cannot exceed 100 characters.");
        }

        if (payload.RateLimitPerUser.HasValue && payload.RateLimitPerUser.Value is < 0 or > 21600)
        {
            return new ValidationError
            (
                "The slowmode (rate limit per user) in a channel must be between 0 and 6 hours, or 21600 seconds."
            );
        }

        if (payload.AutoArchiveDuration.HasValue && !(payload.AutoArchiveDuration.Value is 60 or 1440 or 4320 or 10080))
        {
            return new ValidationError
            (
                "The auto-archive duration of a thread must be either 60, 1440, 4320 or 10080 minutes."
            );
        }

        return await restClient.ExecuteRequestAsync<IChannel>
        (
            HttpMethod.Post,
            $"channels/{channelId}/messages/{messageId}/threads",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"POST channels/{channelId}/messages/:message-id/threads")
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IChannel>> StartThreadInForumOrMediaChannelAsync
    (
        Snowflake channelId,
        IStartThreadInForumOrMediaChannelPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.Length > 100)
        {
            return new ValidationError("The name of a channel cannot exceed 100 characters.");
        }

        if (payload.RateLimitPerUser.HasValue && payload.RateLimitPerUser.Value is < 0 or > 21600)
        {
            return new ValidationError
            (
                "The slowmode (rate limit per user) in a channel must be between 0 and 6 hours, or 21600 seconds."
            );
        }

        if (payload.AutoArchiveDuration.HasValue && !(payload.AutoArchiveDuration.Value is 60 or 1440 or 4320 or 10080))
        {
            return new ValidationError
            (
                "The auto-archive duration of a thread must be either 60, 1440, 4320 or 10080 minutes."
            );
        }

        if (payload.AppliedTags.HasValue && payload.AppliedTags.Value.Count > 5)
        {
            return new ValidationError("A thread can only have up to five tags applied to it.");
        }

        if
        (
            !(payload.Message.Content.HasValue
                || payload.Message.Embeds.HasValue
                || payload.Message.StickerIds.HasValue
                || payload.Message.Components.HasValue
                || payload.Files is not null)
        )
        {
            return new ValidationError
            (
                "At least one of Content, Embeds, StickerIds, Components or Files must be sent."
            );
        }

        if (payload.Message.Content.HasValue && payload.Message.Content.Value.Length > 2000)
        {
            return new ValidationError("The content of a message cannot exceed 2000 characters.");
        }

        if (payload.Message.Embeds.HasValue && payload.Message.Embeds.Value.Count > 10)
        {
            return new ValidationError("Only up to 10 embeds can be sent with a message.");
        }

        if (payload.Message.StickerIds.HasValue && payload.Message.StickerIds.Value.Count > 3)
        {
            return new ValidationError("Only up to 3 stickers can be sent with a message.");
        }

        return await restClient.ExecuteRequestAsync<IChannel>
        (
            HttpMethod.Post,
            $"channels/{channelId}/threads",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IChannel>> StartThreadWithoutMessageAsync
    (
        Snowflake channelId,
        IStartThreadWithoutMessagePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.Length > 100)
        {
            return new ValidationError("The name of a channel cannot exceed 100 characters.");
        }

        if (payload.RateLimitPerUser.HasValue && payload.RateLimitPerUser.Value is < 0 or > 21600)
        {
            return new ValidationError
            (
                "The slowmode (rate limit per user) in a channel must be between 0 and 6 hours, or 21600 seconds."
            );
        }

        if (payload.AutoArchiveDuration.HasValue && !(payload.AutoArchiveDuration.Value is 60 or 1440 or 4320 or 10080))
        {
            return new ValidationError
            (
                "The auto-archive duration of a thread must be either 60, 1440, 4320 or 10080 minutes."
            );
        }

        return await restClient.ExecuteRequestAsync<IChannel>
        (
            HttpMethod.Post,
            $"channels/{channelId}/threads",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithPayload(payload)
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> TriggerTypingIndicatorAsync
    (
        Snowflake channelId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Post,
            $"channels/{channelId}/typing",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> UnpinMessageAsync
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
            $"channels/{channelId}/pins/{messageId}",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"DELETE channels/{channelId}/pins/:message-id"),
            info,
            ct
        );

        return (Result)response;
    }
}
