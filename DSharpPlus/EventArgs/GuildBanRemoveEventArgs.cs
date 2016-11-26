using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class GuildBanRemoveEventArgs : EventArgs
    {
        public DiscordUser User;
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
    }
}
