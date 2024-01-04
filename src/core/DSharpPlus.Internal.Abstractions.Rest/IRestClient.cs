// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

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
    /// <typeparam name="T">The concrete type of request.</typeparam>
    /// <param name="request">
    /// The request object. Implementers should handle all types, but may special-case well-known types.
    /// </param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The response from Discord, or an appropriate error.</returns>
    public ValueTask<Result<HttpResponseMessage>> ExecuteRequestAsync<T>
    (
        T request,
        CancellationToken ct = default
    )
        where T : IRestRequest;
}
