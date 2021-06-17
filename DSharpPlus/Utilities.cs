// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;

namespace DSharpPlus
{
    /// <summary>
    /// Various Discord-related utilities.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Gets the version of the library
        /// </summary>
        private static string VersionHeader { get; set; }
        private static Dictionary<Permissions, string> PermissionStrings { get; set; }

        internal static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);

        static Utilities()
        {
            PermissionStrings = new Dictionary<Permissions, string>();
            var t = typeof(Permissions);
            var ti = t.GetTypeInfo();
            var vals = Enum.GetValues(t).Cast<Permissions>();

            foreach (var xv in vals)
            {
                var xsv = xv.ToString();
                var xmv = ti.DeclaredMembers.FirstOrDefault(xm => xm.Name == xsv);
                var xav = xmv.GetCustomAttribute<PermissionStringAttribute>();

                PermissionStrings[xv] = xav.String;
            }

            var a = typeof(DiscordClient).GetTypeInfo().Assembly;

            var vs = "";
            var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (iv != null)
                vs = iv.InformationalVersion;
            else
            {
                var v = a.GetName().Version;
                vs = v.ToString(3);
            }

            VersionHeader = $"DiscordBot (https://github.com/DSharpPlus/DSharpPlus, v{vs})";
        }

        internal static string GetApiBaseUri()
            => Endpoints.BASE_URI;

        internal static Uri GetApiUriFor(string path)
            => new($"{GetApiBaseUri()}{path}");

        internal static Uri GetApiUriFor(string path, string queryString)
            => new($"{GetApiBaseUri()}{path}{queryString}");

        internal static QueryUriBuilder GetApiUriBuilderFor(string path)
            => new($"{GetApiBaseUri()}{path}");

        internal static string GetFormattedToken(BaseDiscordClient client) => GetFormattedToken(client.Configuration);

        internal static string GetFormattedToken(DiscordConfiguration config)
        {
            return config.TokenType switch
            {
                TokenType.Bearer => $"Bearer {config.Token}",
                TokenType.Bot => $"Bot {config.Token}",
                _ => throw new ArgumentException("Invalid token type specified.", nameof(config.Token)),
            };
        }

        internal static Dictionary<string, string> GetBaseHeaders()
            => new();

        internal static string GetUserAgent()
            => VersionHeader;

        internal static bool ContainsUserMentions(string message)
        {
            var pattern = @"<@(\d+)>";
            var regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(message);
        }

        internal static bool ContainsNicknameMentions(string message)
        {
            var pattern = @"<@!(\d+)>";
            var regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(message);
        }

        internal static bool ContainsChannelMentions(string message)
        {
            var pattern = @"<#(\d+)>";
            var regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(message);
        }

        internal static bool ContainsRoleMentions(string message)
        {
            var pattern = @"<@&(\d+)>";
            var regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(message);
        }

        internal static bool ContainsGuildEmojis(string message)
        {
            var pattern = @"<a?:(.*):(\d+)>";
            var regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(message);
        }

        internal static IEnumerable<ulong> GetUserMentions(DiscordMessage message)
        {
            var regex = new Regex(@"<@!?(\d+)>", RegexOptions.ECMAScript);
            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
                yield return ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        internal static IEnumerable<ulong> GetRoleMentions(DiscordMessage message)
        {
            var regex = new Regex(@"<@&(\d+)>", RegexOptions.ECMAScript);
            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
                yield return ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        internal static IEnumerable<ulong> GetChannelMentions(DiscordMessage message)
        {
            var regex = new Regex(@"<#(\d+)>", RegexOptions.ECMAScript);
            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
                yield return ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        internal static IEnumerable<(ulong id, string name, bool isAnimated)> GetGuildEmojis(DiscordMessage message)
        {
            var regex = new Regex(@"<a?:([a-zA-Z0-9_]+):(\d+)>", RegexOptions.ECMAScript);
            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
                yield return (ulong.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
                              match.Groups[1].Value,
                              match.Value.StartsWith("<a:"));
        }

        public static IEnumerable<DiscordEmoji> GetCommonEmojis(DiscordMessage message)
        {
            return DiscordEmoji.UnicodeEmojis.Keys
                .Where(e => message.Content.Contains(e))
                .Select(e => DiscordEmoji.FromName(message.Discord, e, false))
                .Union(DiscordEmoji.DiscordNameLookup.Keys
                    .Where(e => message.Content.Contains(e))
                    .Select(e => DiscordEmoji.FromUnicode(e)))
                .ToList();
        }

        /// <summary>
        /// Converts all of the Discord message snowflake tags to a human-readable format
        /// </summary>
        /// <param name="message">The message to convert</param>
        /// <returns>A human-readable format of the message</returns>
        public static Task<string> HumanizeContentAsync(DiscordMessage message)
            => HumanizeContentAsync(message, (e)
                => (e != null && e.Id == 0) ? Task.FromResult(e.Name) : Task.FromResult(string.Empty));

        /// <summary>
        /// Converts all of the Discord message snowflake tags to a human-readable format
        /// </summary>
        /// <param name="message">The message to convert</param>
        /// <param name="emojiReplacer">A callback to specify how emojis should be converted</param>
        /// <returns>A human-readable format of the message</returns>
        public async static Task<string> HumanizeContentAsync(DiscordMessage message, Func<DiscordEmoji, Task<string>> emojiReplacer)
        {
            // Preparing the parsing by retrieving all the guild emojis used
            var guildEmojis = Utilities.GetGuildEmojis(message)
                .Select(emojidata =>
                {
                    var result = DiscordEmoji.TryFromGuildEmote(message.Discord, emojidata.id, out var emoji);
                    // If the emoji wasn't found, we still can construct it's URL from the data we got, so create a dummy object, and mark it as unavailable
                    return result ? emoji : new DiscordEmoji
                    {
                        Id = emojidata.id,
                        Name = emojidata.name,
                        IsAvailable = false,
                        IsAnimated = emojidata.isAnimated,
                        Discord = message.Discord,
                        RequiresColons = true
                    };
                });

            // First we take care of "special tags (<>), including guild emojies"
            var regex = new Regex(@"<(?<TagType>(@[!&]?)|(#)|(a?:([a-zA-Z0-9_]+):))(?<TagId>\d+)>", RegexOptions.ECMAScript);
            var text = await regex.ReplaceAsync(message.Content, async m =>
            {
                var tagType = m.Groups["TagType"].Value;
                var tagId = ulong.Parse(m.Groups["TagId"].Value);
                switch (tagType)
                {
                    case "@": // Username mention
                        return $"@{message.MentionedUsers.FirstOrDefault(u => u.Id == tagId).Username}";

                    case "@!": // Nickname mention
                        var user = message.MentionedUsers.FirstOrDefault(u => u.Id == tagId);
                        if (user is DiscordMember member)
                        {
                            return $"@{member.DisplayName}";
                        }
                        if (message.Channel?.Guild != null)
                        {
                            member = await message.Channel.Guild.GetMemberAsync(tagId);
                            return $"@{member.DisplayName}";
                        }
                        return $"@{user.Username}";

                    case "@&": // Role mention
                        return $"@{message.MentionedRoles.FirstOrDefault(r => r.Id == tagId).Name}";

                    case "#": // Channel mention
                        return $"#{message.MentionedChannels.FirstOrDefault(c => c.Id == tagId).Name}";

                    case string s when s.StartsWith("a:") || s.StartsWith(":"): // Guild emoji
                        var emoji = guildEmojis.FirstOrDefault(e => e.Id == tagId);
                        return await emojiReplacer(emoji);

                    default:
                        throw new ArgumentException("Matched something unexpected");
                }
            });

            // Now we take care of common emojis
            // We sort them so we first replace the most complex ("composite") ones first

            // Converting the ":emojiname:" format
            var commonEmojis = DiscordEmoji.UnicodeEmojis.Keys
                .Where(e => text.Contains(e))
                .OrderByDescending(e => e.Count(c => c == ':'))
                .ThenByDescending(e => e.Length);

            foreach (var emojiName in commonEmojis)
            {
                var emoji = DiscordEmoji.FromName(message.Discord, emojiName, false);
                var replacement = await emojiReplacer(emoji);
                text = text.Replace(emojiName, replacement);
            }

            // Converting the unicode emojis
            var unicodeEmojis = DiscordEmoji.DiscordNameLookup.Keys
                .Where(e => text.Contains(e))
                .OrderByDescending(e => e.Length);

            foreach (var unicodeEmoji in unicodeEmojis)
            {
                var emoji = DiscordEmoji.FromUnicode(unicodeEmoji);
                var replacement = await emojiReplacer(emoji);
                text = text.Replace(unicodeEmoji, replacement);
            }

            return text;
        }

        internal static bool HasMessageIntents(DiscordIntents intents)
            => intents.HasIntent(DiscordIntents.GuildMessages) || intents.HasIntent(DiscordIntents.DirectMessages);

        internal static bool HasReactionIntents(DiscordIntents intents)
            => intents.HasIntent(DiscordIntents.GuildMessageReactions) || intents.HasIntent(DiscordIntents.DirectMessageReactions);

        internal static bool HasTypingIntents(DiscordIntents intents)
            => intents.HasIntent(DiscordIntents.GuildMessageTyping) || intents.HasIntent(DiscordIntents.DirectMessageTyping);

        // https://discord.com/developers/docs/topics/gateway#sharding-sharding-formula
        /// <summary>
        /// Gets a shard id from a guild id and total shard count.
        /// </summary>
        /// <param name="guildId">The guild id the shard is on.</param>
        /// <param name="shardCount">The total amount of shards.</param>
        /// <returns>The shard id.</returns>
        public static int GetShardId(ulong guildId, int shardCount)
            => (int)(guildId >> 22) % shardCount;

        /// <summary>
        /// Helper method to create a <see cref="DateTimeOffset"/> from Unix time seconds for targets that do not support this natively.
        /// </summary>
        /// <param name="unixTime">Unix time seconds to convert.</param>
        /// <param name="shouldThrow">Whether the method should throw on failure. Defaults to true.</param>
        /// <returns>Calculated <see cref="DateTimeOffset"/>.</returns>
        public static DateTimeOffset GetDateTimeOffset(long unixTime, bool shouldThrow = true)
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixTime);
            }
            catch (Exception)
            {
                if (shouldThrow)
                    throw;

                return DateTimeOffset.MinValue;
            }
        }

        /// <summary>
        /// Helper method to create a <see cref="DateTimeOffset"/> from Unix time milliseconds for targets that do not support this natively.
        /// </summary>
        /// <param name="unixTime">Unix time milliseconds to convert.</param>
        /// <param name="shouldThrow">Whether the method should throw on failure. Defaults to true.</param>
        /// <returns>Calculated <see cref="DateTimeOffset"/>.</returns>
        public static DateTimeOffset GetDateTimeOffsetFromMilliseconds(long unixTime, bool shouldThrow = true)
        {
            try
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(unixTime);
            }
            catch (Exception)
            {
                if (shouldThrow)
                    throw;

                return DateTimeOffset.MinValue;
            }
        }

        /// <summary>
        /// Helper method to calculate Unix time seconds from a <see cref="DateTimeOffset"/> for targets that do not support this natively.
        /// </summary>
        /// <param name="dto"><see cref="DateTimeOffset"/> to calculate Unix time for.</param>
        /// <returns>Calculated Unix time.</returns>
        public static long GetUnixTime(DateTimeOffset dto)
            => dto.ToUnixTimeMilliseconds();

        /// <summary>
        /// Computes a timestamp from a given snowflake.
        /// </summary>
        /// <param name="snowflake">Snowflake to compute a timestamp from.</param>
        /// <returns>Computed timestamp.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTimeOffset GetSnowflakeTime(this ulong snowflake)
            => DiscordClient.DiscordEpoch.AddMilliseconds(snowflake >> 22);

        /// <summary>
        /// Converts this <see cref="Permissions"/> into human-readable format.
        /// </summary>
        /// <param name="perm">Permissions enumeration to convert.</param>
        /// <returns>Human-readable permissions.</returns>
        public static string ToPermissionString(this Permissions perm)
        {
            if (perm == Permissions.None)
                return PermissionStrings[perm];

            perm &= PermissionMethods.FULL_PERMS;

            var strs = PermissionStrings
                .Where(xkvp => xkvp.Key != Permissions.None && (perm & xkvp.Key) == xkvp.Key)
                .Select(xkvp => xkvp.Value);

            return string.Join(", ", strs.OrderBy(xs => xs));
        }

        /// <summary>
        /// Checks whether this string contains given characters.
        /// </summary>
        /// <param name="str">String to check.</param>
        /// <param name="characters">Characters to check for.</param>
        /// <returns>Whether the string contained these characters.</returns>
        public static bool Contains(this string str, params char[] characters)
        {
            foreach (var xc in str)
                if (characters.Contains(xc))
                    return true;

            return false;
        }

        /// <summary>
        /// An async version of Regex.Replace
        /// </summary>
        /// <param name="regex">The Regex object which is used to match the string for replacements</param>
        /// <param name="input">The string to search for a match</param>
        /// <param name="replacementFn">A custom method that examines each match and returns either the original matched string or a replacement string</param>
        /// <returns>A new string that is identical to the input string, except that a replacement
        /// string takes the place of each matched string. If pattern is not matched in the
        /// current instance, the method returns the current instance unchanged</returns>
        public static async Task<string> ReplaceAsync(this Regex regex, string input, Func<Match, Task<string>> replacementFn)
        {
            var sb = new StringBuilder();
            var lastIndex = 0;

            var matches = regex.Matches(input);
            foreach (Match match in matches)
            {
                sb.Append(input, lastIndex, match.Index - lastIndex)
                  .Append(await replacementFn(match).ConfigureAwait(false));

                lastIndex = match.Index + match.Length;
            }

            sb.Append(input, lastIndex, input.Length - lastIndex);
            return sb.ToString();
        }

        internal static void LogTaskFault(this Task task, ILogger logger, LogLevel level, EventId eventId, string message)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (logger == null)
                return;

            task.ContinueWith(t => logger.Log(level, eventId, t.Exception, message), TaskContinuationOptions.OnlyOnFaulted);
        }

        internal static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
