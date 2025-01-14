using Newtonsoft.Json;

namespace DSharpPlus.Net.InboundWebhooks;

/// <summary>
/// A wrapper type for an incoming webhook event.
/// </summary>
public sealed class DiscordWebhookEvent
{
    /// <summary>
    /// Gets the version.
    /// </summary>
    [JsonProperty("version")]
    public int Version { get; internal set; }

    /// <summary>
    /// Gets the ID of the application that triggered this event.
    /// </summary>
    [JsonProperty("application_id")]
    public ulong ApplicationID { get; internal set; }

    /// <summary>
    /// Gets the type of the event.
    /// </summary>
    [JsonProperty("type")]
    public DiscordWebhookEventType Type { get; internal set; }

    /// <summary>
    /// The event data payload.
    /// </summary>
    [JsonProperty("event")]
    public DiscordWebhookEventBody? Event { get; internal set; }
}
