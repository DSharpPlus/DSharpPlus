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

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// See https://discord.com/developers/docs/resources/channel#message-reference-object-message-reference-structure
    /// </summary>
    public sealed record DiscordMessageReference
    {
        /// <summary>
        /// The id of the originating message.
        /// </summary>
        [JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> MessageId { get; init; }

        /// <summary>
        /// The id of the originating message's channel.
        /// </summary>
        /// <remarks>
        /// channel_id is optional when creating a reply, but will always be present when receiving an event/response that includes this data model.
        /// </remarks>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> ChannelId { get; init; }

        /// <summary>
        /// The id of the originating message's guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// When sending, whether to error if the referenced message doesn't exist instead of sending as a normal (non-reply) message, default true.
        /// </summary>
        [JsonProperty("fail_if_not_exists", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> FailIfNotExists { get; init; }
    }
}
