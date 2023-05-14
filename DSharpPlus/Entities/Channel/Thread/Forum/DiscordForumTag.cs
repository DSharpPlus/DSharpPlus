// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    public sealed class DiscordForumTag : SnowflakeObject
    {
        /// <summary>
        /// Gets the name of this tag.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets whether this tag is moderated. Moderated tags can only be applied by users with the <see cref="Permissions.ManageThreads"/> permission.
        /// </summary>
        [JsonProperty("moderated")]
        public bool Moderated { get; internal set; }

        /// <summary>
        /// Gets the Id of the emoji for this tag, if applicable.
        /// </summary>
        [JsonProperty("emoji_id")]
        public ulong? EmojiId { get; internal set; }

        /// <summary>
        /// Gets the unicode emoji for this tag, if applicable.
        /// </summary>
        [JsonProperty("emoji_name")]
        public string EmojiName { get; internal set; }
    }

    public class DiscordForumTagBuilder
    {
        [JsonProperty("name")]
        private string _name;

        [JsonProperty("moderated")]
        private bool _moderated;

        [JsonProperty("emoji_id")]
        private ulong? _emojiId;

        [JsonProperty("emoji_name")]
        private string _emojiName;


        public static DiscordForumTagBuilder FromTag(DiscordForumTag tag)
        {
            var builder = new DiscordForumTagBuilder();
            builder._name = tag.Name;
            builder._moderated = tag.Moderated;
            builder._emojiId = tag.EmojiId;
            builder._emojiName = tag.EmojiName;
            return builder;
        }

        /// <summary>
        /// Sets the name of this tag.
        /// </summary>
        /// <param name="name">The name of the tag.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordForumTagBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Sets this tag to be moderated (as in, it can only be set by users with the <see cref="Permissions.ManageThreads"/> permission).
        /// </summary>
        /// <param name="moderated">Whether the tag is moderated.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordForumTagBuilder IsModerated(bool moderated = true)
        {
            _moderated = moderated;
            return this;
        }

        /// <summary>
        /// Sets the emoji ID for this tag (which will overwrite the emoji name).
        /// </summary>
        /// <param name="emojiId"></param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordForumTagBuilder WithEmojiId(ulong? emojiId)
        {
            _emojiId = emojiId;
            _emojiName = null;
            return this;
        }

        /// <summary>
        /// Sets the emoji for this tag.
        /// </summary>
        /// <param name="emoji">The emoji to use.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordForumTagBuilder WithEmoji(DiscordEmoji emoji)
        {
            _emojiId = emoji.Id;
            _emojiName = emoji.Name;
            return this;
        }

        /// <returns>The builder to chain calls with.</returns>
        public DiscordForumTagBuilder WithEmojiName(string emojiName)
        {
            _emojiId = null;
            _emojiName = emojiName;
            return this;
        }
    }
}
