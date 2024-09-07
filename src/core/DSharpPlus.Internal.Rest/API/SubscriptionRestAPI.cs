// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="ISubscriptionRestAPI"/>
public sealed class SubscriptionRestAPI(IRestClient restClient)
    : ISubscriptionRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<ISubscription>> GetSkuSubscriptionAsync
    (
        Snowflake skuId,
        Snowflake subscriptionId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteMultipartPayloadRequestAsync<ISubscription>
        (
            HttpMethod.Get,
            $"skus/{skuId}/subscriptions/{subscriptionId}",
            b => b.WithRoute("GET skus/:sku-id/subscriptions/:subscription-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<ISubscription>>> ListSkuSubscriptionsAsync
    (
        Snowflake skuId,
        ListSkuSubscriptionsQuery query,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (query.Limit is < 1 or > 100)
        {
            return new ValidationError("The limit of subscriptions to query must be between 1 and 100.");
        }

        QueryBuilder queryBuilder = new($"skus/{skuId}/subscriptions");

        if (query.Before is Snowflake before)
        {
            _ = queryBuilder.AddParameter("before", before.ToString());
        }

        if (query.After is Snowflake after)
        {
            _ = queryBuilder.AddParameter("after", after.ToString());
        }

        if (query.Limit is int limit)
        {
            _ = queryBuilder.AddParameter("limit", limit.ToString(CultureInfo.InvariantCulture));
        }

        if (query.UserId is Snowflake userId)
        {
            _ = queryBuilder.AddParameter("user_id", userId.ToString());
        }

        return await restClient.ExecuteMultipartPayloadRequestAsync<IReadOnlyList<ISubscription>>
        (
            HttpMethod.Get,
            queryBuilder.Build(),
            b => b.WithRoute("GET skus/:sku-id/subscriptions"),
            info,
            ct
        );
    }
}
