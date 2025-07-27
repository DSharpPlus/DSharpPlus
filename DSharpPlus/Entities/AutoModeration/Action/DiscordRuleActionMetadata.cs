using System;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord rule action metadata.
/// </summary>
public class DiscordRuleActionMetadata
{
    /// <summary>
    /// Gets the ID of the channel where blocked content or moderation events should be logged.
    /// </summary>
    [JsonProperty("channel_id")]
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// Gets the timeout duration in seconds.
    /// </summary>
    [JsonIgnore]
    public TimeSpan TimeoutSeconds => TimeSpan.FromSeconds(this.DurationSeconds);

    /// <summary>
    /// Gets the custom message that will be shown to a user when their content is blocked.
    /// </summary>
    [JsonProperty("custom_message", NullValueHandling = NullValueHandling.Ignore)]
    public string? CustomMessage { get; internal set; }

    [JsonProperty("duration_seconds")]
    internal uint DurationSeconds { get; set; }
}
