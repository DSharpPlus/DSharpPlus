using System;

namespace DSharpPlus
{
    public class TypingStartEventArgs : EventArgs
    {
        internal DiscordClient Discord { get; set; }
        public ulong ChannelID { get; internal set; }
        public ulong UserID { get; internal set; }
        public DiscordChannel Channel => this.Discord._rest_client.InternalGetChannel(ChannelID).GetAwaiter().GetResult();
        public DiscordUser User => this.Discord._rest_client.InternalGetUser(UserID.ToString()).GetAwaiter().GetResult();
    }
}
