using System;

namespace DSharpPlus
{
    public class MessageReactionRemoveEventArgs : EventArgs
    {
        public ulong UserID { get; internal set; }
        public ulong MessageID { get; internal set; }
        public ulong ChannelID { get; internal set; }
        public DiscordEmoji Emoji { get; internal set; }
        public DiscordUser User => this.Emoji.Discord._rest_client.InternalGetUser(UserID).GetAwaiter().GetResult();
        public DiscordMessage Message => this.Emoji.Discord._rest_client.InternalGetMessage(ChannelID, MessageID).GetAwaiter().GetResult();
        public DiscordChannel Channel => this.Emoji.Discord._rest_client.InternalGetChannel(ChannelID).GetAwaiter().GetResult();
    }
}
