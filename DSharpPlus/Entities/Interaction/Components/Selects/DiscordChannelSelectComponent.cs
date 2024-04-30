using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public sealed class DiscordChannelSelectComponent : BaseDiscordSelectComponent
{
    [JsonProperty("channel_types", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordChannelType> ChannelTypes { get; internal set; }

    [JsonProperty("default_values", NullValueHandling = NullValueHandling.Ignore)]
    private readonly List<DiscordSelectDefaultValue> _defaultValues = new();

    /// <summary>
    /// The default values for this component.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordSelectDefaultValue> DefaultValues => _defaultValues;

    /// <summary>
    /// Adds a default channel to this component.
    /// </summary>
    /// <param name="channel">Channel to add</param>
    public DiscordChannelSelectComponent AddDefaultChannel(DiscordChannel channel)
    {
        DiscordSelectDefaultValue defaultValue = new(channel.Id, DiscordSelectDefaultValueType.Channel);
        _defaultValues.Add(defaultValue);
        return this;
    }

    /// <summary>
    /// Adds a collections of DiscordChannel as default values.
    /// </summary>
    /// <param name="channels">Collection of DiscordChannel</param>
    public DiscordChannelSelectComponent AddDefaultChannels(IEnumerable<DiscordChannel> channels)
    {
        foreach (DiscordChannel value in channels)
        {
            DiscordSelectDefaultValue defaultValue = new(value.Id, DiscordSelectDefaultValueType.Channel);
            _defaultValues.Add(defaultValue);
        }

        return this;
    }

    /// <summary>
    /// Adds a default channel to this component.
    /// </summary>
    /// <param name="id">Id of a DiscordChannel</param>
    public DiscordChannelSelectComponent AddDefaultChannel(ulong id)
    {
        DiscordSelectDefaultValue defaultValue = new(id, DiscordSelectDefaultValueType.Channel);
        _defaultValues.Add(defaultValue);
        return this;
    }

    /// <summary>
    /// Collections of channel ids to add as default values.
    /// </summary>
    /// <param name="ids">Collection of DiscordChannel ids</param>
    public DiscordChannelSelectComponent AddDefaultChannels(IEnumerable<ulong> ids)
    {
        foreach (ulong value in ids)
        {
            DiscordSelectDefaultValue defaultValue = new(value, DiscordSelectDefaultValueType.Channel);
            _defaultValues.Add(defaultValue);
        }

        return this;
    }


    /// <summary>
    /// Enables this component.
    /// </summary>
    /// <returns>The current component.</returns>
    public DiscordChannelSelectComponent Enable()
    {
        Disabled = false;
        return this;
    }

    /// <summary>
    /// Disables this component.
    /// </summary>
    /// <returns>The current component.</returns>
    public DiscordChannelSelectComponent Disable()
    {
        Disabled = true;
        return this;
    }

    internal DiscordChannelSelectComponent() => Type = DiscordComponentType.ChannelSelect;

    /// <summary>
    /// Creates a new channel select component.
    /// </summary>
    /// <param name="customId">The ID of this component.</param>
    /// <param name="placeholder">Placeholder text that's shown when no options are selected.</param>
    /// <param name="channelTypes">Optional channel types to filter by.</param>
    /// <param name="disabled">Whether this component is disabled.</param>
    /// <param name="minOptions">The minimum amount of options to be selected.</param>
    /// <param name="maxOptions">The maximum amount of options to be selected, up to 25.</param>
    public DiscordChannelSelectComponent
    (
        string customId,
        string placeholder,
        IEnumerable<DiscordChannelType>? channelTypes = null,
        bool disabled = false,
        int minOptions = 1,
        int maxOptions = 1
    ) : base(DiscordComponentType.ChannelSelect, customId, placeholder, disabled, minOptions, maxOptions) =>
        ChannelTypes = channelTypes?.ToList();
}
