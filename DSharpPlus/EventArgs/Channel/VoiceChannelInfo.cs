using System;

using DSharpPlus.Entities;
using DSharpPlus.Net.Serialization;

using Newtonsoft.Json;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents information about a voice channel.
/// </summary>
public sealed class VoiceChannelInfo
{
    [JsonProperty("id")]
    internal ulong Id { get; set; }

    /// <summary>
    /// The channel this info relates to.
    /// </summary>
    [JsonIgnore]
    public DiscordChannel Channel { get; internal set; }

    /// <summary>
    /// The voice channel status of this channel.
    /// </summary>
    [JsonProperty("status")]
    public string? Status { get; internal set; }

    /// <summary>
    /// The time whereat this voice session started.
    /// </summary>
    [JsonProperty("voice_start_time")]
    [JsonConverter(typeof(UnixTimeSecondsAsDateTimeOffsetJsonConverter))]
    public DateTimeOffset? StartTime { get; internal set; }
}
