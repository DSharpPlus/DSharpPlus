using System;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// A representation of a scheduled event in a guild.
/// </summary>
public sealed record InternalGuildScheduledEvent
{
    /// <summary>
    /// The id of the scheduled event.
    /// </summary>
    [JsonPropertyName("id")]
    public InternalSnowflake Id { get; init; } = null!;

    /// <summary>
    /// The guild id which the scheduled event belongs to.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public InternalSnowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The channel id in which the scheduled event will be hosted, or null if <see cref="EntityType"/> is <see cref="InternalGuildScheduledEventEntityType.External"/>.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public InternalSnowflake? ChannelId { get; init; }

    /// <summary>
    /// The id of the user that created the scheduled event.
    /// </summary>
    /// <remarks>
    /// Null if the event was created before October 25th, 2021.
    /// </remarks>
    [JsonPropertyName("creator_id")]
    public Optional<InternalSnowflake?> CreatorId { get; init; }

    /// <summary>
    /// The name of the scheduled event (1-100 characters).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// The description of the scheduled event (1-1000 characters).
    /// </summary>
    [JsonPropertyName("description")]
    public Optional<string?> Description { get; init; }

    /// <summary>
    /// The time the scheduled event will start.
    /// </summary>
    [JsonPropertyName("scheduled_start_time")]
    public DateTimeOffset ScheduledStartTime { get; init; }

    /// <summary>
    /// The time the scheduled event will end, required if <see cref="EntityType"/> is <see cref="InternalGuildScheduledEventEntityType.External"/>.
    /// </summary>
    [JsonPropertyName("scheduled_end_time")]
    public DateTimeOffset? ScheduledEndTime { get; init; }

    /// <summary>
    /// The privacy level of the scheduled event.
    /// </summary>
    [JsonPropertyName("privacy_level")]
    public DiscordGuildScheduledEventPrivacyLevel PrivacyLevel { get; init; }

    /// <summary>
    /// The status of the scheduled event.
    /// </summary>
    [JsonPropertyName("status")]
    public DiscordGuildScheduledEventStatus Status { get; init; }

    /// <summary>
    /// The type of the scheduled event
    /// </summary>
    [JsonPropertyName("entity_type")]
    public DiscordGuildScheduledEventEntityType EntityType { get; init; }

    /// <summary>
    /// The id of an entity associated with a guild scheduled event.
    /// </summary>
    [JsonPropertyName("entity_id")]
    public InternalSnowflake? EntityId { get; init; }

    /// <summary>
    /// Additional metadata for the guild scheduled event.
    /// </summary>
    [JsonPropertyName("entity_metadata")]
    public InternalGuildScheduledEventEntityMetadata? EntityMetadata { get; init; }

    /// <summary>
    /// The user that created the scheduled event
    /// </summary>
    /// <remarks>
    /// Not included if the event was created before October 25th, 2021.
    /// </remarks>
    [JsonPropertyName("creator")]
    public Optional<InternalUser> Creator { get; init; }

    /// <summary>
    /// The number of users subscribed to the scheduled event
    /// </summary>
    [JsonPropertyName("user_count")]
    public Optional<int> UserCount { get; init; }

    /// <summary>
    /// The cover image hash of the scheduled event
    /// </summary>
    [JsonPropertyName("image")]
    public Optional<string?> Image { get; init; }

    public static implicit operator ulong(InternalGuildScheduledEvent guildScheduledEvent) => guildScheduledEvent.Id;
    public static implicit operator InternalSnowflake(InternalGuildScheduledEvent guildScheduledEvent) => guildScheduledEvent.Id;
}
