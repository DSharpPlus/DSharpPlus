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
    /// The minimum amount of options that can be selected. Must be less than or equal to <see cref="MaximumSelectedValues"/>. Defaults to one.
    /// </summary>
    [JsonProperty("min_values", NullValueHandling = NullValueHandling.Ignore)]
    public int? MinimumSelectedValues { get; internal set; }

    /// <summary>
    /// The maximum amount of options that can be selected. Must be greater than or equal to zero or <see cref="MinimumSelectedValues"/>. Defaults to one.
    /// </summary>
    [JsonProperty("max_values", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaximumSelectedValues { get; internal set; }
}
