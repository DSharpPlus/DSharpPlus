using System;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a base class for all select-menus.
/// </summary>
public abstract class BaseDiscordSelectComponent : DiscordComponent
{
    /// <summary>
    /// The text to show when no option is selected.
    /// </summary>
    [JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
    public string Placeholder { get; internal set; }

    /// <summary>
    /// Whether this dropdown can be interacted with.
    /// </summary>
    [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool Disabled { get; internal set; }

    /// <summary>
    /// Whether this component is required. Only affects usage in modals. Defaults to <c>true</c>.
    /// </summary>
    [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
    public bool Required { get; internal set; } = true; // Align with Discord's default

    /// <summary>
    /// The minimum amount of options that can be selected. Must be less than or equal to <see cref="MaximumSelectedValues"/>. Defaults to one.
    /// </summary>
    [JsonProperty("min_values", NullValueHandling = NullValueHandling.Ignore)]
    public int? MinimumSelectedValues { get; internal set; }

    /// <summary>
    /// The maximum amount of options that can be selected. Must be greater than or equal to zero or <see cref="MinimumSelectedValues"/>. Defaults to one.
    /// </summary>
    [JsonProperty("max_values", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaximumSelectedValues { get; internal set; }

    /// <summary>
    /// Internally used for parsing responses to modals, since those send submitted values in the component response object.
    /// </summary>
    [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
    internal string[]? SubmittedValues { get; set; }

    // used by Newtonsoft.Json
    public BaseDiscordSelectComponent()
    {
    }

    internal BaseDiscordSelectComponent
    (
        DiscordComponentType type,
        string customId,
        string placeholder,
        bool disabled = false,
        int minOptions = 1,
        int maxOptions = 1,
        bool required = true
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(minOptions, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxOptions, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minOptions, maxOptions);

        if (this.Required)
        {
            ArgumentOutOfRangeException.ThrowIfEqual(minOptions, 0);
        }
        
        this.Type = type;
        this.CustomId = customId;
        this.Placeholder = placeholder;
        this.Disabled = disabled;
        this.MinimumSelectedValues = minOptions;
        this.MaximumSelectedValues = maxOptions;
        this.Required = required;
    }
}
