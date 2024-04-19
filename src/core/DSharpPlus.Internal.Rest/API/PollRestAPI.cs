// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Internal.Abstractions.Rest.Responses;
using DSharpPlus.Internal.Rest.Ratelimiting;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IPollRestAPI"/>
public sealed class PollRestAPI(IRestClient restClient)
    : IPollRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IMessage>> EndPollAsync
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
            $"channels/{channelId}/polls/{messageId}/expire",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"POST channels/{channelId}/polls/:message-id/expire"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<GetAnswerVotersResponse>> GetAnswerVotersAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        int answerId,
        ForwardsPaginatedQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new($"channels/{channelId}/polls/{messageId}/answers/{answerId}");

        if (query.After is not null)
        {
            _ = builder.AddParameter("after", query.After.Value.ToString());
        }

        if (query.Limit is not null)
        {
            if (query.Limit is < 1 or > 100)
            {
                return new ValidationError("The provided limit must be between 1 and 100.");
            }

            _ = builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        return await restClient.ExecuteRequestAsync<GetAnswerVotersResponse>
        (
            HttpMethod.Post,
            builder.Build(),
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                 .WithRoute($"POST channels/{channelId}/polls/:message-id/answers/:answer-id"),
            info,
            ct
        );
    }
}
