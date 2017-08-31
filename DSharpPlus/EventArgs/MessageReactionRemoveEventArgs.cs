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
        public DiscordChannel Channel { get; internal set; }

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
