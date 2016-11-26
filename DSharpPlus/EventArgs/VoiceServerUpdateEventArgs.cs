using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class VoiceServerUpdateEventArgs : EventArgs
    {
        internal string VoiceToken;
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public string Endpoint;
    }
}
