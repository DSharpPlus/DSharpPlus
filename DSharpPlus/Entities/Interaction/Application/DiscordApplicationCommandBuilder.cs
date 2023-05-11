using System.Collections.Generic;

namespace DSharpPlus.Entities;

public class DiscordApplicationCommandBuilder
{
    private List<DiscordApplicationCommandOption> _options = new();
    private Dictionary<string, string> _nameLocalizations = new();
    private Dictionary<string, string> _descriptionLocalizations = new();

    public ApplicationCommandType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public IReadOnlyList<DiscordApplicationCommandOption> Options => _options;
    public bool? DefaultPermissions { get; private set; }
    public bool? AllowDMUsage { get; private set; }
    public Permissions? DefaultMemberPermissions { get; private set; }
    public IReadOnlyDictionary<string, string> NameLocalizations => _nameLocalizations;
    public IReadOnlyDictionary<string, string> DescriptionLocalizations => _descriptionLocalizations;

    public DiscordApplicationCommandBuilder WithType(ApplicationCommandType type)
    {
        Type = type;
        return this;
    }

    public DiscordApplicationCommandBuilder WithName(string name)
    {
        Name = name;
        return this;
    }

    public DiscordApplicationCommandBuilder WithDescription(string description)
    {
        Description = description;
        return this;
    }

    public DiscordApplicationCommandBuilder AddOption(DiscordApplicationCommandOption option)
    {
        _options.Add(option);
        return this;
    }

    public DiscordApplicationCommandBuilder AddOptions(params DiscordApplicationCommandOption[] options)
    {
        _options.AddRange(options);
        return this;
    }

    public DiscordApplicationCommandBuilder WithDefaultPermissions(bool permissions)
    {
        DefaultPermissions = permissions;
        return this;
    }

    public DiscordApplicationCommandBuilder WithAllowDMUsage(bool @bool)
    {
        AllowDMUsage = @bool;
        return this;
    }

    public DiscordApplicationCommandBuilder WithDefaultMemberPermissions(Permissions permissions)
    {
        DefaultMemberPermissions = permissions;
        return this;
    }

    public DiscordApplicationCommandBuilder AddNameLocalization(string language, string name)
    {
        _nameLocalizations.Add(language, name);
        return this;
    }

    public DiscordApplicationCommandBuilder AddDescriptionLocalization(string language, string description)
    {
        _descriptionLocalizations.Add(language, description);
        return this;
    }

    public DiscordApplicationCommand Build()
        => new DiscordApplicationCommand(Name, Description, _options, DefaultPermissions, Type, _nameLocalizations,
            _descriptionLocalizations, AllowDMUsage, DefaultMemberPermissions);
}
