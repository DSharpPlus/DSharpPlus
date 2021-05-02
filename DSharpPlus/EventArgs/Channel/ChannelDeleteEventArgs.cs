using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.ChannelDeleted"/> event.
    /// </summary>
    public class ChannelDeleteEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the channel that was deleted.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the guild this channel belonged to.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        internal ChannelDeleteEventArgs() : base() { }
    }
}
