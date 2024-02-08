// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading;

namespace DSharpPlus.Internal.Rest.Ratelimiting;

/// <summary>
/// Contains options ot configure ratelimiting behaviour.
/// </summary>
public sealed class RatelimitOptions
{
    /// <summary>
    /// Indicates to the library whether to use a separate thread to clean up old ratelimits.
    /// </summary>
    public bool UseSeparateCleanupThread { get; set; } = true;

    /// <summary>
    /// Specifies the interval in milliseconds at which ratelimits are cleaned up.
    /// </summary>
    public int CleanupInterval { get; set; } = 10000;

    /// <summary>
    /// Gets a cancellation token for ratelimit cleanup.
    /// </summary>
    public CancellationToken Token { get; set; }
}
