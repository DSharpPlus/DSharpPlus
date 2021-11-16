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
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// A scheduled event on a guild, which notifies all people that are interested in it.
    /// </summary>
    public sealed class DiscordScheduledGuildEvent : SnowflakeObject
    {
        /// <summary>
        /// The name of the event.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// The description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }

        /// <summary>
        /// The time at which this event will begin.
        /// </summary>
        [JsonProperty("scheduled_start_time")]
        public DateTimeOffset StartTime { get; internal set; }

        /// <summary>
        /// The time at which the event will end, or null if it doesn't have an end time.
        /// </summary>
        [JsonProperty("scheduled_end_time")]
        public DateTimeOffset? EndTime { get; internal set; }

        /// <summary>
        /// The guild this event is scheduled for.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild => (this.Discord as DiscordClient).InternalGetCachedGuild(this.GuildId);

        /// <summary>
        /// The channel this event is scheduled for, if applicable.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel => this.ChannelId.HasValue ? this.Guild.GetChannel(this.ChannelId.Value) : null;

        /// <summary>
        /// The id of the channel this event is scheduled in, if any.
        /// </summary>
        [JsonProperty("channel_id")]
        public ulong? ChannelId { get; internal set; }

        /// <summary>
        /// The id of the guild this event is scheduled for.
        /// </summary>
        [JsonProperty("guild_id")]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The user that created this event.
        /// </summary>
        [JsonProperty("creator")]
        public DiscordUser Creator { get; internal set; }

        /// <summary>
        /// The privacy of this event.
        /// </summary>
        [JsonProperty("privacy_level")]
        public ScheduledGuildEventPrivacyLevel PrivacyLevel { get; internal set; }

        /// <summary>
        /// The current status of this event.
        /// </summary>
        [JsonProperty("status")]
        public ScheduledGuildEventStatus Status { get; internal set; }

        /// <summary>
        /// Metadata associated with this event.
        /// </summary>
        [JsonProperty("entity_metadata")]
        public DiscordScheduledGuildEventMetadata Metadata { get; internal set; }

        /// <summary>
        /// What type of event this is.
        /// </summary>
        [JsonProperty("entity_type")]
        public ScheduledGuildEventType Type { get; internal set; }

        /// <summary>
        /// How many users are interested in this event.
        /// </summary>
        [JsonProperty("user_count")]
        public int? UserCount { get; internal set; }

        internal DiscordScheduledGuildEvent() { }
    }
}
