using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class MessageReactionAddEventArgs : System.EventArgs
    {
        public ulong UserID;
        public ulong MessageID;
        public ulong ChannelID;
        public DiscordEmoji Emoji;
        public DiscordUser User => DiscordClient.InternalGetUser(UserID).Result;
        public DiscordMessage Message => DiscordClient.InternalGetMessage(ChannelID, MessageID).Result;
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
    }
}
