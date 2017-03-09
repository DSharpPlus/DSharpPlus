using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildEmojisUpdateEventArgs : EventArgs
    {
        public IReadOnlyCollection<DiscordEmoji> Emojis { get; internal set; }
        internal ulong GuildID { get; set; }
        public DiscordGuild Guild { get; internal set; }
    }
}
