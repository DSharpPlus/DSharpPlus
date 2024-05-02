using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessageReactionAdded"/> event.
/// </summary>
public class MessageReactionAddEventArgs : DiscordEventArgs
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
        => Message.Channel;

    /// <summary>
    /// Gets the guild in which the reaction was added.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// Gets the user who created the reaction.
    /// <para>This can be cast to a <see cref="DiscordMember"/> if the reaction was in a guild.</para>
    /// </summary>
    public DiscordUser User { get; internal set; }

    /// <summary>
    /// Gets the emoji used for this reaction.
    /// </summary>
    public DiscordEmoji Emoji { get; internal set; }

    internal MessageReactionAddEventArgs() : base() { }
}
