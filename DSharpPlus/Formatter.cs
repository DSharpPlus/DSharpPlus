using System;

namespace DSharpPlus
{
    public static class Formatter
    {
        public static string BlockCode(string content, string language = "") => $"```{language}\n{content}\n```";
        public static string InlineCode(string content) => $"`{content}`";
        public static string Bold(string content) => $"**{content}**";
        public static string Italic(string content) => $"*{content}*";
        public static string Underline(string content) => $"__{content}__";
        public static string Strike(string content) => $"~~{content}~~";
        public static string EmbedlessUrl(Uri url) => $"<{url.ToString()}>";
        public static string MaskedUrl(string text, Uri url, string alt_text = "") => string.Concat("[", text, "](", url.ToString(), !string.IsNullOrWhiteSpace(alt_text) ? $" \"{alt_text}\"" : "", ")");

        public static string Mention(DiscordUser user, bool nickname = false) => (nickname ? $"<@!{user.Id}>" : $"<@{user.Id}>");
        public static string Mention(DiscordChannel channel) => $"<#{channel.Id}>";
        public static string Mention(DiscordRole role) => $"<@&{role.Id}>";

        public static string Emoji(DiscordEmoji emoji) => $"<:{emoji.Name}:{emoji.Id}>";
    }
}
