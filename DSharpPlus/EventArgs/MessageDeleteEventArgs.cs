using System;

namespace DSharpPlus
{
    public class MessageDeleteEventArgs : EventArgs
    {
        internal DiscordClient Discord { get; set; }
        public ulong MessageID { get; internal set; }
        public ulong ChannelID { get; internal set; }
        public DiscordChannel Channel => this.Discord._rest_client.InternalGetChannel(ChannelID).GetAwaiter().GetResult();
    }
}
