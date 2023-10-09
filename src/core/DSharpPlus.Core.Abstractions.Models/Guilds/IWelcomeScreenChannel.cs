// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a single shown in the welcome screen.
/// </summary>
public interface IWelcomeScreenChannel
{
    /// <summary>
    /// The snowflake identifier of the channel.
    /// </summary>
    public Snowflake ChannelId { get; }

    /// <summary>
    /// The description shown for this channel.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The snowflake identifier of the associated emoji, if this is a custom emoji.
    /// </summary>
    public Snowflake? EmojiId { get; }

    /// <summary>
    /// The emoji name of the associated emoji if this is a custom emoji; the unicode character if this is
    /// a default emoji, or <see langword="null"/> if no emoji is set.
    /// </summary>
    public string? EmojiName { get; }
}
