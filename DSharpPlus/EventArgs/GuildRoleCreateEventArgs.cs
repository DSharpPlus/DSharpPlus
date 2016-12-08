using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class GuildRoleCreateEventArgs : EventArgs
    {
<<<<<<< HEAD
        public ulong GuildID;
=======
        internal ulong GuildID;
>>>>>>> master
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public DiscordRole Role;
    }
}
