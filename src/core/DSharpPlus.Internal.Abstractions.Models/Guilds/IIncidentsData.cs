// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Contains metadata for a guild pertaining to recent incidents and moderation actions, such as temporarily disabling invites or detected raids.
/// </summary>
public interface IIncidentsData
{
    /// <summary>
    /// Indicates when invites get enabled again.
    /// </summary>
    public DateTimeOffset? InvitesDisabledUntil { get; }

    /// <summary>
    /// Indicates when direct messages between guild members get enabled again. Note that they can still message each other if they can do so through other
    /// means, such as being friends or sharing another mutual server.
    /// </summary>
    public DateTimeOffset? DmsDisabledUntil { get; }

    /// <summary>
    /// Indicates when DM spam was last detected in the guild.
    /// </summary>
    public Optional<DateTimeOffset?> DmSpamDetectedAt { get; }

    /// <summary>
    /// Indicates when a raid was last detected in the guild.
    /// </summary>
    public Optional<DateTimeOffset?> RaidDetectedAt { get; }
}
