using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

using Caching;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessageReactionRemoved"/> event.
/// </summary>
public class MessageReactionRemoveEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the message for which the update occurred.
    /// </summary>
    public CachedEntity<ulong, DiscordMessage> Message { get; internal set; }

    /// <summary>
    /// Gets the channel to which this message belongs.
    /// </summary>
    public CachedEntity<ulong, DiscordChannel> Channel { get; internal set; }

    /// <summary>
    /// Gets the users whose reaction was removed. This can be a <see cref="DiscordMember"/> if the reaction was in a guild and the member was in cache.
    /// </summary>
    public CachedEntity<ulong, DiscordUser> User { get; internal set; }

    /// <summary>
    /// Gets the guild in which the reaction was deleted. This value is null if message was in DMs.
    /// </summary>
    public CachedEntity<ulong, DiscordGuild>? Guild { get; internal set; }

    /// <summary>
    /// Gets the emoji used for this reaction.
    /// </summary>
    public DiscordEmoji Emoji { get; internal set; }

    internal MessageReactionRemoveEventArgs() : base() { }
}
