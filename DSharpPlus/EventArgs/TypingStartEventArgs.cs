using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class TypingStartEventArgs : EventArgs
    {
        public ulong ChannelID;
        public ulong UserID;
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
        public DiscordUser User => DiscordClient.InternalGetUser(UserID.ToString()).Result;
    }
}
