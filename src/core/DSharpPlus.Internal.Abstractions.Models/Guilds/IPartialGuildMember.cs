// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a single member of a guild.
/// </summary>
public interface IPartialGuildMember
{
    /// <summary>
    /// The underlying user account.
    /// </summary>
    public Optional<IUser> User { get; }

    /// <summary>
    /// This user's guild nickname.
    /// </summary>
    public Optional<string?> Nick { get; }

    /// <summary>
    /// The user's guild avatar hash.
    /// </summary>
    public Optional<string?> Avatar { get; }

    /// <summary>
    /// This user's list of roles.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> Roles { get; }

    /// <summary>
    /// Stores when this user last joined this guild.
    /// </summary>
    public Optional<DateTimeOffset> JoinedAt { get; }

    /// <summary>
    /// Stores when this user started boosting this guild.
    /// </summary>
    public Optional<DateTimeOffset?> PremiumSince { get; }

    /// <summary>
    /// Indicates whether this user is server deafened.
    /// </summary>
    public Optional<bool> Deaf { get; }

    /// <summary>
    /// Indicates whether this user is server muted.
    /// </summary>
    public Optional<bool> Mute { get; }

    /// <summary>
    /// Additional flags for this guild member.
    /// </summary>
    public Optional<DiscordGuildMemberFlags> Flags { get; }

    /// <summary>
    /// Indicates whether the user is in the process of passing the guild membership
    /// screening requirements.
    /// </summary>
    public Optional<bool> Pending { get; }

    /// <summary>
    /// Total permissions of this guild member including channel overwrites. This is only set in
    /// interaction-related objects.
    /// </summary>
    public Optional<DiscordPermissions> Permissions { get; }

    /// <summary>
    /// The timestamp at which this user's timeout expires.
    /// </summary>
    public Optional<DateTimeOffset?> CommunicationDisabledUntil { get; }
}
