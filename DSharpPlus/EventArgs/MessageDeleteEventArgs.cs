using System;

namespace DSharpPlus
{
    public class MessageDeleteEventArgs : EventArgs
    {
        public ulong MessageID { get; internal set; }
        public ulong ChannelID { get; internal set; }
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
    }
}
