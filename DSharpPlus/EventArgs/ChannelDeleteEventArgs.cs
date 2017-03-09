using System;

namespace DSharpPlus
{
    public class ChannelDeleteEventArgs : EventArgs
    {
        public DiscordChannel Channel { get; internal set; }
        public DiscordGuild Guild { get; internal set; }
    }
}
