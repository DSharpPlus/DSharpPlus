// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System;
using System.Collections.Generic;
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

using Remora.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IWebhookRestAPI"/>
public sealed class WebhookRestAPI(IRestClient restClient)
    : IWebhookRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IWebhook>> CreateWebhookAsync
    (
        Snowflake channelId,
        ICreateWebhookPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.Length > 80)
        {
            return new ValidationError("A webhook name cannot exceed 80 characters.");
        }

        if
        (
            payload.Name.Contains("discord", StringComparison.InvariantCultureIgnoreCase)
            || payload.Name.Contains("clyde", StringComparison.InvariantCultureIgnoreCase)
        )
        {
            return new ValidationError("A webhook name cannot contain the substrings \"clyde\" or \"discord\".");
        }

        return await restClient.ExecuteRequestAsync<IWebhook>
        (
            HttpMethod.Post,
            $"channels/{channelId}/webhooks",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Channel,
                        Id = channelId
                    }
                 )
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteWebhookAsync
    (
        Snowflake webhookId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"webhooks/{webhookId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Webhook,
                        Id = webhookId
                    }
                 )
                 .WithAuditLogReason(reason),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteWebhookMessageAsync
    (
        Snowflake webhookId,
        string webhookToken,
        Snowflake messageId,
        ThreadIdQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new()
        {
            RootUri = $"webhooks/{webhookId}/{webhookToken}/messages/{messageId}"
        };

        if (query.ThreadId is not null)
        {
            builder.AddParameter("thread_id", query.ThreadId.Value.ToString());
        }

        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            builder.ToString(),
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Webhook,
                        Id = webhookId
                    }
                 )
                 .WithRoute($"DELETE webhooks/{webhookId}/:webhook-token/messages/:message-id")
                 .AsWebhookRequest(),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteWebhookWithTokenAsync
    (
        Snowflake webhookId,
        string webhookToken,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"webhooks/{webhookId}/{webhookToken}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Webhook,
                        Id = webhookId
                    }
                 )
                 .WithRoute($"DELETE webhooks/{webhookId}/:webhook-token")
                 .WithAuditLogReason(reason)
                 .AsWebhookRequest(),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IMessage>> EditWebhookMessageAsync
    (
        Snowflake webhookId,
        string webhookToken,
        Snowflake messageId,
        IEditWebhookMessagePayload payload,
        ThreadIdQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Content.TryGetNonNullValue(out string? content) && content.Length > 2000)
        {
            return new ValidationError("A webhook message cannot exceed 2000 characters in length.");
        }

        if (payload.Embeds.TryGetNonNullValue(out IReadOnlyList<IEmbed>? embeds) && embeds.Count > 10)
        {
            return new ValidationError("A webhook message cannot contain more than 10 embeds.");
        }

        if (payload.Components.TryGetNonNullValue(out IReadOnlyList<IActionRowComponent>? components) && components.Count > 5)
        {
            return new ValidationError("A webhook message cannot contain more than 5 action rows.");
        }

        QueryBuilder builder = new()
        {
            RootUri = $"webhooks/{webhookId}/{webhookToken}/messages/{messageId}"
        };

        if (query.ThreadId is not null)
        {
            builder.AddParameter("thread_id", query.ThreadId.Value.ToString());
        }

        return await restClient.ExecuteRequestAsync<IMessage>
        (
            HttpMethod.Patch,
            builder.Build(),
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Webhook,
                        Id = webhookId
                    }
                 )
                 .WithRoute($"PATCH webhooks/{webhookId}/:webhook-token/messages/:message-id")
                 .WithPayload(payload)
                 .AsWebhookRequest(),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IMessage?>> ExecuteWebhookAsync
    (
        Snowflake webhookId,
        string webhookToken,
        IExecuteWebhookPayload payload,
        ExecuteWebhookQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Content.TryGetNonNullValue(out string? content) && content.Length > 2000)
        {
            return new ValidationError("A webhook message cannot exceed 2000 characters in length.");
        }

        if (payload.Embeds.TryGetNonNullValue(out IReadOnlyList<IEmbed>? embeds) && embeds.Count > 10)
        {
            return new ValidationError("A webhook message cannot contain more than 10 embeds.");
        }

        if (payload.Components.TryGetNonNullValue(out IReadOnlyList<IActionRowComponent>? components) && components.Count > 5)
        {
            return new ValidationError("A webhook message cannot contain more than 5 action rows.");
        }

        QueryBuilder builder = new()
        {
            RootUri = $"webhooks/{webhookId}/{webhookToken}"
        };

        if (query.ThreadId is not null)
        {
            builder.AddParameter("thread_id", query.ThreadId.Value.ToString());
        }

        if (query.Wait is not null)
        {
            builder.AddParameter("wait", query.Wait.Value.ToString().ToLowerInvariant());
        }

        if (query.Wait == true)
        {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return await restClient.ExecuteRequestAsync<IMessage>
            (
                HttpMethod.Patch,
                builder.Build(),
                b => b.WithSimpleRoute
                     (
                        new SimpleSnowflakeRatelimitRoute
                        {
                            Resource = TopLevelResource.Webhook,
                            Id = webhookId
                        }
                     )
                     .WithRoute($"PATCH webhooks/{webhookId}/:webhook-token/messages/:message-id")
                     .WithPayload(payload)
                     .AsWebhookRequest(),
                info,
                ct
            );
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }
        else
        {
            Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
            (
                HttpMethod.Patch,
                builder.Build(),
                b => b.WithSimpleRoute
                     (
                        new SimpleSnowflakeRatelimitRoute
                        {
                            Resource = TopLevelResource.Webhook,
                            Id = webhookId
                        }
                     )
                     .WithRoute($"PATCH webhooks/{webhookId}/:webhook-token/messages/:message-id")
                     .WithPayload(payload)
                     .AsWebhookRequest(),
                info,
                ct
            );

            return response.Map<IMessage?>(message => null);
        }
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IWebhook>>> GetChannelWebhooksAsync
    (
        Snowflake channelId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IWebhook>>
        (
            HttpMethod.Get,
            $"channels/{channelId}/webhooks",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Channel,
                        Id = channelId
                    }
                 ),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IWebhook>>> GetGuildWebhooksAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IWebhook>>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/webhooks",
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
    public async ValueTask<Result<IWebhook>> GetWebhookAsync
    (
        Snowflake webhookId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IWebhook>
        (
            HttpMethod.Get,
            $"webhooks/{webhookId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Webhook,
                        Id = webhookId
                    }
                 ),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IMessage>> GetWebhookMessageAsync
    (
        Snowflake webhookId,
        string webhookToken,
        Snowflake messageId,
        ThreadIdQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new()
        {
            RootUri = $"webhooks/{webhookId}/{webhookToken}/messages/{messageId}"
        };

        if (query.ThreadId is not null)
        {
            builder.AddParameter("thread_id", query.ThreadId.Value.ToString());
        }

        return await restClient.ExecuteRequestAsync<IMessage>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Webhook,
                        Id = webhookId
                    }
                 )
                 .WithRoute($"PATCH webhooks/{webhookId}/:webhook-token/messages/:message-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IWebhook>> GetWebhookWithTokenAsync
    (
        Snowflake webhookId,
        string webhookToken,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IWebhook>
        (
            HttpMethod.Get,
            $"webhooks/{webhookId}/{webhookToken}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Webhook,
                        Id = webhookId
                    }
                 )
                 .AsWebhookRequest(),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IWebhook>> ModifyWebhookAsync
    (
        Snowflake webhookId,
        IModifyWebhookPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.HasValue)
        {
            if (payload.Name.Value.Length > 80)
            {
                return new ValidationError("A webhook name cannot exceed 80 characters.");
            }

            if
            (
                payload.Name.Value.Contains("discord", StringComparison.InvariantCultureIgnoreCase)
                || payload.Name.Value.Contains("clyde", StringComparison.InvariantCultureIgnoreCase)
            )
            {
                return new ValidationError("A webhook name cannot contain the substrings \"clyde\" or \"discord\".");
            }
        }

        return await restClient.ExecuteRequestAsync<IWebhook>
        (
            HttpMethod.Patch,
            $"webhooks/{webhookId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Webhook,
                        Id = webhookId
                    }
                 )
                 .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IWebhook>> ModifyWebhookWithTokenAsync
    (
        Snowflake webhookId,
        string webhookToken,
        IModifyWebhookWithTokenPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.HasValue)
        {
            if (payload.Name.Value.Length > 80)
            {
                return new ValidationError("A webhook name cannot exceed 80 characters.");
            }

            if
            (
                payload.Name.Value.Contains("discord", StringComparison.InvariantCultureIgnoreCase)
                || payload.Name.Value.Contains("clyde", StringComparison.InvariantCultureIgnoreCase)
            )
            {
                return new ValidationError("A webhook name cannot contain the substrings \"clyde\" or \"discord\".");
            }
        }

        return await restClient.ExecuteRequestAsync<IWebhook>
        (
            HttpMethod.Patch,
            $"webhooks/{webhookId}/{webhookToken}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Webhook,
                        Id = webhookId
                    }
                 )
                 .WithAuditLogReason(reason)
                 .AsWebhookRequest(),
            info,
            ct
        );
    }
}
