namespace DSharpPlus.Entities.Interaction.Components;

/// <summary>
/// Represents a label in a modal.
/// </summary>
public class DiscordLabelComponent : DiscordComponent
{
    public DiscordComponentType ComponentType => DiscordComponentType.Label;
    
    /// <summary>
    /// Gets or sets the label. 
    /// </summary>
    /// <remarks>This value is not returned by Discord, and will be null in a modal submit event.</remarks>
    public string? Label { get; set; }
    
    /// <summary>
    /// Gets or sets the description of the label.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets the component of the label. At this time, this may only be <see cref="DiscordSelectComponent"/> or <see cref="DiscordTextInputComponent"/>
    /// </summary>
    public DiscordComponent Component { get; }

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
