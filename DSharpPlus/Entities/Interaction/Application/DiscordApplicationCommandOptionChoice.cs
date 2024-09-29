using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a choice for a <see cref="DiscordApplicationCommandOption"/> parameter.
/// </summary>
public sealed class DiscordApplicationCommandOptionChoice
{
    /// <summary>
    /// Gets the name of this choice.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets the value of this choice. This will either be a type of <see cref="int"/> / <see cref="long"/>, <see cref="double"/> or <see cref="string"/>.
    /// </summary>
    [JsonProperty("value")]
    public object Value { get; set; }

    /// <summary>
    /// Gets the localized names for this choice.
    /// </summary>
    /// <remarks>
    /// The keys must be appropriate locales as documented by Discord:
    /// <seealso href="https://discord.com/developers/docs/reference#locales"/>.
    /// </remarks>
    [JsonProperty("name_localizations")]
    public IReadOnlyDictionary<string, string>? NameLocalizations { get; set; }

    [JsonConstructor]
    private DiscordApplicationCommandOptionChoice()
    {
        this.Name = null!;
        this.Value = null!;
        this.NameLocalizations = null;
    }

    /// <summary>
    /// Creates a new instance of a <see cref="DiscordApplicationCommandOptionChoice"/>.
    /// </summary>
    private DiscordApplicationCommandOptionChoice(
        string name,
        IReadOnlyDictionary<string, string>? nameLocalizations
    )
    {
        if (name.Length is < 1 or > 100)
        {
            throw new ArgumentException(
                "Application command choice name cannot be empty or exceed 100 characters.",
                nameof(name)
            );
        }

        if (nameLocalizations is not null)
        {
            foreach ((string locale, string localized) in nameLocalizations)
            {
                if (localized.Length is < 1 or > 100)
                {
                    throw new ArgumentException(
                        $"Localized application command choice name for locale {locale} cannot be empty or exceed 100 characters. Value: '{localized}'",
                        nameof(nameLocalizations)
                    );
                }
            }
        }

        this.Name = name;
        this.Value = null!;
        this.NameLocalizations = nameLocalizations;
    }

    /// <inheritdoc cref="DiscordApplicationCommandOptionChoice(string, IReadOnlyDictionary{string, string}?)"/>
    /// <param name="name">The name of this choice.</param>
    /// <param name="value">The value of this choice.</param>
    /// <param name="nameLocalizations">
    /// Localized names for this choice. The keys must be appropriate locales as documented by Discord:
    /// <seealso href="https://discord.com/developers/docs/reference#locales"/>.
    /// </param>
    public DiscordApplicationCommandOptionChoice(
        string name,
        string value,
        IReadOnlyDictionary<string, string>? nameLocalizations = null
    )
        : this(name, nameLocalizations)
    {
        if (value.Length > 100)
        {
            throw new ArgumentException(
                "Application command choice value cannot exceed 100 characters.",
                nameof(value)
            );
        }

        this.Value = value;
    }

    /// <inheritdoc cref="DiscordApplicationCommandOptionChoice(string, string, IReadOnlyDictionary{string, string}?)"/>
    public DiscordApplicationCommandOptionChoice(
        string name,
        int value,
        IReadOnlyDictionary<string, string>? nameLocalizations = null
    )
        : this(name, nameLocalizations) => this.Value = value;

    /// <inheritdoc cref="DiscordApplicationCommandOptionChoice(string, string, IReadOnlyDictionary{string, string}?)"/>
    public DiscordApplicationCommandOptionChoice(
        string name,
        long value,
        IReadOnlyDictionary<string, string>? nameLocalizations = null
    )
        : this(name, nameLocalizations) => this.Value = value;

    /// <inheritdoc cref="DiscordApplicationCommandOptionChoice(string, string, IReadOnlyDictionary{string, string}?)"/>
    public DiscordApplicationCommandOptionChoice(
        string name,
        double value,
        IReadOnlyDictionary<string, string>? nameLocalizations = null
    )
        : this(name, nameLocalizations) => this.Value = value;

    /// <inheritdoc cref="DiscordApplicationCommandOptionChoice(string, string, IReadOnlyDictionary{string, string}?)"/>
    public DiscordApplicationCommandOptionChoice(
        string name,
        float value,
        IReadOnlyDictionary<string, string>? nameLocalizations = null
    )
        : this(name, nameLocalizations) => this.Value = value;
}
