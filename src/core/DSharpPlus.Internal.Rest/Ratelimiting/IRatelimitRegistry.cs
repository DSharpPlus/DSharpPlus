// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Net.Http;

using Remora.Results;

namespace DSharpPlus.Internal.Rest.Ratelimiting;

/// <summary>
/// Represents a library hook for modifying ratelimiting behaviour. This is called into by rest-internal code,
/// and third-party rest implementations may not behave identically.
/// </summary>
public interface IRatelimitRegistry
{
    /// <summary>
    /// Checks whether this request should be allowed to proceed. This should be considering as enqueuing a request
    /// as far as <seealso cref="CancelRequest(HttpRequestMessage)"/> is concerned.
    /// </summary>
    public Result<bool> CheckRatelimit(HttpRequestMessage request);

    /// <summary>
    /// Updates the ratelimits encountered from the given request.
    /// </summary>
    public Result UpdateRatelimit(HttpRequestMessage request, HttpResponseMessage response);

    /// <summary>
    /// Cancels a request reservation made in <seealso cref="CheckRatelimit(HttpRequestMessage)"/>.
    /// </summary>
    public Result CancelRequest(HttpRequestMessage request);
}
