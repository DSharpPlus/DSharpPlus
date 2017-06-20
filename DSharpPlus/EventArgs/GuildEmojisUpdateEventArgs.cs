using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildEmojisUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// New emoji list
        /// </summary>
        public IReadOnlyList<DiscordEmoji> Emojis { get; internal set; }
        /// <summary>
        /// Old emoji list
        /// </summary>
        public IReadOnlyList<DiscordEmoji> EmojisBefore { get; internal set; }
        /// <summary>
        /// Guild these emojis belong to
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public GuildEmojisUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
