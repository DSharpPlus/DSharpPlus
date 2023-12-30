// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a partially populated invite object.
/// </summary>
public interface IPartialInvite
{
    /// <summary>
    /// The unique invite code.
    /// </summary>
    public Optional<string> Code { get; }

    /// <summary>
    /// The guild this invite points to.
    /// </summary>
    public Optional<IPartialGuild> Guild { get; }

    /// <summary>
    /// The channel this invite points to.
    /// </summary>
    public Optional<IPartialChannel?> Channel { get; }

    /// <summary>
    /// The user who created this invite.
    /// </summary>
    public Optional<IUser> Inviter { get; }

    /// <summary>
    /// The target type of this voice channel invite.
    /// </summary>
    public Optional<DiscordInviteTargetType> TargetType { get; }

    /// <summary>
    /// The user whose stream to display for this stream invite.
    /// </summary>
    public Optional<IUser> TargetUser { get; }

    /// <summary>
    /// The embedded application to open for this embedded application invite.
    /// </summary>
    public Optional<IPartialApplication> TargetApplication { get; }

    /// <summary>
    /// The approximate count of online members for this guild.
    /// </summary>
    public Optional<int> ApproximatePresenceCount { get; }

    /// <summary>
    /// The approximate count of total members in this guild.
    /// </summary>
    public Optional<int> ApproximateMemberCount { get; }

    /// <summary>
    /// The expiration date of this invite.
    /// </summary>
    public Optional<DateTimeOffset?> ExpiresAt { get; }

    /// <summary>
    /// Guild scheduled event data for the guild this invite points to.
    /// </summary>
    public Optional<IScheduledEvent> GuildScheduledEvent { get; }

    /// <summary>
    /// The number of times this invite has been used.
    /// </summary>
    public Optional<int> Uses { get; }

    /// <summary>
    /// The number of times this invite can be used.
    /// </summary>
    public Optional<int> MaxUses { get; }

    /// <summary>
    /// The duration in seconds after which this invite expires.
    /// </summary>
    public Optional<int> MaxAge { get; }

    /// <summary>
    /// Indicates whether this invite only grants temporary membership.
    /// </summary>
    public Optional<bool> Temporary { get; }

    /// <summary>
    /// Indicates when this invite was created.
    /// </summary>
    public Optional<DateTimeOffset> CreatedAt { get; }
}
