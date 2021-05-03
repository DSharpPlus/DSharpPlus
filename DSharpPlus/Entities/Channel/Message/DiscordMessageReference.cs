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

using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents data from the original message.
    /// </summary>
    public class DiscordMessageReference
    {
        /// <summary>
        /// Gets the original message.
        /// </summary>
        public DiscordMessage Message { get; internal set; }

        /// <summary>
        /// Gets the channel of the original message.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the guild of the original message.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public override string ToString()
            => $"Guild: {this.Guild.Id}, Channel: {this.Channel.Id}, Message: {this.Message.Id}";

        internal DiscordMessageReference() { }
    }

    internal struct InternalDiscordMessageReference
    {
        [JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong? MessageId { get; set; }

        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong? ChannelId { get; set; }

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong? GuildId { get; set; }

        [JsonProperty("fail_if_not_exists", NullValueHandling = NullValueHandling.Ignore)]
        public bool FailIfNotExists { get; set; }
    }
}
