using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a label in a modal.
/// </summary>
public class DiscordLabelComponent : DiscordComponent
{
    [JsonProperty("type")]
    public DiscordComponentType ComponentType => DiscordComponentType.Label;

    /// <summary>
    /// Gets or sets the label. 
    /// </summary>
    /// <remarks>This value is not returned by Discord, and will be null in a modal submit event.</remarks>
    [JsonProperty("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the description of the label.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets the component contained within the label. At this time, this may only be <see cref="BaseDiscordSelectComponent"/> or <see cref="DiscordTextInputComponent"/>.
    /// </summary>
    [JsonProperty("component")]
    public DiscordComponent Component { get; internal set; }

    public DiscordLabelComponent
    (
        DiscordTextInputComponent component,
        string label = "",
        string? description = null
    )
    {
        this.Component = component;
        this.Label = label;
        this.Description = description;
    }

    public DiscordLabelComponent
    (
        BaseDiscordSelectComponent component,
        string label = "",
        string? description = null
    )
    {
        this.Component = component;
        this.Label = label;
        this.Description = description;
    }

    internal DiscordLabelComponent() { }
}
