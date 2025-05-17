using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// A select component that allows users to be selected.
/// </summary>
public sealed class DiscordUserSelectComponent : BaseDiscordSelectComponent
{
    [JsonProperty("default_values", NullValueHandling = NullValueHandling.Ignore)]
    private readonly List<DiscordSelectDefaultValue> defaultValues = [];

    /// <summary>
    /// The default values for this component.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordSelectDefaultValue> DefaultValues => this.defaultValues;

    /// <summary>
    /// Adds a default user to this component.
    /// </summary>
    /// <param name="value">User to add</param>
    public DiscordUserSelectComponent AddDefaultUser(DiscordUser value)
    {
        DiscordSelectDefaultValue defaultValue = new(value.Id, DiscordSelectDefaultValueType.User);
        this.defaultValues.Add(defaultValue);
        return this;
    }

    /// <summary>
    /// Collections of DiscordUser to add as default values.
    /// </summary>
    /// <param name="values">Collection of DiscordUser</param>
    public DiscordUserSelectComponent AddDefaultUsers(IEnumerable<DiscordUser> values)
    {
        foreach (DiscordUser value in values)
        {
            DiscordSelectDefaultValue defaultValue = new(value.Id, DiscordSelectDefaultValueType.User);
            this.defaultValues.Add(defaultValue);
        }

        return this;
    }

    /// <summary>
    /// Adds a default user to this component.
    /// </summary>
    /// <param name="value">Id of a DiscordUser</param>
    public DiscordUserSelectComponent AddDefaultUser(ulong value)
    {
        DiscordSelectDefaultValue defaultValue = new(value, DiscordSelectDefaultValueType.User);
        this.defaultValues.Add(defaultValue);
        return this;
    }

    /// <summary>
    /// Collections of user ids to add as default values.
    /// </summary>
    /// <param name="values">Collection of DiscordUser ids</param>
    public DiscordUserSelectComponent AddDefaultUsers(IEnumerable<ulong> values)
    {
        foreach (ulong value in values)
        {
            DiscordSelectDefaultValue defaultValue = new(value, DiscordSelectDefaultValueType.User);
            this.defaultValues.Add(defaultValue);
        }

        return this;
    }

    /// <summary>
    /// Enables this component.
    /// </summary>
    /// <returns>The current component.</returns>
    public DiscordUserSelectComponent Enable()
    {
        this.Disabled = false;
        return this;
    }

    /// <summary>
    /// Disables this component.
    /// </summary>
    /// <returns>The current component.</returns>
    public DiscordUserSelectComponent Disable()
    {
        this.Disabled = true;
        return this;
    }

    internal DiscordUserSelectComponent() => this.Type = DiscordComponentType.UserSelect;

    /// <summary>
    /// Creates a new user select component.
    /// </summary>
    /// <param name="customId">The ID of this component.</param>
    /// <param name="placeholder">Placeholder text that's shown when no options are selected.</param>
    /// <param name="disabled">Whether this component is disabled.</param>
    /// <param name="minOptions">The minimum amount of options to be selected.</param>
    /// <param name="maxOptions">The maximum amount of options to be selected, up to 25.</param>
    public DiscordUserSelectComponent(string customId, string placeholder, bool disabled = false, int minOptions = 1, int maxOptions = 1)
    : base(DiscordComponentType.UserSelect, customId, placeholder, disabled, minOptions, maxOptions) { }
}
