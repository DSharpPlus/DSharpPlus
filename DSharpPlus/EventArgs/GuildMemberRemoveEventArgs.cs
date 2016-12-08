using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class GuildMemberRemoveEventArgs
    {
<<<<<<< HEAD
        public ulong GuildID;
=======
        internal ulong GuildID;
>>>>>>> master
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public DiscordUser User;
    }
}
