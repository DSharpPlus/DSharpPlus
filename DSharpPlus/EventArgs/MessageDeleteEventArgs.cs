using System;

namespace DSharpPlus
{
    public class MessageDeleteEventArgs : EventArgs
    {
        public ulong MessageID;
        public ulong ChannelID;
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
    }
}
