using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents the message interaction data sent when a message is an interaction response.
/// </summary>
public class DiscordMessageInteraction : SnowflakeObject
{
    /// <summary>
    /// Gets the type of the interaction.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public InteractionType Type { get; internal set; }

    /// <summary>
    /// Gets the name of the <see cref="DiscordApplicationCommand"/>.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the user who invoked the interaction.
    /// </summary>
    [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUser User { get; internal set; }
}
