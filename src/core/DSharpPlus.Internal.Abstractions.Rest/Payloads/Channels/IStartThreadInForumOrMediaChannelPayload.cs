// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /channels/:channel-id/threads</c>.
/// </summary>
public interface IStartThreadInForumOrMediaChannelPayload
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
    /// The first message in this forum/media thread.
    /// </summary>
    public IForumAndMediaThreadMessage Message { get; }

    /// <summary>
    /// The snowflake identifiers of tags to apply to this thread.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> AppliedTags { get; }

    /// <summary>
    /// The contents of the files to send.
    /// </summary>
    public IReadOnlyList<AttachmentData>? Files { get; }
}
