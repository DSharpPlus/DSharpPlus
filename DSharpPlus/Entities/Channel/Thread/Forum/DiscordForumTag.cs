// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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
        /// Gets whether this tag is moderated. Moderated tags can only be applied by users with the <see cref="DiscordPermissions.ManageThreads"/> permission.
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
}
