using System;

namespace DSharpPlus
{
    public class ChannelDeleteEventArgs : EventArgs
    {
        public DiscordChannel Channel;
        public DiscordGuild Guild;
    }
}
