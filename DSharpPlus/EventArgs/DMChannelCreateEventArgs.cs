using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.DmChannelCreated"/> event.
    /// </summary>
    public class DmChannelCreateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the direct message channel that was created.
        /// </summary>
        public DiscordDmChannel Channel { get; internal set; }

        internal DmChannelCreateEventArgs() : base() { }
    }
}
