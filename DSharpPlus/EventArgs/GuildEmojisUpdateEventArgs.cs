using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.GuildEmojisUpdated"/> event.
    /// </summary>
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
        /// Gets the guild in which the update occured.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        internal GuildEmojisUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
