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
<<<<<<< HEAD
        public ulong ChannelID;
=======
        internal ulong ChannelID;
>>>>>>> master
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
    }
}
