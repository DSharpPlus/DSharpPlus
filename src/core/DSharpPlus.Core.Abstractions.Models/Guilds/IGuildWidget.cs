// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a guild's widget object.
/// </summary>
public interface IGuildWidget
{
    /// <summary>
    /// The snowflake identifier of the guild this widget belongs to.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The name of the guild, 2 to 100 characters.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// An instant invite code for the guild's specified widget invite channel.
    /// </summary>
    public string? InstantInvite { get; }

    /// <summary>
    /// Voice and stage channels accessible by everyone.
    /// </summary>
    public IReadOnlyList<IPartialChannel> Channels { get; }

    /// <summary>
    /// Up to 100 users including their presences.
    /// </summary>
    public IReadOnlyList<IPartialUser> Members { get; }

    /// <summary>
    /// The number of online members in this guild.
    /// </summary>
    public int PresenceCount { get; }
}
