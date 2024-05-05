using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents options for <see cref="DiscordSelectComponent"/>.
/// </summary>
public sealed class DiscordSelectComponentOption
{
    /// <summary>
    /// The label to add. This is required.
    /// </summary>
    [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
    public string Label { get; internal set; }

    /// <summary>
    /// The value of this option. Akin to the Custom Id of components.
    /// </summary>
    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    public string Value { get; internal set; }

    /// <summary>
    /// Whether this option is default. If true, this option will be pre-selected. Defaults to false.
    /// </summary>
    [JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
    public bool Default { get; internal set; } // false //

    /// <summary>
    /// The description of this option. This is optional.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; internal set; }

    /// <summary>
    /// The emoji of this option. This is optional.
    /// </summary>
    [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordComponentEmoji Emoji { get; internal set; }

    public DiscordSelectComponentOption(string label, string value, string description = null, bool isDefault = false, DiscordComponentEmoji emoji = null)
    {
        this.Label = label;
        this.Value = value;
        this.Description = description;
        this.Default = isDefault;
        this.Emoji = emoji;
    }
}
