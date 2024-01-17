// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Rest.Ratelimiting;

using Remora.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IInteractionRestAPI"/>
public sealed class InteractionRestAPI(IRestClient restClient)
    : IInteractionRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IMessage>> CreateFollowupMessageAsync
    (
        Snowflake applicationId,
        string interactionToken,
        ICreateFollowupMessagePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (!(payload.Content.HasValue || payload.Embeds.HasValue || payload.Components.HasValue || payload.Files is not null))
        {
            return new ValidationError
            (
                "At least one of Content, Embeds, Components or Files must be sent."
            );
        }

        if (payload.Content.HasValue && payload.Content.Value.Length > 2000)
        {
            return new ValidationError("The content of a message cannot exceed 2000 characters.");
        }

        if (payload.Embeds.HasValue && payload.Embeds.Value.Count > 10)
        {
            return new ValidationError("Only up to 10 embeds can be sent with a message.");
        }

        return await restClient.ExecuteRequestAsync<IMessage>
        (
            HttpMethod.Post,
            $"webhooks/{applicationId}/{interactionToken}",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = true,
                        Resource = TopLevelResource.Webhook,
                        Route = $"webhooks/{applicationId}/{interactionToken}"
                    }
                 )
                 .WithRoute($"webhooks/{applicationId}/{interactionToken}")
                 .WithFullRatelimit($"POST webhooks/{applicationId}/{interactionToken}")
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> CreateInteractionResponseAsync
    (
        Snowflake interactionId,
        string interactionToken,
        IInteractionResponse payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Type is DiscordInteractionCallbackType.ChannelMessageWithSource or DiscordInteractionCallbackType.UpdateMessage)
        {
            IMessageCallbackData message = payload.Data.Value.AsT1;

            if (!(message.Content.HasValue || message.Embeds.HasValue || message.Components.HasValue || message.Files is not null))
            {
                return new ValidationError
                (
                    "At least one of Content, Embeds, Components or Files must be sent."
                );
            }

            if (message.Content.HasValue && message.Content.Value.Length > 2000)
            {
                return new ValidationError("The content of a message cannot exceed 2000 characters.");
            }

            if (message.Embeds.HasValue && message.Embeds.Value.Count > 10)
            {
                return new ValidationError("Only up to 10 embeds can be sent with a message.");
            }
        }
        else if (payload.Type is DiscordInteractionCallbackType.Modal)
        {
            IModalCallbackData modal = payload.Data.Value.AsT2;

            if (modal.CustomId.Length > 100)
            {
                return new ValidationError("The length of the custom ID of a modal cannot exceed 100 characters.");
            }

            if (modal.Title.Length > 45)
            {
                return new ValidationError("The length of the modal title cannot exceed 45 characters.");
            }

            if (modal.Components.Count > 5)
            {
                return new ValidationError("A modal does not support more than five components.");
            }
        }

        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Post,
            $"interactions/{interactionId}/{interactionToken}/callback",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = true,
                        Resource = TopLevelResource.Webhook,
                        Route = $"interactions/{interactionId}/{interactionToken}"
                    }
                 )
                 .WithRoute($"interactions/{interactionId}/{interactionToken}/callback")
                 .WithFullRatelimit($"POST interactions/{interactionId}/{interactionToken}/callback")
                 .WithPayload(payload),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteFollowupMessageAsync
    (
        Snowflake applicationId,
        string interactionToken,
        Snowflake messageId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"webhooks/{applicationId}/{interactionToken}/messages/{messageId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = true,
                        Resource = TopLevelResource.Webhook,
                        Route = $"webhooks/{applicationId}/{interactionToken}"
                    }
                 )
                 .WithRoute($"webhooks/{applicationId}/{interactionToken}/messages/:message-id")
                 .WithFullRatelimit($"DELETE webhooks/{applicationId}/{interactionToken}/messages/:message-id"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteInteractionResponseAsync
    (
        Snowflake applicationId,
        string interactionToken,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"webhooks/{applicationId}/{interactionToken}/messages/@original",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = true,
                        Resource = TopLevelResource.Webhook,
                        Route = $"webhooks/{applicationId}/{interactionToken}"
                    }
                 )
                 .WithRoute($"webhooks/{applicationId}/{interactionToken}/messages/@original")
                 .WithFullRatelimit($"DELETE webhooks/{applicationId}/{interactionToken}/messages/@original"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IMessage>> EditFollowupMessageAsync
    (
        Snowflake applicationId,
        string interactionToken,
        Snowflake messageId,
        IEditFollowupMessagePayload payload,
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
            return new ValidationError("Only up to 10 embeds can be sent with a message.");
        }

        return await restClient.ExecuteRequestAsync<IMessage>
        (
            HttpMethod.Patch,
            $"webhooks/{applicationId}/{interactionToken}/messages/{messageId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = true,
                        Resource = TopLevelResource.Webhook,
                        Route = $"webhooks/{applicationId}/{interactionToken}"
                    }
                 )
                 .WithRoute($"webhooks/{applicationId}/{interactionToken}/messages/:message-id")
                 .WithFullRatelimit($"PATCH webhooks/{applicationId}/{interactionToken}/messages/:message-id")
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IMessage>> EditInteractionResponseAsync
    (
        Snowflake applicationId,
        string interactionToken,
        IEditInteractionResponsePayload payload,
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
            return new ValidationError("Only up to 10 embeds can be sent with a message.");
        }

        return await restClient.ExecuteRequestAsync<IMessage>
        (
            HttpMethod.Patch,
            $"webhooks/{applicationId}/{interactionToken}/messages/@original",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = true,
                        Resource = TopLevelResource.Webhook,
                        Route = $"webhooks/{applicationId}/{interactionToken}"
                    }
                 )
                 .WithRoute($"webhooks/{applicationId}/{interactionToken}/messages/@original")
                 .WithFullRatelimit($"PATCH webhooks/{applicationId}/{interactionToken}/messages/@original")
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IMessage>> GetFollowupMessageAsync
    (
        Snowflake applicationId,
        string interactionToken,
        Snowflake messageId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IMessage>
        (
            HttpMethod.Get,
            $"webhooks/{applicationId}/{interactionToken}/messages/{messageId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = true,
                        Resource = TopLevelResource.Webhook,
                        Route = $"webhooks/{applicationId}/{interactionToken}"
                    }
                 )
                 .WithRoute($"webhooks/{applicationId}/{interactionToken}/messages/{messageId}")
                 .WithFullRatelimit($"GET webhooks/{applicationId}/{interactionToken}/messages/{messageId}"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IMessage>> GetInteractionResponseAsync
    (
        Snowflake applicationId,
        string interactionToken,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IMessage>
        (
            HttpMethod.Get,
            $"webhooks/{applicationId}/{interactionToken}/messages/@original",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = true,
                        Resource = TopLevelResource.Webhook,
                        Route = $"webhooks/{applicationId}/{interactionToken}"
                    }
                 )
                 .WithRoute($"webhooks/{applicationId}/{interactionToken}/messages/@original")
                 .WithFullRatelimit($"GET webhooks/{applicationId}/{interactionToken}/messages/@original"),
            info,
            ct
        );
    }
}
