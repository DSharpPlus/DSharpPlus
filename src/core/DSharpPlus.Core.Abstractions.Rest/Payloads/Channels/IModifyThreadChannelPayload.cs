// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /channels/:channel-id</c>.
/// </summary>
public interface IModifyThreadChannelPayload
{
    /// <summary>
    /// The new name for this thread channel.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// Indicates whether this thread is archived. This must either be false or be set to false.
    /// </summary>
    public Optional<bool> Archived { get; }

    /// <summary>
    /// The new auto archive duration for this thread, in seconds.
    /// </summary>
    public Optional<int> AutoArchiveDuration { get; }

    /// <summary>
    /// Indicates whether this thread is locked.
    /// </summary>
    public Optional<bool> Locked { get; }

    /// <summary>
    /// Indicates whether non-moderators can add other non-moderators to this private thread.
    /// </summary>
    public Optional<bool> Invitable { get; }

    /// <summary>
    /// The new slowmode duration for this thread, in seconds.
    /// </summary>
    public Optional<int?> RateLimitPerUser { get; }

    /// <summary>
    /// Flags for this thread.
    /// </summary>
    public Optional<DiscordChannelFlags> Flags { get; }

    /// <summary>
    /// The snowflake IDs of the tags that have been applied to this thread.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> AppliedTags { get; }
}
