using System;

namespace DSharpPlus
{
    public class MessageReactionRemoveAllEventArgs : EventArgs
    {
        internal DiscordClient Discord { get; set; }
        public ulong ChannelID { get; internal set; }
        public ulong MessageID { get; internal set; }
        public DiscordMessage Message => this.Discord._rest_client.InternalGetMessage(ChannelID, MessageID).GetAwaiter().GetResult();
        public DiscordChannel Channel => this.Discord._rest_client.InternalGetChannel(ChannelID).GetAwaiter().GetResult();
    }
}
