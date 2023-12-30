// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Internal.Abstractions.Rest;

/// <summary>
/// Represents information and operations on a callback.
/// </summary>
public interface IRatelimitCallbackInfo
{
    /// <summary>
    /// Indicates whether the encountered ratelimit is a global ratelimit.
    /// </summary>
    public bool IsGlobalRatelimit { get; }

    /// <summary>
    /// The now-exhausted limit of this bucket.
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// The timestamp at which this ratelimit bucket will reset and be freed up.
    /// </summary>
    public DateTimeOffset Reset { get; }

    /// <summary>
    /// The route this request sent over.
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// The hash of the encountered ratelimit bucket.
    /// </summary>
    public string Hash { get; }

    /// <summary>
    /// Indicates whether this request can retry without user interaction.
    /// </summary>
    public bool MayRetry { get; }

    /// <summary>
    /// Cancels all retries and forcibly ends this request's lifespan.
    /// </summary>
    public void CancelRetries();

    /// <summary>
    /// Forces this request to retry, even if it wouldn't have otherwise.
    /// </summary>
    public void Retry();
}
