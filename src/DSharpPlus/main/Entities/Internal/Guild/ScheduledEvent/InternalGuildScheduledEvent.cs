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
    public required Snowflake Id { get; init; } 

    /// <summary>
    /// The guild id which the scheduled event belongs to.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public required Snowflake GuildId { get; init; } 

    /// <summary>
    /// The channel id in which the scheduled event will be hosted, or null if 
    /// <see cref="EntityType"/> is <see cref="DiscordGuildScheduledEventEntityType.External"/>.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake? ChannelId { get; init; }

    /// <summary>
    /// The id of the user that created the scheduled event.
    /// </summary>
    /// <remarks>
    /// Null if the event was created before October 25th, 2021.
    /// </remarks>
    [JsonPropertyName("creator_id")]
    public Optional<Snowflake?> CreatorId { get; init; }

    /// <summary>
    /// The name of the scheduled event (1-100 characters).
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The description of the scheduled event (1-1000 characters).
    /// </summary>
    [JsonPropertyName("description")]
    public Optional<string?> Description { get; init; }

    /// <summary>
    /// The time the scheduled event will start.
    /// </summary>
    [JsonPropertyName("scheduled_start_time")]
    public required DateTimeOffset ScheduledStartTime { get; init; }

    /// <summary>
    /// The time the scheduled event will end, required if <see cref="EntityType"/> is 
    /// <see cref="DiscordGuildScheduledEventEntityType.External"/>.
    /// </summary>
    [JsonPropertyName("scheduled_end_time")]
    public DateTimeOffset? ScheduledEndTime { get; init; }

    /// <summary>
    /// The privacy level of the scheduled event.
    /// </summary>
    [JsonPropertyName("privacy_level")]
    public required DiscordGuildScheduledEventPrivacyLevel PrivacyLevel { get; init; }

    /// <summary>
    /// The status of the scheduled event.
    /// </summary>
    [JsonPropertyName("status")]
    public required DiscordGuildScheduledEventStatus Status { get; init; }

    /// <summary>
    /// The type of the scheduled event
    /// </summary>
    [JsonPropertyName("entity_type")]
    public required DiscordGuildScheduledEventEntityType EntityType { get; init; }

    /// <summary>
    /// The id of an entity associated with a guild scheduled event.
    /// </summary>
    [JsonPropertyName("entity_id")]
    public Snowflake? EntityId { get; init; }

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
    public static implicit operator Snowflake(InternalGuildScheduledEvent guildScheduledEvent) => guildScheduledEvent.Id;
}
