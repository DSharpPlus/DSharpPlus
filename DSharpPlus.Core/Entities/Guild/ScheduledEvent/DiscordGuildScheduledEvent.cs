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
using System.Text.Json.Serialization;
using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// A representation of a scheduled event in a guild.
    /// </summary>
    public sealed record DiscordGuildScheduledEvent
    {
        /// <summary>
        /// The id of the scheduled event.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The guild id which the scheduled event belongs to.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The channel id in which the scheduled event will be hosted, or null if <see cref="EntityType"/> is <see cref="DiscordGuildScheduledEventEntityType.External"/>.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public DiscordSnowflake? ChannelId { get; internal set; }

        /// <summary>
        /// The id of the user that created the scheduled event.
        /// </summary>
        /// <remarks>
        /// Null if the event was created before October 25th, 2021.
        /// </remarks>
        [JsonPropertyName("creator_id")]
        public Optional<DiscordSnowflake?> CreatorId { get; init; }

        /// <summary>
        /// The name of the scheduled event (1-100 characters).
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; internal set; } = null!;

        /// <summary>
        /// The description of the scheduled event (1-1000 characters).
        /// </summary>
        [JsonPropertyName("description")]
        public Optional<string?> Description { get; internal set; }

        /// <summary>
        /// The time the scheduled event will start.
        /// </summary>
        [JsonPropertyName("scheduled_start_time")]
        public DateTimeOffset ScheduledStartTime { get; internal set; }

        /// <summary>
        /// The time the scheduled event will end, required if <see cref="EntityType"/> is <see cref="DiscordGuildScheduledEventEntityType.External"/>.
        /// </summary>
        [JsonPropertyName("scheduled_start_time")]
        public DateTimeOffset? ScheduledEndTime { get; internal set; }

        /// <summary>
        /// The privacy level of the scheduled event.
        /// </summary>
        [JsonPropertyName("scheduled_end_time")]
        public DiscordGuildScheduledEventPrivacyLevel PrivacyLevel { get; internal set; }

        /// <summary>
        /// The status of the scheduled event.
        /// </summary>
        [JsonPropertyName("privacy_level")]
        public DiscordGuildScheduledEventStatus Status { get; internal set; }

        /// <summary>
        /// The type of the scheduled event
        /// </summary>
        [JsonPropertyName("entity_type")]
        public DiscordGuildScheduledEventEntityType EntityType { get; internal set; }

        /// <summary>
        /// The id of an entity associated with a guild scheduled event.
        /// </summary>
        [JsonPropertyName("entity_id")]
        public DiscordSnowflake? EntityId { get; internal set; }

        /// <summary>
        /// Additional metadata for the guild scheduled event.
        /// </summary>
        [JsonPropertyName("entity_metadata")]
        public DiscordGuildScheduledEventEntityMetadata? EntityMetadata { get; internal set; }

        /// <summary>
        /// The user that created the scheduled event
        /// </summary>
        /// <remarks>
        /// Not included if the event was created before October 25th, 2021.
        /// </remarks>
        [JsonPropertyName("creator")]
        public Optional<DiscordUser> Creator { get; internal set; }

        /// <summary>
        /// The number of users subscribed to the scheduled event
        /// </summary>
        [JsonPropertyName("user_count")]
        public Optional<int> UserCount { get; internal set; }

        /// <summary>
        /// The cover image hash of the scheduled event
        /// </summary>
        [JsonPropertyName("image")]
        public Optional<string?> Image { get; internal set; }

        public static implicit operator ulong(DiscordGuildScheduledEvent guildScheduledEvent) => guildScheduledEvent.Id;
        public static implicit operator DiscordSnowflake(DiscordGuildScheduledEvent guildScheduledEvent) => guildScheduledEvent.Id;
    }
}
