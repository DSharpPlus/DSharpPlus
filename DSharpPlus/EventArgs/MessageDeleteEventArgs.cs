using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class MessageDeleteEventArgs : System.EventArgs
    {
        public ulong MessageID;
        public ulong ChannelID;
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
    }
}
