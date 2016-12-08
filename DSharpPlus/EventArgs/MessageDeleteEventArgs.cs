using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class MessageDeleteEventArgs : EventArgs
    {
        public ulong MessageID;
        public ulong ChannelID;
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
    }
}
