using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class ChannelDeleteEventArgs : System.EventArgs
    {
        public DiscordChannel Channel;
        public DiscordGuild Guild;
    }
}
