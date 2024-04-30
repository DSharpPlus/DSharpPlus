namespace DSharpPlus.Entities;
using Newtonsoft.Json;

/// <summary>
/// Represents a field inside a discord embed.
/// </summary>
public sealed class DiscordEmbedField
{
    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets the value of the field.
    /// </summary>
    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    public string? Value { get; set; }

    /// <summary>
    /// Gets whether or not this field should display inline.
    /// </summary>
    [JsonProperty("inline", NullValueHandling = NullValueHandling.Ignore)]
    public bool Inline { get; set; }

    internal DiscordEmbedField() { }
}
