using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity
{
    public class PaginationEmojis
    {
		public DiscordEmoji Left { get; set; }
		public DiscordEmoji Right { get; set; }
		public DiscordEmoji Stop { get; set; }
		public DiscordEmoji SkipLeft { get; internal set; }
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
