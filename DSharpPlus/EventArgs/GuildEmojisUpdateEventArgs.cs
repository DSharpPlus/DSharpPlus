using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildEmojisUpdateEventArgs : DiscordEventArgs
    {
        public IReadOnlyList<DiscordEmoji> EmojisAfter { get; internal set; }
        public IReadOnlyList<DiscordEmoji> EmojisBefore { get; internal set; }
        /// <summary>
        /// Guild these emojis belong to
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public GuildEmojisUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
