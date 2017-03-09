using System;

namespace DSharpPlus
{
    public class MessageReactionRemoveAllEventArgs : EventArgs
    {
        public ulong ChannelID { get; internal set; }
        public ulong MessageID { get; internal set; }
        public DiscordMessage Message => DiscordClient.InternalGetMessage(ChannelID, MessageID).Result;
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
    }
}
