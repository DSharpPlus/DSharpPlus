using System;

namespace DSharpPlus
{
    public class ChannelUpdateEventArgs : EventArgs
    {
        public DiscordChannel Channel;
        public DiscordGuild Guild;
    }
}
