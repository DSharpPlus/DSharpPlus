using System;

namespace DSharpPlus
{
    public class ChannelPinsUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Channel that just got its pins updated
        /// </summary>
        public DiscordChannel Channel { get; internal set; }
        /// <summary>
        /// Timestamp of latest pin
        /// </summary>
        public DateTimeOffset LastPinTimestamp { get; internal set; }

        public ChannelPinsUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
