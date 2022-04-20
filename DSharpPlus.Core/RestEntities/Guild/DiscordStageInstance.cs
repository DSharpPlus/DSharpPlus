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

using System;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// A Stage Instance holds information about a live stage.
    /// </summary>
    public sealed record DiscordStageInstance
    {
        /// <summary>
        /// The id of this Stage instance.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The guild id of the associated Stage channel.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The id of the associated Stage channel.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The topic of the Stage instance (1-120 characters).
        /// </summary>
        [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
        public string Topic { get; internal set; } = null!;

        /// <summary>
        /// The privacy level of the Stage instance.
        /// </summary>
        [JsonProperty("privacy", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordStageInstancePrivacyLevel PrivacyLevel { get; init; }

        /// <summary>
        /// Whether or not Stage Discovery is disabled (deprecated).
        /// </summary>
        [JsonProperty("discovery_disabled", NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("Whether or not Stage Discovery is disabled (deprecated)")]
        public bool DiscoverableDisabled { get; set; }

        /// <summary>
        /// The id of the scheduled event for this Stage instance.
        /// </summary>
        [JsonProperty("guild_scheduled_event_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? GuildScheduledEventId { get; init; }
    }
}
