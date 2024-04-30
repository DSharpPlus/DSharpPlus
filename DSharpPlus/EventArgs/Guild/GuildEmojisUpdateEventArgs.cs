namespace DSharpPlus.EventArgs;
using System.Collections.Generic;
using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildEmojisUpdated"/> event.
/// </summary>
public class GuildEmojisUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the list of emojis after the change.
    /// </summary>
    public IReadOnlyDictionary<ulong, DiscordEmoji> EmojisAfter { get; internal set; }

    /// <summary>
    /// Gets the list of emojis before the change.
    /// </summary>
    public IReadOnlyDictionary<ulong, DiscordEmoji> EmojisBefore { get; internal set; }

    /// <summary>
    /// Gets the guild in which the update occurred.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal GuildEmojisUpdateEventArgs() : base() { }
}
