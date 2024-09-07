// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Abstractions.Rest.API;

/// <summary>
/// Provides access to subscription-related rest API calls.
/// </summary>
public interface ISubscriptionRestAPI
{
    /// <summary>
    /// Fetches all subscriptions pertaining to the relevant SKU, filtered by user.
    /// </summary>
    /// <param name="skuId">The snowflake identifier of the queried SKU.</param>
    /// <param name="query">Additional query arguments for pagination and user filtering.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<ISubscription>>> ListSkuSubscriptionsAsync
    (
        Snowflake skuId,
        ListSkuSubscriptionsQuery query,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Fetches a subscription by ID.
    /// </summary>
    /// <param name="skuId">The snowflake identifier of the SKU this subscription corresponds to.</param>
    /// <param name="subscriptionId">The snowflake identifier of the subscription to query.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<ISubscription>> GetSkuSubscriptionAsync
    (
        Snowflake skuId,
        Snowflake subscriptionId,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
