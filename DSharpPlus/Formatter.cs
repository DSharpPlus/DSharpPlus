using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public static class Formatter
    {
        public static string BlockCode(string content, string language = "") => $"```{language}\n{content}\n```";
        public static string InlineCode(string content) => $"``{content}``";
        public static string Bold(string content) => $"**{content}**";
        public static string Italic(string content) => $"*{content}*";
        public static string Underline(string content) => $"_{content}_";
        public static string Strike(string content) => $"~{content}~";

        public static string Mention(DiscordMember member, bool nickname = false) => Mention(member.User, nickname);
        public static string Mention(DiscordUser user, bool nickname = false) => (nickname ? $"<@!{user.ID}>" : $"<@{user.ID}>");
        public static string Mention(DiscordChannel channel) => $"<#{channel.ID}>";
        public static string Mention(DiscordRole role) => $"<@&{role.ID}>";

        public static string Emoji(DiscordEmoji emoji) => $"<:{emoji.Name}:{emoji.ID}>";
    }
}
