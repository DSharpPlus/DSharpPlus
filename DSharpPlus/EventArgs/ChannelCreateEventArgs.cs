using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class ChannelCreateEventArgs : System.EventArgs
    {
        public DiscordChannel Channel;
        public DiscordGuild Guild;
    }
}
