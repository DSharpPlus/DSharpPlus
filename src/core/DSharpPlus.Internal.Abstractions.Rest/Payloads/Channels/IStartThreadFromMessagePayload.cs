// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /channels/:channel-id/messages/:message-id/threads</c>.
/// </summary>
public interface IStartThreadFromMessagePayload
{
    /// <summary>
    /// 1-100 characters, channel name for this thread.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Auto archive duration for this thread in minutes.
    /// </summary>
    public Optional<int> AutoArchiveDuration { get; }

    /// <summary>
    /// Slowmode for users in seconds.
    /// </summary>
    public Optional<int?> RateLimitPerUser { get; }
}
