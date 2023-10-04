using Newtonsoft.Json;
namespace DSharpPlus.Entities;

/// <summary>
/// A text-input field. Like selects, this can only be used once per action row.
/// </summary>
public sealed class TextInputComponent : DiscordComponent
{
    /// <summary>
    /// Optional placeholder text for this input.
    /// </summary>
    [JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
    public string Placeholder { get; set; }

    /// <summary>
    /// Label text to put above this input.
    /// </summary>
    [JsonProperty("label")]
    public string Label { get; set; }

    /// <summary>
    /// Pre-filled value for this input.
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; set; }

    /// <summary>
    /// Optional minimum length for this input.
    /// </summary>
    [JsonProperty("min_length", NullValueHandling = NullValueHandling.Ignore)]
    public int MinimumLength { get; set; }

    /// <summary>
    /// Optional maximum length for this input. Must be a positive integer, if set.
    /// </summary>
    [JsonProperty("max_length", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaximumLength { get; set; }

    /// <summary>
    /// Whether this input is required.
    /// </summary>
    [JsonProperty("required")]
    public bool Required { get; set; }

    /// <summary>
    /// Style of this input.
    /// </summary>
    [JsonProperty("style")]
    public TextInputStyle Style { get; set; }

    public TextInputComponent() => this.Type = ComponentType.FormInput;

    /// <summary>
    /// Constructs a new text input field.
    /// </summary>
    /// <param name="label">The label for the field, placed above the input itself.</param>
    /// <param name="customId">The ID of this field.</param>
    /// <param name="placeholder">Placeholder text for the field.</param>
    /// <param name="value">A pre-filled value for this field.</param>
    /// <param name="required">Whether this field is required.</param>
    /// <param name="style">The style of this field. A single-ling short, or multi-line paragraph.</param>
    /// <param name="min_length">The minimum input length.</param>
    /// <param name="max_length">The maximum input length. Must be greater than the minimum, if set.</param>
    public TextInputComponent(string label, string customId, string placeholder = null, string value = null, bool required = true, TextInputStyle style = TextInputStyle.Short, int min_length = 0, int? max_length = null)
    {
        this.CustomId = customId;
        this.Type = ComponentType.FormInput;
        this.Label = label;
        this.Required = required;
        this.Placeholder = placeholder;
        this.MinimumLength = min_length;
        this.MaximumLength = max_length;
        this.Style = style;
        this.Value = value;
    }
}
