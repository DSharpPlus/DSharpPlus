using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildEmojisUpdateEventArgs : DiscordEventArgs
    {
        public IReadOnlyList<DiscordEmoji> Emojis { get; internal set; }
        public IReadOnlyList<DiscordEmoji> EmojisBefore { get; internal set; }
        internal ulong GuildID { get; set; }
        public DiscordGuild Guild { get; internal set; }

        public GuildEmojisUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
