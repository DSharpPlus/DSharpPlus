using System.Collections.Generic;

namespace DSharpPlus.Entities;

public class DiscordApplicationCommandOptionBuilder
{
    private List<DiscordApplicationCommandOptionChoice> _choices = new();
    private List<DiscordApplicationCommandOption> _options = new();
    private List<ChannelType> _channelTypes = new();
    private Dictionary<string, string> _nameLocalization = new();
    private Dictionary<string, string> _descriptionLocalization = new();

    public ApplicationCommandOptionType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool AutoComplete { get; private set; } = false;
    public bool Required { get; private set; } = false;
    public IReadOnlyList<DiscordApplicationCommandOptionChoice> Choices => _choices;
    public IReadOnlyList<DiscordApplicationCommandOption> Options => _options;
    public IReadOnlyList<ChannelType> ChannelTypes => _channelTypes;
    public IReadOnlyDictionary<string, string> NameLocalization => _nameLocalization;
    public IReadOnlyDictionary<string, string> DescriptionLocalization => _descriptionLocalization;
    public Optional<double> MinValue { get; private set; }
    public Optional<double> MaxValue { get; private set; }
    public Optional<int> MinLength { get; private set; }
    public Optional<int> MaxLength { get; private set; }

    public DiscordApplicationCommandOptionBuilder WithType(ApplicationCommandOptionType type)
    {
        Type = type;
        return this;
    }
    
    public DiscordApplicationCommandOptionBuilder WithName(string name)
    {
        Name = name;
        return this;
    }

    public DiscordApplicationCommandOptionBuilder WithDescription(string description)
    {
        Description = description;
        return this;
    }

    public DiscordApplicationCommandOptionBuilder WithRequired(bool required)
    {
        Required = required;
        return this;
    }

    public DiscordApplicationCommandOptionBuilder WithAutoComplete(bool autoComplete)
    {
        AutoComplete = autoComplete;
        return this;
    }

    public DiscordApplicationCommandOptionBuilder AddChoice(DiscordApplicationCommandOptionChoice choice)
    {
        _choices.Add(choice);
        return this;
    }

    public DiscordApplicationCommandOptionBuilder AddChoices(params DiscordApplicationCommandOptionChoice[] choices)
    {
        _choices.AddRange(choices);
        return this;
    }

    public DiscordApplicationCommandOptionBuilder AddOption(DiscordApplicationCommandOption option)
    {
        _options.Add(option);
        return this;
    }

    public DiscordApplicationCommandOptionBuilder AddOptions(params DiscordApplicationCommandOption[] options)
    {
        _options.AddRange(options);
        return this;
    }

    public DiscordApplicationCommandOptionBuilder AddNameLocalization(string key, string value)
    {
        _nameLocalization.Add(key, value);
        return this;
    }

    public DiscordApplicationCommandOptionBuilder AddDescriptionLocalization(string key, string value)
    {
        _descriptionLocalization.Add(key, value);
        return this;
    }

    public DiscordApplicationCommandOptionBuilder WithMinValue(Optional<double> value)
    {
        MinValue = value;
        return this;
    }

    public DiscordApplicationCommandOptionBuilder WithMaxValue(Optional<double> value)
    {
        MaxValue = value;
        return this;
    }

    public DiscordApplicationCommandOptionBuilder WithMaxLength(Optional<int> value)
    {
        MaxLength = value;
        return this;
    }

    public DiscordApplicationCommandOptionBuilder WithMinLength(Optional<int> value)
    {
        MinLength = value;
        return this;
    }

    public DiscordApplicationCommandOption Build()
        => new DiscordApplicationCommandOption(Name, Description, Type, Required, _choices, _options, _channelTypes,
            AutoComplete, MinValue, MaxValue, _nameLocalization, _descriptionLocalization,
            MinLength.HasValue ? MinLength.Value : null, MaxLength.HasValue ? MinLength.Value : null);
}
