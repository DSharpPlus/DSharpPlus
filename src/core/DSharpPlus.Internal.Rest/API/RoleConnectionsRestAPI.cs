// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Rest.Ratelimiting;

using Remora.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IRoleConnectionsRestAPI"/>
public sealed class RoleConnectionsRestAPI(IRestClient restClient)
    : IRoleConnectionsRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IRoleConnectionMetadata>>> GetRoleConnectionMetadataRecordsAsync
    (
        Snowflake applicationId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IRoleConnectionMetadata>>
        (
            HttpMethod.Get,
            $"applications/{applicationId}/role-connections/metadata",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = false,
                        Resource = TopLevelResource.Other,
                        Route = "applications/:application-id/role-connections/metadata"
                    }
                 )
                 .WithFullRatelimit("GET applications/:application-id/role-connections/metadata"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IRoleConnectionMetadata>>> UpdateRoleConnectionMetadataRecordsAsync
    (
        Snowflake applicationId,
        IReadOnlyList<IRoleConnectionMetadata> payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Count > 5)
        {
            return new ValidationError("An application can have up to five role connection metadata records.");
        }

        return await restClient.ExecuteRequestAsync<IReadOnlyList<IRoleConnectionMetadata>>
        (
            HttpMethod.Put,
            $"applications/{applicationId}/role-connections/metadata",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = false,
                        Resource = TopLevelResource.Other,
                        Route = "applications/:application-id/role-connections/metadata"
                    }
                 )
                 .WithFullRatelimit("PUT applications/:application-id/role-connections/metadata")
                 .WithPayload(payload),
            info,
            ct
        );
    }
}
