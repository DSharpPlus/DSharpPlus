// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IEntitlementsRestAPI"/>
public sealed class EntitlementsRestAPI(IRestClient restClient)
    : IEntitlementsRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IPartialEntitlement>> CreateTestEntitlementAsync
    (
        Snowflake applicationId,
        ICreateTestEntitlementPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IPartialEntitlement>
        (
            HttpMethod.Post,
            $"applications/{applicationId}/entitlements",
            b => b.WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteTestEntitlementAsync
    (
        Snowflake applicationId,
        Snowflake entitlementId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"applications/{applicationId}/entitlements/{entitlementId}",
            b => b.WithRoute($"DELETE applications/:application-id/entitlements/:entitlement-id"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IEntitlement>>> ListEntitlementsAsync
    (
        Snowflake applicationId,
        ListEntitlementsQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (query.Limit is not null and (< 1 or > 100))
        {
            return new ValidationError("The limit of entitlements to list must be between 1 and 100.");
        }

        QueryBuilder builder = new($"applications/{applicationId}/entitlements");

        if (query.UserId is not null)
        {
            _ = builder.AddParameter("user_id", query.UserId.Value.ToString());
        }

        if (query.SkuIds is not null)
        {
            _ = builder.AddParameter("sku_ids", query.SkuIds);
        }

        if (query.Before is not null)
        {
            _ = builder.AddParameter("before", query.Before.Value.ToString());
        }

        if (query.After is not null)
        {
            _ = builder.AddParameter("after", query.After.Value.ToString());
        }

        if (query.Limit is not null)
        {
            _ = builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.GuildId is not null)
        {
            _ = builder.AddParameter("guild_id", query.GuildId.Value.ToString());
        }

        if (query.ExcludeEnded is not null)
        {
            _ = builder.AddParameter("exclude_ended", query.ExcludeEnded.Value.ToString().ToLowerInvariant());
        }

        return await restClient.ExecuteRequestAsync<IReadOnlyList<IEntitlement>>
        (
            method: HttpMethod.Get,
            path: builder.Build(),
            info: info,
            ct: ct
        );
    }
}
