namespace DSharpPlus.Entities;

using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;

/// <summary>
/// A component to attach to a message.
/// </summary>
[JsonConverter(typeof(DiscordComponentJsonConverter))]
public class DiscordComponent
{
    /// <summary>
    /// The type of component this represents.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordComponentType Type { get; internal set; }

    /// <summary>
    /// The Id of this component, if applicable. Not applicable on ActionRow(s) and link buttons.
    /// </summary>
    [JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
    public string CustomId { get; internal set; }

    internal DiscordComponent() { }
}
