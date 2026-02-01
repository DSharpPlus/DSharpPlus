using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// A select menu with multiple options to choose from.
/// </summary>
public sealed class DiscordSelectComponent : BaseDiscordSelectComponent
{
    /// <summary>
    /// The options to pick from on this component.
    /// </summary>
    [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordSelectComponentOption> Options { get; internal set; } = Array.Empty<DiscordSelectComponentOption>();

    /// <summary>
    /// Enables this component.
    /// </summary>
    /// <returns>The current component.</returns>
    public DiscordSelectComponent Enable()
    {
        this.Disabled = false;
        return this;
    }

    /// <summary>
    /// Disables this component.
    /// </summary>
    /// <returns>The current component.</returns>
    public DiscordSelectComponent Disable()
    {
        this.Disabled = true;
        return this;
    }

    internal DiscordSelectComponent() => this.Type = DiscordComponentType.StringSelect;

    /// <summary>
    /// Creates a new string select component.
    /// </summary>
    /// <param name="customId">The ID of this component.</param>
    /// <param name="placeholder">Placeholder text that's shown when no options are selected.</param>
    /// <param name="options">The selectable options for this component.</param>
    /// <param name="disabled">Whether this component is disabled.</param>
    /// <param name="minOptions">The minimum amount of options to be selected.</param>
    /// <param name="maxOptions">The maximum amount of options to be selected, up to 25.</param>
    /// <param name="required">Indicates whether this component, in a modal, requires user input.</param>
    public DiscordSelectComponent
    (
        string customId, 
        string placeholder, 
        IEnumerable<DiscordSelectComponentOption> options, 
        bool disabled = false, 
        int minOptions = 1, 
        int maxOptions = 1,
        bool required = false
    )
        : base(DiscordComponentType.StringSelect, customId, placeholder, disabled, minOptions, maxOptions, required)
        => this.Options = options.ToArray();
}
