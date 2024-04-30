namespace DSharpPlus.Entities;

using Newtonsoft.Json;

/// <summary>
/// Represents a Rich Presence activity.
/// </summary>
public class DiscordMessageActivity
{
    /// <summary>
    /// Gets the activity type.
    /// </summary>
    [JsonProperty("type")]
    public DiscordMessageActivityType Type { get; internal set; }

    /// <summary>
    /// Gets the party id of the activity.
    /// </summary>
    [JsonProperty("party_id")]
    public string? PartyId { get; internal set; }

    internal DiscordMessageActivity() { }
}
