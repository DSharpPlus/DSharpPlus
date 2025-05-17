using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public sealed class DiscordMentionableSelectComponent : BaseDiscordSelectComponent
{
    [JsonProperty("default_values", NullValueHandling = NullValueHandling.Ignore)]
    private readonly List<DiscordSelectDefaultValue> defaultValues = [];

    /// <summary>
    /// The default values for this component.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordSelectDefaultValue> DefaultValues => this.defaultValues;

    /// <summary>
    /// Adds a default role or user to this component.
    /// </summary>
    /// <param name="type">type of the default</param>
    /// <param name="id">Id of the default</param>
    public DiscordMentionableSelectComponent AddDefault(DiscordSelectDefaultValueType type, ulong id)
    {
        if (type == DiscordSelectDefaultValueType.Channel)
        {
            throw new ArgumentException("Mentionable select components do not support channel defaults");
        }

        DiscordSelectDefaultValue defaultValue = new(id, type);
        this.defaultValues.Add(defaultValue);
        return this;
    }

    /// <summary>
    /// Adds a collections of DiscordRoles or DiscordUsers to this component. All the ids must be of the same type.
    /// </summary>
    /// <param name="type">Type of the defaults</param>
    /// <param name="ids">Collection of ids</param>
    public DiscordMentionableSelectComponent AddDefaults(DiscordSelectDefaultValueType type, IEnumerable<ulong> ids)
    {
        if (type == DiscordSelectDefaultValueType.Channel)
        {
            throw new ArgumentException("Mentionable select components do not support channel defaults");
        }

        foreach (ulong id in ids)
        {
            DiscordSelectDefaultValue defaultValue = new(id, type);
            this.defaultValues.Add(defaultValue);
        }

        return this;
    }

    /// <summary>
    /// Enables this component.
    /// </summary>
    /// <returns>The current component.</returns>
    public DiscordMentionableSelectComponent Enable()
    {
        this.Disabled = false;
        return this;
    }
    /// <summary>
    /// Disables this component.
    /// </summary>
    /// <returns>The current component.</returns>
    public DiscordMentionableSelectComponent Disable()
    {
        this.Disabled = true;
        return this;
    }

    internal DiscordMentionableSelectComponent() => this.Type = DiscordComponentType.MentionableSelect;

    /// <summary>
    /// Creates a new mentionable select component.
    /// </summary>
    /// <param name="customId">The ID of this component.</param>
    /// <param name="placeholder">Placeholder text that's shown when no options are selected.</param>
    /// <param name="disabled">Whether this component is disabled.</param>
    /// <param name="minOptions">The minimum amount of options to be selected.</param>
    /// <param name="maxOptions">The maximum amount of options to be selected, up to 25.</param>
    public DiscordMentionableSelectComponent(string customId, string placeholder, bool disabled = false, int minOptions = 1, int maxOptions = 1)
    : base(DiscordComponentType.MentionableSelect, customId, placeholder, disabled, minOptions, maxOptions) { }
}
