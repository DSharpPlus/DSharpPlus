// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IApplicationRestAPI"/>
public sealed class ApplicationRestAPI(IRestClient restClient)
    : IApplicationRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IApplication>> EditCurrentApplicationAsync
    (
        IEditCurrentApplicationPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Tags.TryGetNonNullValue(out IReadOnlyList<string>? value))
        {
            if (value.Count > 5)
            {
                return new ValidationError("An application can only have up to five tags.");
            }

            if (value.Any(tag => tag.Length > 20))
            {
                return new ValidationError("Tags of an application cannot exceed 20 characters.");
            }
        }

        return await restClient.ExecuteRequestAsync<IApplication>
        (
            HttpMethod.Patch,
            $"applications/@me",
            b => b.WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IApplication>> GetCurrentApplicationAsync
    (
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IApplication>
        (
            method: HttpMethod.Get,
            path: $"applications/@me",
            info: info,
            ct: ct
        );
    }
}
