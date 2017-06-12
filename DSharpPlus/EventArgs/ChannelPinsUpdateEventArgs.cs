using System;

namespace DSharpPlus
{
    public class ChannelPinsUpdateEventArgs : DiscordEventArgs
    {
        public DiscordChannel Channel { get; internal set; }
        public DateTimeOffset LastPinTimestamp { get; internal set; }

        public ChannelPinsUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
