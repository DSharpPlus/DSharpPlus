using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class ChannelUpdateEventArgs : System.EventArgs
    {
        public DiscordChannel Channel;
        public DiscordGuild Guild;
    }
}
