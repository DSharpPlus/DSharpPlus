using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

using Caching;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessageReactionRemovedEmoji"/>
/// </summary>
public sealed class MessageReactionRemoveEmojiEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the channel the removed reactions were in.
    /// </summary>
    public CachedEntity<ulong, DiscordChannel> Channel { get; internal set; }

    /// <summary>
    /// Gets the guild the removed reactions were in. This value is null if the message was in DMs.
    /// </summary>
    public CachedEntity<ulong, DiscordGuild>? Guild { get; internal set; }

    /// <summary>
    /// Gets the message that had the removed reactions.
    /// </summary>
    public CachedEntity<ulong, DiscordMessage> Message { get; internal set; }

    /// <summary>
    /// Gets the emoji of the reaction that was removed.
    /// </summary>
    public DiscordEmoji Emoji { get; internal set; }

    internal MessageReactionRemoveEmojiEventArgs() : base() { }
}
