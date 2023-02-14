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
    /// <summary>
    /// Represents a channel in a welcome screen
    /// </summary>
    public class DiscordGuildWelcomeScreenChannel
    {
        /// <summary>
        /// Gets the id of the channel.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ChannelId { get; internal set; }

        /// <summary>
        /// Gets the description shown for the channel.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the emoji id if the emoji is custom, when applicable.
        /// </summary>
        [JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? EmojiId { get; internal set; }

        /// <summary>
        /// Gets the name of the emoji if custom or the unicode character if standard, when applicable.
        /// </summary>
        [JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Ignore)]
        public string EmojiName { get; internal set; }

        public DiscordGuildWelcomeScreenChannel(ulong channelId, string description, DiscordEmoji emoji = null)
        {
            this.ChannelId = channelId;
            this.Description = description;
            if (emoji != null)
            {
                if (emoji.Id == 0)
                    this.EmojiName = emoji.Name;
                else
                    this.EmojiId = emoji.Id;
            }
        }
    }
}
