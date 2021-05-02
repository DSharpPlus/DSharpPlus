using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.MessageDeleted"/> event.
    /// </summary>
    public class MessageDeleteEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the message that was deleted.
        /// </summary>
        public DiscordMessage Message { get; internal set; }

        /// <summary>
        /// Gets the channel this message belonged to.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the guild this message belonged to.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        internal MessageDeleteEventArgs() : base() { }
    }
}
