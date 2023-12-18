// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /channels/:channel-id/threads</c>.
/// </summary>
public interface IStartThreadWithoutMessagePayload
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

    /// <summary>
    /// The type of thread to be created.
    /// </summary>
    // This field is currently technically optional as per API spec, but this behaviour is slated for removal in the future.
    // Therefore, it is kept as a required field here.
    public DiscordChannelType Type { get; }

    /// <summary>
    /// Indicates whether non-moderators can add members to this private thread.
    /// </summary>
    public Optional<bool> Invitable { get; init; }
}
