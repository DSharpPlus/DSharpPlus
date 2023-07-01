// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a guild preview object.
/// </summary>
public interface IGuildPreview
{
    /// <summary>
    /// The snowflake identifier of the guild this preview belongs to.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The name of this guild, between 2 and 100 characters.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The icon hash for this guild.
    /// </summary>
    public string? Icon { get; }

    /// <summary>
    /// The splash hash for this guild.
    /// </summary>
    public string? Splash { get; }

    /// <summary>
    /// The discovery splash hash for this guild.
    /// </summary>
    public string? DiscoverySplash { get; }

    /// <summary>
    /// The custom emojis in this guild.
    /// </summary>
    public IReadOnlyList<IEmoji> Emojis { get; }

    /// <summary>
    /// The enabled guild features.
    /// </summary>
    public IReadOnlyList<string> Features { get; }

    /// <summary>
    /// The approximate amount of members in this guild.
    /// </summary>
    public int ApproximateMemberCount { get; }

    /// <summary>
    /// The approximate amount of online members in this guild.
    /// </summary>
    public int ApproximatePresenceCount { get; }

    /// <summary>
    /// The description of this guild.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// The custom stickers in this guild.
    /// </summary>
    public IReadOnlyList<ISticker> Stickers { get; }
}
