using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity
{
    /// <summary>
    /// Emojis to use for pagination
    /// </summary>
    public class PaginationEmojis
    {
        /// <summary>
        /// Emoji for going back 1 page in the message
        /// </summary>
		public DiscordEmoji Left { get; set; }
        /// <summary>
        /// Emoji for going forward 1 page in the message
        /// </summary>
		public DiscordEmoji Right { get; set; }
        /// <summary>
        /// Emoji to stop pagination
        /// </summary>
		public DiscordEmoji Stop { get; set; }
        /// <summary>
        /// Emoji to skip all the way to the first page
        /// </summary>
		public DiscordEmoji SkipLeft { get; internal set; }
        /// <summary>
        /// Emoji to skip all the way to the last page
        /// </summary>
        public DiscordEmoji SkipRight { get; internal set; }

		public PaginationEmojis(DiscordClient client)
		{
			Left = DiscordEmoji.FromUnicode(client, "◀");
			Right = DiscordEmoji.FromUnicode(client, "▶");
			SkipLeft = DiscordEmoji.FromUnicode(client, "⏮");
			SkipRight = DiscordEmoji.FromUnicode(client, "⏭");
			Stop = DiscordEmoji.FromUnicode(client, "⏹");
		}
	}
}
