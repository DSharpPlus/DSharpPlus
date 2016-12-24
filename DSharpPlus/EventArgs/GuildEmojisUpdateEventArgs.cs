using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildEmojisUpdateEventArgs : EventArgs
    {
        public List<DiscordEmoji> Emojis;
        internal ulong GuildID;
        public DiscordGuild Guild;
    }
}
