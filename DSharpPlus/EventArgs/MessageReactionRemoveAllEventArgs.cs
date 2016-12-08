using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class MessageReactionRemoveAllEventArgs : EventArgs
    {
        public ulong ChannelID;
        public ulong MessageID;
        public DiscordMessage Message => DiscordClient.InternalGetMessage(ChannelID, MessageID).Result;
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
    }
}
