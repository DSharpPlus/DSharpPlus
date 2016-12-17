using System.Collections.Generic;
using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class GuildEmojisUpdateEventArgs : System.EventArgs
    {
        public List<DiscordEmoji> Emojis;
        internal ulong GuildID;
        public DiscordGuild Guild;
    }
}
