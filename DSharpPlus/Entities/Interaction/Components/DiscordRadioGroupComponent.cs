using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a group of two to ten options to choose from. Available in modals.
/// </summary>
public class DiscordRadioGroupComponent : DiscordComponent
{
    /// <summary>
    /// The options within this group that can be selected from.
    /// </summary>
    [JsonProperty("options")]
    public IReadOnlyList<DiscordRadioGroupOption> Options { get; internal set; }

    /// <summary>
    /// Indicates whether checking at least one option is required to submit the modal.
    /// </summary>
    [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsRequired { get; internal set; }

    // for the modal submit event
    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    internal string? Value { get; private set; }

    /// <summary>
    /// Creates a new radio group.
    /// </summary>
    /// <param name="customId">The custom ID of this component.</param>
    /// <param name="options">The options to include within this group, 2-10.</param>
    /// <param name="required">Indicates whether responding to this component is required.</param>
    public DiscordRadioGroupComponent
    (
        string customId,
        IReadOnlyList<DiscordRadioGroupOption> options,

        bool? required = null
    )
    : this()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(options.Count, 2, "options.Count");
        ArgumentOutOfRangeException.ThrowIfGreaterThan(options.Count, 10, "options.Count");

        this.CustomId = customId;
        this.Options = options;
        this.IsRequired = required;
    }

    internal DiscordRadioGroupComponent()
        => this.Type = DiscordComponentType.RadioGroup;
}
