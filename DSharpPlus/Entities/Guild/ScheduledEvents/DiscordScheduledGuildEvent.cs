using System;
using Newtonsoft.Json;
namespace DSharpPlus.Entities;

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
    //TODO apply caching
    public DiscordGuild Guild => (this.Discord as DiscordClient).GetCachedGuild(this.GuildId);

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
