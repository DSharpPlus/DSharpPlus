using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessageReactionRemoved"/> event.
/// </summary>
public class MessageReactionRemoveEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the message for which the update occurred.
    /// </summary>
    public DiscordMessage Message { get; internal set; }

    /// <summary>
    /// Gets the channel to which this message belongs.
    /// </summary>
    /// <remarks>
    /// This will be <c>null</c> for an uncached channel, which will usually happen for when this event triggers on
    /// DM channels in which no prior messages were received or sent.
    /// </remarks>
    public DiscordChannel Channel
        => this.Message.Channel;

    /// <summary>
    /// Gets the users whose reaction was removed.
    /// </summary>
    public DiscordUser User { get; internal set; }

    /// <summary>
    /// Gets the guild in which the reaction was deleted.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// Gets the emoji used for this reaction.
    /// </summary>
    public DiscordEmoji Emoji { get; internal set; }

    internal MessageReactionRemoveEventArgs() : base() { }
}
