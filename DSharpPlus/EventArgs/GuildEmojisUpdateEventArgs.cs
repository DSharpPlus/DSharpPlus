using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildEmojisUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the list of emojis after the change.
        /// </summary>
        public IReadOnlyList<DiscordEmoji> EmojisAfter { get; internal set; }

        /// <summary>
        /// Gets the list of emojis before the change.
        /// </summary>
        public IReadOnlyList<DiscordEmoji> EmojisBefore { get; internal set; }

        /// <summary>
        /// Guild these emojis belong to
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public GuildEmojisUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
