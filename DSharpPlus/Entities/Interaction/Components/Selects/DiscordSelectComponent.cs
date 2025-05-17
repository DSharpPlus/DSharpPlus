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

    public DiscordSelectComponent(string customId, string placeholder, IEnumerable<DiscordSelectComponentOption> options, bool disabled = false, int minOptions = 1, int maxOptions = 1)
        : base(DiscordComponentType.StringSelect, customId, placeholder, disabled, minOptions, maxOptions)
        => this.Options = options.ToArray();
}
