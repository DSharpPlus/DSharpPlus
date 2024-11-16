using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.InboundWebhooks;

/// <summary>
/// Represents the outer data for a <see cref="DiscordWebhookEvent"/>.
///
/// This contains metadata about the actual event.
/// </summary>
// Hello source-code viewer! I hate this too -V
public class DiscordWebhookEventBody
{
    /// <summary>
    /// The type of the event.
    /// </summary>
    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    public DiscordWebhookEventBodyType Type { get; internal set; }

    /// <summary>
    /// The timestamp at which this event was invoked.
    /// </summary>
    [JsonProperty("timestamp")]
    public DateTimeOffset Timestamp { get; internal set; }

    /// <summary>
    /// The data of the event. The data within depends on the value of <see cref="Type"/>.
    /// </summary>
    [JsonProperty("data")]
    public JObject? Data { get; internal set; }
}
