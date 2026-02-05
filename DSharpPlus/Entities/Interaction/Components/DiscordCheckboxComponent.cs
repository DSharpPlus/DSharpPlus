using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a single checkbox for a binary choice. Only available in modals.
/// </summary>
public class DiscordCheckboxComponent : DiscordComponent
{
    /// <summary>
    /// Indicates whether this checkbox is selected by default. Defaults to false.
    /// </summary>
    [JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
    public bool? SelectedByDefault { get; internal set; }

    // this is there for the modal submitted event because our entity modeling is too abysmal to understand that submitted components
    // are, actually, not the same type as sent components
    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    internal bool? Value { get; private set; }

    /// <summary>
    /// Creates a new checkbox component.
    /// </summary>
    /// <param name="customId">The custom ID for this component.</param>
    /// <param name="selectedByDefault">Indicates whether this component is selected by default.</param>
    public DiscordCheckboxComponent(string customId, bool? selectedByDefault = null) : this()
    {
        this.CustomId = customId;
        this.SelectedByDefault = selectedByDefault;
    }

    internal DiscordCheckboxComponent()
        => this.Type = DiscordComponentType.Checkbox;
}
