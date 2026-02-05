using System;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a single checkbox within a <see cref="DiscordCheckboxGroupComponent"/>.
/// </summary>
public class DiscordCheckboxGroupOption
{
    /// <summary>
    /// The developer-defined value that will be returned to the bot when the modal is submitted.
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; internal set; }

    /// <summary>
    /// The user-facing label of the checkbox. Maximum 100 characters.
    /// </summary>
    [JsonProperty("label")]
    public string Label { get; internal set; }

    /// <summary>
    /// An optional user-facing description of this checkbox. Maximum 100 characters.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; internal set; }

    /// <summary>
    /// Indicates whether this option is selected by default.
    /// </summary>
    [JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
    public bool? SelectedByDefault { get; internal set; }

    /// <summary>
    /// Creates a new checkbox for a checkbox group.
    /// </summary>
    /// <param name="value">The developer-defined value that will be returned to the bot when the modal is submitted.</param>
    /// <param name="label">The user-facing label of the checkbox, max 100 characters.</param>
    /// <param name="description">The user-facing description of the checkbox, max 100 characters.</param>
    /// <param name="selectedByDefault">Indicates whether this checkbox is selected by default.</param>
    public DiscordCheckboxGroupOption(string value, string label, string? description = null, bool? selectedByDefault = null)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, 100, "value.Length");
        ArgumentOutOfRangeException.ThrowIfGreaterThan(label.Length, 100, "label.Length");
        ArgumentOutOfRangeException.ThrowIfGreaterThan(description?.Length ?? 0, 100, "description.Length");

        this.Value = value;
        this.Label = label;
        this.Description = description;
        this.SelectedByDefault = selectedByDefault;
    }
}
