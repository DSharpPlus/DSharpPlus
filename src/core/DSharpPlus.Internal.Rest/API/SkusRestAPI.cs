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
using DSharpPlus.Internal.Rest.Ratelimiting;

using Remora.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="ISkusRestAPI"/>
public sealed class SkusRestAPI(IRestClient restClient)
    : ISkusRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<ISku>>> ListSkusAsync
    (
        Snowflake applicationId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<ISku>>
        (
            HttpMethod.Get,
            $"applications/{applicationId}/skus",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = false,
                        Resource = TopLevelResource.Other,
                        Route = "applications/:application-id/skus"
                    }
                 )
                 .WithFullRatelimit("GET applications/:application-id/skus"),
            info,
            ct
        );
    }
}
