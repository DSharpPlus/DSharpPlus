using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.MessageReactionsCleared"/> event.
    /// </summary>
    public class MessageReactionsClearEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the message for which the update occured.
        /// </summary>
        public DiscordMessage Message { get; internal set; }

        /// <summary>
        /// Gets the channel to which this message belongs.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        internal MessageReactionsClearEventArgs(DiscordClient client) : base(client) { }
    }
}
