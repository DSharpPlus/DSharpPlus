using System;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// A Stage Instance holds information about a live stage.
/// </summary>
public sealed record InternalStageInstance
{
    /// <summary>
    /// The id of this Stage instance.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; init; } = null!;

    /// <summary>
    /// The guild id of the associated Stage channel.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The id of the associated Stage channel.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake ChannelId { get; init; } = null!;

    /// <summary>
    /// The topic of the Stage instance (1-120 characters).
    /// </summary>
    [JsonPropertyName("topic")]
    public string Topic { get; init; } = null!;

    /// <summary>
    /// The privacy level of the Stage instance.
    /// </summary>
    [JsonPropertyName("privacy")]
    public DiscordStageInstancePrivacyLevel PrivacyLevel { get; init; }

    /// <summary>
    /// Whether or not Stage Discovery is disabled (deprecated).
    /// </summary>
    [JsonPropertyName("discovery_disabled")]
    [Obsolete("Whether or not Stage Discovery is disabled (deprecated)")]
    public bool DiscoverableDisabled { get; set; }

    /// <summary>
    /// The id of the scheduled event for this Stage instance.
    /// </summary>
    [JsonPropertyName("guild_scheduled_event_id")]
    public Snowflake? GuildScheduledEventId { get; init; }
}
