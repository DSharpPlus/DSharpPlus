﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;

namespace DSharpPlus
{
    /// <summary>
    /// Contains markdown formatting helpers.
    /// </summary>
    public static class Formatter
    {
        private static Regex MdSanitizeRegex { get; } = new Regex(@"([`\*_~<>\[\]\(\)""@\!\&#:])", RegexOptions.ECMAScript);
        private static Regex MdStripRegex { get; } = new Regex(@"([`\*_~\[\]\(\)""]|<@\!?\d+>|<#\d+>|<@\&\d+>|<:[a-zA-Z0-9_\-]:\d+>)", RegexOptions.ECMAScript);

        /// <summary>
        /// Creates a block of code.
        /// </summary>
        /// <param name="content">Contents of the block.</param>
        /// <param name="language">Language to use for highlighting.</param>
        /// <returns>Formatted block of code.</returns>
        public static string BlockCode(string content, string language = "") 
            => $"```{language}\n{content}\n```";

        /// <summary>
        /// Creates inline code snippet.
        /// </summary>
        /// <param name="content">Contents of the snippet.</param>
        /// <returns>Formatted inline code snippet.</returns>
        public static string InlineCode(string content) 
            => $"`{content}`";

        /// <summary>
        /// Creates bold text.
        /// </summary>
        /// <param name="content">Text to bolden.</param>
        /// <returns>Formatted text.</returns>
        public static string Bold(string content) 
            => $"**{content}**";

        /// <summary>
        /// Creates italicized text.
        /// </summary>
        /// <param name="content">Text to italicize.</param>
        /// <returns>Formatted text.</returns>
        public static string Italic(string content) 
            => $"*{content}*";

        /// <summary>
        /// Creates underlined text.
        /// </summary>
        /// <param name="content">Text to underline.</param>
        /// <returns>Formatted text.</returns>
        public static string Underline(string content) 
            => $"__{content}__";

        /// <summary>
        /// Creates strikethrough text.
        /// </summary>
        /// <param name="content">Text to strikethrough.</param>
        /// <returns>Formatted text.</returns>
        public static string Strike(string content) 
            => $"~~{content}~~";

        /// <summary>
        /// Creates a URL that won't create a link preview.
        /// </summary>
        /// <param name="url">Url to prevent from being previewed.</param>
        /// <returns>Formatted url.</returns>
        public static string EmbedlessUrl(Uri url) 
            => $"<{url.ToString()}>";

        /// <summary>
        /// Creates a masked link. This link will display as specified text, and alternatively provided alt text. This can only be used in embeds.
        /// </summary>
        /// <param name="text">Text to display the link as.</param>
        /// <param name="url">Url that the link will lead to.</param>
        /// <param name="alt_text">Alt text to display on hover.</param>
        /// <returns>Formatted url.</returns>
        public static string MaskedUrl(string text, Uri url, string alt_text = "") 
            => $"[{text}]({url}{(!string.IsNullOrWhiteSpace(alt_text) ? $" \"{alt_text}\"" : "")})";

        /// <summary>
        /// Escapes all markdown formatting from specified text.
        /// </summary>
        /// <param name="text">Text to sanitize.</param>
        /// <returns>Sanitized text.</returns>
        public static string Sanitize(string text) 
            => MdSanitizeRegex.Replace(text, m => $"\\{m.Groups[1].Value}");

        /// <summary>
        /// Removes all markdown formatting from specified text.
        /// </summary>
        /// <param name="text">Text to strip of formatting.</param>
        /// <returns>Formatting-stripped text.</returns>
        public static string Strip(string text) 
            => MdStripRegex.Replace(text, m => string.Empty);

        /// <summary>
        /// Creates a mention for specified user or member. Can optionally specify to resolve nicknames.
        /// </summary>
        /// <param name="user">User to create mention for.</param>
        /// <param name="nickname">Whether the mention should resovle nicknames or not.</param>
        /// <returns>Formatted mention.</returns>
        public static string Mention(DiscordUser user, bool nickname = false) 
            => (nickname ? $"<@!{user.Id.ToString(CultureInfo.InvariantCulture)}>" : $"<@{user.Id.ToString(CultureInfo.InvariantCulture)}>");

        /// <summary>
        /// Creates a mention for specified channel.
        /// </summary>
        /// <param name="channel">Channel to mention.</param>
        /// <returns>Formatted mention.</returns>
        public static string Mention(DiscordChannel channel) 
            => $"<#{channel.Id.ToString(CultureInfo.InvariantCulture)}>";

        /// <summary>
        /// Creates a mention for specified role.
        /// </summary>
        /// <param name="role">Role to mention.</param>
        /// <returns>Formatted mention.</returns>
        public static string Mention(DiscordRole role) 
            => $"<@&{role.Id.ToString(CultureInfo.InvariantCulture)}>";

        /// <summary>
        /// Creates a custom emoji string.
        /// </summary>
        /// <param name="emoji">Emoji to display.</param>
        /// <returns>Formatted emoji.</returns>
        public static string Emoji(DiscordEmoji emoji) 
            => $"<:{emoji.Name}:{emoji.Id.ToString(CultureInfo.InvariantCulture)}>";

        /// <summary>
        /// Creates a url for using attachments in embeds. This can only be used as an Image URL, Thumbnail URL, Author icon URL or Footer icon URL.
        /// </summary>
        /// <param name="filename">Name of attached image to display</param>
        /// <returns></returns>
        public static string AttachedImageUrl(string filename) 
            => $"attachment://{filename}";
    }
}
