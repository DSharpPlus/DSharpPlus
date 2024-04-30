namespace DSharpPlus.Entities;

using System;
using Newtonsoft.Json;

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
    /// The minimum amount of options that can be selected. Must be less than or equal to <see cref="MaximumSelectedValues"/>. Defaults to one.
    /// </summary>
    [JsonProperty("min_values", NullValueHandling = NullValueHandling.Ignore)]
    public int? MinimumSelectedValues { get; internal set; }

    /// <summary>
    /// The maximum amount of options that can be selected. Must be greater than or equal to zero or <see cref="MinimumSelectedValues"/>. Defaults to one.
    /// </summary>
    [JsonProperty("max_values", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaximumSelectedValues { get; internal set; }

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
        int maxOptions = 1
    )
    {
        Type = type;
        CustomId = customId;
        Placeholder = placeholder;
        Disabled = disabled;
        MinimumSelectedValues = minOptions;
        MaximumSelectedValues = maxOptions;

        if (MinimumSelectedValues < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minOptions), "Minimum selected values must be greater than or equal to zero.");
        }

        if (MaximumSelectedValues < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxOptions), "Maximum selected values must be greater than or equal to one.");
        }

        if (MinimumSelectedValues > MaximumSelectedValues)
        {
            throw new ArgumentOutOfRangeException(nameof(minOptions), "Minimum selected values must be less than or equal to maximum selected values.");
        }
    }
}
