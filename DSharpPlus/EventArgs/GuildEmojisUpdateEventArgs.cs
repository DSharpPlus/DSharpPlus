using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class GuildEmojisUpdateEventArgs : EventArgs
    {
        public List<DiscordEmoji> Emojis;
        internal ulong GuildID;
        public DiscordGuild Guild;
    }
}
