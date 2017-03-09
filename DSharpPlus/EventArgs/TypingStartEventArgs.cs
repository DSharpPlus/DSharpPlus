using System;

namespace DSharpPlus
{
    public class TypingStartEventArgs : EventArgs
    {
        public ulong ChannelID { get; internal set; }
        public ulong UserID { get; internal set; }
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
        public DiscordUser User => DiscordClient.InternalGetUser(UserID.ToString()).Result;
    }
}
