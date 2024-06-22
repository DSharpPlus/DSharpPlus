using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a parameter for a <see cref="DiscordApplicationCommand"/>.
/// </summary>
public sealed class DiscordApplicationCommandOption
{
    /// <summary>
    /// Gets the type of this command parameter.
    /// </summary>
    [JsonProperty("type")]
    public DiscordApplicationCommandOptionType Type { get; internal set; }

    /// <summary>
    /// Gets the name of this command parameter.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the description of this command parameter.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; internal set; }

    /// <summary>
    /// Gets whether this option will auto-complete.
    /// </summary>
    [JsonProperty("autocomplete")]
    public bool? AutoComplete { get; internal set; }

    /// <summary>
    /// Gets whether this command parameter is required.
    /// </summary>
    [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Required { get; internal set; }

    /// <summary>
    /// Gets the optional choices for this command parameter. Not applicable for auto-complete options.
    /// </summary>
    [JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordApplicationCommandOptionChoice> Choices { get; internal set; }

    /// <summary>
    /// Gets the optional subcommand parameters for this parameter.
    /// </summary>
    [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordApplicationCommandOption> Options { get; internal set; }

    /// <summary>
    /// Gets the channel types this command parameter is restricted to, if of type <see cref="DiscordApplicationCommandOptionType.Channel"/>..
    /// </summary>
    [JsonProperty("channel_types", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordChannelType> ChannelTypes { get; internal set; }

    /// <summary>
    /// Gets the minimum value for this slash command parameter.
    /// </summary>
    [JsonProperty("min_value", NullValueHandling = NullValueHandling.Ignore)]
    public object MinValue { get; internal set; }

    /// <summary>
    /// Gets the maximum value for this slash command parameter.
    /// </summary>
    [JsonProperty("max_value", NullValueHandling = NullValueHandling.Ignore)]
    public object MaxValue { get; internal set; }

    /// <summary>
    /// Gets the minimum allowed length for this slash command parameter.
    /// </summary>
    [JsonProperty("min_length", NullValueHandling = NullValueHandling.Ignore)]
    public int? MinLength { get; internal set; }

    /// <summary>
    /// Gets the maximum allowed length for this slash command parameter.
    /// </summary>
    [JsonProperty("max_length", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxLength { get; internal set; }

    /// <summary>
    /// Localized names for this option.
    /// </summary>
    [JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Include)]
    public IReadOnlyDictionary<string, string> NameLocalizations { get; internal set; }

    /// <summary>
    /// Localized descriptions for this option.
    /// </summary>
    [JsonProperty("description_localizations", NullValueHandling = NullValueHandling.Include)]
    public IReadOnlyDictionary<string, string> DescriptionLocalizations { get; internal set; }

    /// <summary>
    /// Creates a new instance of a <see cref="DiscordApplicationCommandOption"/>.
    /// </summary>
    /// <param name="name">The name of this parameter.</param>
    /// <param name="description">The description of the parameter.</param>
    /// <param name="type">The type of this parameter.</param>
    /// <param name="required">Whether the parameter is required.</param>
    /// <param name="choices">The optional choice selection for this parameter.</param>
    /// <param name="options">The optional subcommands for this parameter.</param>
    /// <param name="channelTypes">The channel types to be restricted to for this parameter, if of type <see cref="DiscordApplicationCommandOptionType.Channel"/>.</param>
    /// <param name="autocomplete">Whether this parameter is autocomplete. If true, <paramref name="choices"/> must not be provided.</param>
    /// <param name="minValue">The minimum value for this parameter. Only valid for types <see cref="DiscordApplicationCommandOptionType.Integer"/> or <see cref="DiscordApplicationCommandOptionType.Number"/>.</param>
    /// <param name="maxValue">The maximum value for this parameter. Only valid for types <see cref="DiscordApplicationCommandOptionType.Integer"/> or <see cref="DiscordApplicationCommandOptionType.Number"/>.</param>
    /// <param name="name_localizations">Name localizations for this parameter.</param>
    /// <param name="description_localizations">Description localizations for this parameter.</param>
    /// <param name="minLength">The minimum allowed length for this parameter. Only valid for type <see cref="DiscordApplicationCommandOptionType.String"/>.</param>
    /// <param name="maxLength">The maximum allowed length for this parameter. Only valid for type <see cref="DiscordApplicationCommandOptionType.String"/>.</param>
    public DiscordApplicationCommandOption(string name, string description, DiscordApplicationCommandOptionType type, bool? required = null, IEnumerable<DiscordApplicationCommandOptionChoice> choices = null, IEnumerable<DiscordApplicationCommandOption> options = null, IEnumerable<DiscordChannelType> channelTypes = null, bool? autocomplete = null, object minValue = null, object maxValue = null, IReadOnlyDictionary<string, string> name_localizations = null, IReadOnlyDictionary<string, string> description_localizations = null, int? minLength = null, int? maxLength = null)
    {
        if (!Utilities.IsValidSlashCommandName(name))
        {
            throw new ArgumentException($"Invalid slash command option name specified: {name}. It must be below 32 characters and not contain any whitespace.", nameof(name));
        }

        if (name.Any(char.IsUpper))
        {
            throw new ArgumentException("Slash command option name cannot have any upper case characters.", nameof(name));
        }

        if (description.Length > 100)
        {
            throw new ArgumentException("Slash command option description cannot exceed 100 characters.", nameof(description));
        }

        if ((autocomplete ?? false) && (choices?.Any() ?? false))
        {
            throw new InvalidOperationException("Auto-complete slash command options cannot provide choices.");
        }

        ReadOnlyCollection<DiscordApplicationCommandOptionChoice>? choiceList = choices != null ? new ReadOnlyCollection<DiscordApplicationCommandOptionChoice>(choices.ToList()) : null;
        ReadOnlyCollection<DiscordApplicationCommandOption>? optionList = options != null ? new ReadOnlyCollection<DiscordApplicationCommandOption>(options.ToList()) : null;
        ReadOnlyCollection<DiscordChannelType>? channelTypeList = channelTypes != null ? new ReadOnlyCollection<DiscordChannelType>(channelTypes.ToList()) : null;

        this.Name = name;
        this.Description = description;
        this.Type = type;
        this.AutoComplete = autocomplete;
        this.Required = required;
        this.Choices = choiceList;
        this.Options = optionList;
        this.ChannelTypes = channelTypeList;
        this.MinValue = minValue;
        this.MaxValue = maxValue;
        this.MinLength = minLength;
        this.MaxLength = maxLength;
        this.NameLocalizations = name_localizations;
        this.DescriptionLocalizations = description_localizations;
    }
}
