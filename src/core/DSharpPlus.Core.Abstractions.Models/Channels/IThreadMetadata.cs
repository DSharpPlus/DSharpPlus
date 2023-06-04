// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Contains additional metadata about thread channels.
/// </summary>
public interface IThreadMetadata
{
    /// <summary>
    /// Indicates whether this thread is considered archived.
    /// </summary>
    public bool Archived { get; }

    /// <summary>
    /// The time in minutes of inactivity before this thread stops showing in the channel list.
    /// Legal values are 60, 1440, 4320 and 10080.
    /// </summary>
    public int AutoArchiveDuration { get; }

    /// <summary>
    /// The timestamp at which this thread's archive status was last changed.
    /// </summary>
    public DateTimeOffset ArchiveTimestamp { get; }

    /// <summary>
    /// Indicates whether this thread is locked, if it is, only users with the manage threads permission
    /// can unlock it.
    /// </summary>
    public bool Locked { get; }

    /// <summary>
    /// Indicates whether non-moderators can add other non-moderators to this thread.
    /// </summary>
    public Optional<bool> Invitable { get; }

    /// <summary>
    /// The timestamp at which this thread was created.
    /// </summary>
    public Optional<DateTimeOffset?> CreateTimestamp { get; }
}
