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
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// A representation of a scheduled event in a guild.
    /// </summary>
    [DiscordGatewayEventName("GUILD_SCHEDULED_EVENT_CREATE", "GUILD_SCHEDULED_EVENT_UPDATE", "GUILD_SCHEDULED_EVENT_DELETE")]
    public sealed record DiscordGuildScheduledEvent
    {
        /// <summary>
        /// The id of the scheduled event.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The guild id which the scheduled event belongs to.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The channel id in which the scheduled event will be hosted, or null if <see cref="EntityType"/> is <see cref="DiscordGuildScheduledEventEntityType.External"/>.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? ChannelId { get; init; }

        /// <summary>
        /// The id of the user that created the scheduled event.
        /// </summary>
        /// <remarks>
        /// Null if the event was created before October 25th, 2021.
        /// </remarks>
        [JsonProperty("creator_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake?> CreatorId { get; init; }

        /// <summary>
        /// The name of the scheduled event (1-100 characters).
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The description of the scheduled event (1-1000 characters).
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> Description { get; init; }

        /// <summary>
        /// The time the scheduled event will start.
        /// </summary>
        [JsonProperty("scheduled_start_time", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset ScheduledStartTime { get; init; }

        /// <summary>
        /// The time the scheduled event will end, required if <see cref="EntityType"/> is <see cref="DiscordGuildScheduledEventEntityType.External"/>.
        /// </summary>
        [JsonProperty("scheduled_end_time", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ScheduledEndTime { get; init; }

        /// <summary>
        /// The privacy level of the scheduled event.
        /// </summary>
        [JsonProperty("privacy_level", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildScheduledEventPrivacyLevel PrivacyLevel { get; init; }

        /// <summary>
        /// The status of the scheduled event.
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildScheduledEventStatus Status { get; init; }

        /// <summary>
        /// The type of the scheduled event
        /// </summary>
        [JsonProperty("entity_type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildScheduledEventEntityType EntityType { get; init; }

        /// <summary>
        /// The id of an entity associated with a guild scheduled event.
        /// </summary>
        [JsonProperty("entity_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? EntityId { get; init; }

        /// <summary>
        /// Additional metadata for the guild scheduled event.
        /// </summary>
        [JsonProperty("entity_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildScheduledEventEntityMetadata? EntityMetadata { get; init; }

        /// <summary>
        /// The user that created the scheduled event
        /// </summary>
        /// <remarks>
        /// Not included if the event was created before October 25th, 2021.
        /// </remarks>
        [JsonProperty("creator", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordUser> Creator { get; init; }

        /// <summary>
        /// The number of users subscribed to the scheduled event
        /// </summary>
        [JsonProperty("user_count", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> UserCount { get; init; }

        /// <summary>
        /// The cover image hash of the scheduled event
        /// </summary>
        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> Image { get; init; }

        public static implicit operator ulong(DiscordGuildScheduledEvent guildScheduledEvent) => guildScheduledEvent.Id;
        public static implicit operator DiscordSnowflake(DiscordGuildScheduledEvent guildScheduledEvent) => guildScheduledEvent.Id;
    }
}
