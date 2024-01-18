// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Remora.Results;

namespace DSharpPlus.Internal.Abstractions.Rest;

/// <summary>
/// Represents the central rest client handling the requests made to Discord.
/// </summary>
public interface IRestClient
{
    /// <summary>
    /// Sends a request to the Discord API.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="method">The HTTP method this request should be sent to.</param>
    /// <param name="path">The path this request will take.</param>
    /// <param name="request">Constructs the request to be sent to Discord.</param>
    /// <param name="info">Specifies additional parameters for this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The response from Discord, or an appropriate error.</returns>
    public ValueTask<Result<T>> ExecuteRequestAsync<T>
    (
        HttpMethod method,
        string path,
        Action<RequestBuilder> request,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Sends a request to the Discord API, serializing every payload element into a separate form parameter.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="method">The HTTP method this request should be sent to.</param>
    /// <param name="path">The path this request will take.</param>
    /// <param name="request">Constructs the request to be sent to Discord.</param>
    /// <param name="info">Specifies additional parameters for this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The response from Discord, or an appropriate error.</returns>
    public ValueTask<Result<T>> ExecuteMultipartPayloadRequestAsync<T>
    (
        HttpMethod method,
        string path,
        Action<RequestBuilder> request,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Sends a request to the Discord API.
    /// </summary>
    /// <param name="method">The HTTP method this request should be sent to.</param>
    /// <param name="path">The path this request will take.</param>
    /// <param name="request">Constructs the request to be sent to Discord.</param>
    /// <param name="info">Specifies additional parameters for this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The response from Discord, or an appropriate error.</returns>
    public ValueTask<Result<HttpResponseMessage>> ExecuteRequestAsync
    (
        HttpMethod method,
        string path,
        Action<RequestBuilder> request,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
