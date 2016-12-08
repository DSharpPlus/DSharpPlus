using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class MessageReactionRemoveEventArgs : EventArgs
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
