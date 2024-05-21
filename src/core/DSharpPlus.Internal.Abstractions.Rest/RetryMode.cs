// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Internal.Abstractions.Rest;

/// <summary>
/// Specifies behaviour flags to apply to retrying a request.
/// </summary>
[Flags]
public enum RetryMode
{
    /// <summary>
    /// No options, behave as the rest client default.
    /// </summary>
    None = 0,

    /// <summary>
    /// Suppresses executing a configured <see cref="RequestInfo.RatelimitCallback"/>.
    /// </summary>
    SuppressCallback = 1 << 1,

    /// <summary>
    /// If any error is encountered, excluding a pre-emptive ratelimit, fail immediately.
    /// </summary>
    AlwaysFailExcludingPreemptive = 1 << 2,

    /// <summary>
    /// If any error is encountered, including a pre-emptive ratelimit, fail immediately.
    /// </summary>
    AlwaysFail = 1 << 3,

    /// <summary>
    /// Retry on 5xx errors.
    /// </summary>
    Retry5xx = 1 << 4,

    /// <summary>
    /// Skips retrying on preemptive ratelimits.
    /// </summary>
    DoNotRetryPreemptiveRatelimit = 1 << 5,

    /// <summary>
    /// Retry on discord-returned ratelimits, except global ratelimits.
    /// </summary>
    RetryDiscordRatelimit = 1 << 6,

    /// <summary>
    /// Retry on global ratelimits.
    /// </summary>
    RetryGlobalRatelimit = 1 << 7,

    /// <summary>
    /// Skip request validation. This should be used only with the greatest care.
    /// </summary>
    SkipValidation = 1 << 8,

    /// <summary>
    /// Convenience combination for always retrying on ratelimits and 5xx errors.
    /// </summary>
    AlwaysRetry = Retry5xx | RetryDiscordRatelimit | RetryGlobalRatelimit,

    /// <summary>
    /// Convenience combination for always retrying on non-global ratelimits and 5xx errors, as global ratelimits
    /// can take untenably long times to complete.
    /// </summary>
    AlwaysRetryNonGlobal = Retry5xx | RetryDiscordRatelimit
}
