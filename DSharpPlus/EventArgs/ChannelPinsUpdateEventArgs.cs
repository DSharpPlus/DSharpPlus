using System;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.ChannelPinsUpdated"/> event.
    /// </summary>
    public class ChannelPinsUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the channel in which the update occured.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the timestamp of the latest pin.
        /// </summary>
        public DateTimeOffset LastPinTimestamp { get; internal set; }

        internal ChannelPinsUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
