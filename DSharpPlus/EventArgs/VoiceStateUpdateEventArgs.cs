using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class VoiceStateUpdateEventArgs : EventArgs
    {
        public ulong UserID;
        public DiscordUser User => DiscordClient.InternalGetUser(UserID.ToString()).Result;
        internal string SessionID;
    }
}
