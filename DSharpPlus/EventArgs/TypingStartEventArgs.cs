using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class TypingStartEventArgs : EventArgs
    {
<<<<<<< HEAD
        public ulong ChannelID;
        public ulong UserID;
=======
        internal ulong ChannelID;
        internal ulong UserID;
>>>>>>> master
        public DiscordChannel Channel => DiscordClient.InternalGetChannel(ChannelID).Result;
        public DiscordUser User => DiscordClient.InternalGetUser(UserID.ToString()).Result;
    }
}
