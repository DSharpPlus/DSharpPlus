using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

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
    
    /// <summary>
    /// The ID of the component - not to be confused with <see cref="CustomId"/>; this is a numeric ID only used for identifying the component within an array.
    ///
    /// If this field is not set, it is generated in an auto-incrementing manner server-side.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public int Id { get; set; }

    internal DiscordComponent() { }
}
