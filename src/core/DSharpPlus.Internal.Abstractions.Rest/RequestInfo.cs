// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;

namespace DSharpPlus.Internal.Abstractions.Rest;

/// <summary>
/// Contains additional instructions on how to execute a request.
/// </summary>
public readonly record struct RequestInfo
{
    /// <summary>
    /// The maximum time to wait for this request, in milliseconds.
    /// </summary>
    /// <remarks>
    /// If a ratelimit is triggered for this request and it cannot be retried in time, this request will fail
    /// immediately. <c>null</c> specifies no timeout, which is the default.
    /// </remarks>
    public int? Timeout { get; init; }

    /// <summary>
    /// The maximum amount of retries to make for this request.
    /// </summary>
    /// <remarks>
    /// <c>null</c> falls back to the underlying rest clients discretion.
    /// </remarks>
    public int? MaxRetries { get; init; }

    /// <summary>
    /// A set of flags to specify retrying behaviour.
    /// </summary>
    /// <remarks>
    /// <c>null</c> falls back to the underlying rest clients discretion.
    /// </remarks>
    public RetryMode? RetryMode { get; init; }

    /// <summary>
    /// Specifies whether to skip updating the cache after making a request.
    /// </summary>
    public bool SkipUpdatingCache { get; init; }

    /// <summary>
    /// Specifies whether to skip asking cache for whether it already has the requested information.
    /// </summary>
    public bool SkipCache { get; init; }

    /// <summary>
    /// An asynchronous callback to execute when the ratelimit fails.
    /// </summary>
    public Func<IRatelimitCallbackInfo, ValueTask>? RatelimitCallback { get; init; }
}
