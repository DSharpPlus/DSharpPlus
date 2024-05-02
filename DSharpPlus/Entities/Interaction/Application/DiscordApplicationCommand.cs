using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a command that is registered to an application.
/// </summary>
public sealed class DiscordApplicationCommand : SnowflakeObject, IEquatable<DiscordApplicationCommand>
{
    /// <summary>
    /// Gets the unique ID of this command's application.
    /// </summary>
    [JsonProperty("application_id")]
    public ulong ApplicationId { get; internal set; }

    /// <summary>
    /// Gets the type of this application command.
    /// </summary>
    [JsonProperty("type")]
    public DiscordApplicationCommandType Type { get; internal set; }

    /// <summary>
    /// Gets the name of this command.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the description of this command.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; internal set; }

    /// <summary>
    /// Gets the potential parameters for this command.
    /// </summary>
    [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyCollection<DiscordApplicationCommandOption> Options { get; internal set; }

    /// <summary>
    /// Gets whether the command is enabled by default when the application is added to a guild.
    /// </summary>
    [JsonProperty("default_permission")]
    public bool? DefaultPermission { get; internal set; }

    /// <summary>
    /// Whether this command can be invoked in DMs.
    /// </summary>
    [JsonProperty("dm_permission")]
    public bool? AllowDMUsage { get; internal set; }

    /// <summary>
    /// What permissions this command requires to be invoked.
    /// </summary>
    [JsonProperty("default_member_permissions")]
    public DiscordPermissions? DefaultMemberPermissions { get; internal set; }

    /// <summary>
    /// Whether this command is age-restricted.
    /// </summary>
    [JsonProperty("nsfw")]
    public bool? NSFW { get; internal set; }

    /// <summary>
    /// Gets the auto-incrementing version number for this command.
    /// </summary>
    [JsonProperty("version")]
    public ulong Version { get; internal set; }

    [JsonProperty("name_localizations")]
    public IReadOnlyDictionary<string, string> NameLocalizations { get; internal set; }

    [JsonProperty("description_localizations")]
    public IReadOnlyDictionary<string, string> DescriptionLocalizations { get; internal set; }

    /// <summary>
    /// Contexts in which this command can be invoked.
    /// </summary>
    [JsonProperty("contexts")]
    public IReadOnlyList<DiscordInteractionContextType>? Contexts { get; internal set; }

    /// <summary>
    /// Contexts in which this command can be installed.
    /// </summary>
    [JsonProperty("integration_types")]
    public IReadOnlyList<DiscordApplicationIntegrationType>? IntegrationTypes { get; internal set; }

    /// <summary>
    /// Gets the command's mention string.
    /// </summary>
    [JsonIgnore]
    public string Mention
        => Formatter.Mention(this);

    /// <summary>
    /// Creates a new instance of a <see cref="DiscordApplicationCommand"/>.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <param name="description">The description of the command.</param>
    /// <param name="options">Optional parameters for this command.</param>
    /// <param name="defaultPermission">Whether the command is enabled by default when the application is added to a guild.</param>
    /// <param name="type">The type of the application command</param>
    /// <param name="name_localizations">Localization dictionary for <paramref name="name"/> field. Values follow the same restrictions as <paramref name="name"/>.</param>
    /// <param name="description_localizations">Localization dictionary for <paramref name="description"/> field. Values follow the same restrictions as <paramref name="description"/>.</param>
    /// <param name="allowDMUsage">Whether this command can be invoked in DMs.</param>
    /// <param name="defaultMemberPermissions">What permissions this command requires to be invoked.</param>
    /// <param name="nsfw">Whether the command is age restricted.</param>
    /// <param name="contexts">The contexts in which the command is allowed to be run in.</param>
    /// <param name="integrationTypes">The installation contexts the command can be installed to.</param>
    public DiscordApplicationCommand
    (
        string name,
        string description,
        IEnumerable<DiscordApplicationCommandOption> options = null,
        bool? defaultPermission = null,
        DiscordApplicationCommandType type = DiscordApplicationCommandType.SlashCommand,
        IReadOnlyDictionary<string, string> name_localizations = null,
        IReadOnlyDictionary<string, string> description_localizations = null,
        bool? allowDMUsage = null,
        DiscordPermissions? defaultMemberPermissions = null,
        bool? nsfw = null,
        IReadOnlyList<DiscordInteractionContextType>? contexts = null,
        IReadOnlyList<DiscordApplicationIntegrationType>? integrationTypes = null
    )
    {
        if (type is DiscordApplicationCommandType.SlashCommand)
        {
            if (!Utilities.IsValidSlashCommandName(name))
            {
                throw new ArgumentException("Invalid slash command name specified. It must be below 32 characters and not contain any whitespace.", nameof(name));
            }

            if (name.Any(ch => char.IsUpper(ch)))
            {
                throw new ArgumentException("Slash command name cannot have any upper case characters.", nameof(name));
            }

            if (description.Length > 100)
            {
                throw new ArgumentException("Slash command description cannot exceed 100 characters.", nameof(description));
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Context menus do not support descriptions.");
            }

            if (options?.Any() ?? false)
            {
                throw new ArgumentException("Context menus do not support options.");
            }
        }

        ReadOnlyCollection<DiscordApplicationCommandOption>? optionsList = options != null ? new ReadOnlyCollection<DiscordApplicationCommandOption>(options.ToList()) : null;

        Type = type;
        Name = name;
        Description = description;
        Options = optionsList;
        DefaultPermission = defaultPermission;
        NameLocalizations = name_localizations;
        DescriptionLocalizations = description_localizations;
        AllowDMUsage = allowDMUsage;
        DefaultMemberPermissions = defaultMemberPermissions;
        NSFW = nsfw;
        Contexts = contexts;
        IntegrationTypes = integrationTypes;
    }

    /// <summary>
    /// Creates a mention for a subcommand.
    /// </summary>
    /// <param name="name">The name of the subgroup and/or subcommand.</param>
    /// <returns>Formatted mention.</returns>
    public string GetSubcommandMention(params string[] name) => !Options.Any(x => x.Name == name[0])
            ? throw new ArgumentException("Specified subgroup/subcommand doesn't exist.")
            : $"</{Name} {string.Join(" ", name)}:{Id.ToString(CultureInfo.InvariantCulture)}>";

    /// <summary>
    /// Checks whether this <see cref="DiscordApplicationCommand"/> object is equal to another object.
    /// </summary>
    /// <param name="other">The command to compare to.</param>
    /// <returns>Whether the command is equal to this <see cref="DiscordApplicationCommand"/>.</returns>
    public bool Equals(DiscordApplicationCommand other)
        => Id == other.Id;

    /// <summary>
    /// Determines if two <see cref="DiscordApplicationCommand"/> objects are equal.
    /// </summary>
    /// <param name="e1">The first command object.</param>
    /// <param name="e2">The second command object.</param>
    /// <returns>Whether the two <see cref="DiscordApplicationCommand"/> objects are equal.</returns>
    public static bool operator ==(DiscordApplicationCommand e1, DiscordApplicationCommand e2)
        => e1.Equals(e2);

    /// <summary>
    /// Determines if two <see cref="DiscordApplicationCommand"/> objects are not equal.
    /// </summary>
    /// <param name="e1">The first command object.</param>
    /// <param name="e2">The second command object.</param>
    /// <returns>Whether the two <see cref="DiscordApplicationCommand"/> objects are not equal.</returns>
    public static bool operator !=(DiscordApplicationCommand e1, DiscordApplicationCommand e2)
        => !(e1 == e2);

    /// <summary>
    /// Determines if a <see cref="object"/> is equal to the current <see cref="DiscordApplicationCommand"/>.
    /// </summary>
    /// <param name="other">The object to compare to.</param>
    /// <returns>Whether the two <see cref="DiscordApplicationCommand"/> objects are not equal.</returns>
    public override bool Equals(object other)
        => other is DiscordApplicationCommand dac && Equals(dac);

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordApplicationCommand"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordApplicationCommand"/>.</returns>
    public override int GetHashCode()
        => Id.GetHashCode();
}
