using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Rich Presence activity.
/// </summary>
public class DiscordMessageActivity
{
    /// <summary>
    /// Gets the activity type.
    /// </summary>
    [JsonProperty("type")]
    public MessageActivityType Type { get; internal set; }

    /// <summary>
    /// Gets the party id of the activity.
    /// </summary>
    [JsonProperty("party_id")]
    public string? PartyId { get; internal set; }

    internal DiscordMessageActivity() { }
}
