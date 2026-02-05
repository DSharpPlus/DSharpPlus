using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a group of up to ten multi-select checkboxes. Available in modals.
/// </summary>
public class DiscordCheckboxGroupComponent : DiscordComponent
{
    /// <summary>
    /// The checkboxes within this group that can be selected from.
    /// </summary>
    [JsonProperty("options")]
    public IReadOnlyList<DiscordCheckboxGroupOption> Options { get; internal set; }

    /// <summary>
    /// The minimum amount of checkboxes that must be checked within this group.
    /// </summary>
    [JsonProperty("min_values", NullValueHandling = NullValueHandling.Ignore)]
    public int? MinValues { get; internal set; }

    /// <summary>
    /// The maximum amount of checkboxes that may be checked within this group.
    /// </summary>
    [JsonProperty("max_values", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxValues { get; internal set; }

    /// <summary>
    /// Indicates whether checking at least one box is required to submit the modal.
    /// </summary>
    [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsRequired { get; internal set; }

    // for the modal submit event
    [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
    internal IReadOnlyList<string>? Values { get; private set; }

    /// <summary>
    /// Creates a new checkbox group.
    /// </summary>
    /// <param name="customId">The custom ID of this component.</param>
    /// <param name="options">The checkboxes to include within this group, up to 10.</param>
    /// <param name="minValues">The minimum amount of checkboxes that must be checked within this group, 0-10.</param>
    /// <param name="maxValues">The maximum amount of checkboxes that may be checked within this group, 1-10.</param>
    /// <param name="required">Indicates whether responding to this component is required.</param>
    public DiscordCheckboxGroupComponent
    (
        string customId, 
        IReadOnlyList<DiscordCheckboxGroupOption> options,
        int? minValues = null,
        int? maxValues = null,
        bool? required = null
    )
    : this()
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(options.Count, 10, "options.Count");

        ArgumentOutOfRangeException.ThrowIfGreaterThan(minValues ?? 0, 10, nameof(minValues));
        ArgumentOutOfRangeException.ThrowIfLessThan(minValues ?? 0, 0, nameof(minValues));

        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxValues ?? 1, 10, nameof(maxValues));
        ArgumentOutOfRangeException.ThrowIfLessThan(maxValues ?? 1, 1, nameof(maxValues));

        this.CustomId = customId;
        this.Options = options;
        this.MinValues = minValues;
        this.MaxValues = maxValues;
        this.IsRequired = required;
    }

    internal DiscordCheckboxGroupComponent()
        => this.Type = DiscordComponentType.CheckboxGroup;
}
