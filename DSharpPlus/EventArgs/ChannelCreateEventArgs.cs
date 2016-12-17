using System;

namespace DSharpPlus
{
    public class ChannelCreateEventArgs : EventArgs
    {
        public DiscordChannel Channel;
        public DiscordGuild Guild;
    }
}
