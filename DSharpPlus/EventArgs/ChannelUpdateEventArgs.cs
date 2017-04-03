using System;

namespace DSharpPlus
{
    public class ChannelUpdateEventArgs : EventArgs
    {
        public DiscordChannel Channel { get; internal set; }
        public DiscordGuild Guild { get; internal set; }
        public DiscordChannel ChannelBefore { get; internal set; }
    }
}
