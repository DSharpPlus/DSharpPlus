using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.MessageReactionRemoved"/> event.
    /// </summary>
    public class MessageReactionRemoveEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the message for which the update occured.
        /// </summary>
        public DiscordMessage Message { get; internal set; }

        /// <summary>
        /// Gets the channel to which this message belongs.
        /// </summary>
        /// <remarks>
        /// This will be <c>null</c> for an uncached channel, which will usually happen for when this event triggers on
        /// DM channels in which no prior messages were received or sent.
        /// </remarks>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the ID of the channel to which this message belongs. This property can be used even when
        /// <see cref="Channel"/> is <c>null</c>.
        /// </summary>
        public ulong ChannelId { get; internal set; }

        /// <summary>
        /// Gets the users whose reaction was removed.
        /// </summary>
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the emoji used for this reaction.
        /// </summary>
        public DiscordEmoji Emoji { get; internal set; }

        internal MessageReactionRemoveEventArgs(DiscordClient client) : base(client) { }
    }
}