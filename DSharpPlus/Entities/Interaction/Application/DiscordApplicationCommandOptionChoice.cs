
using System;
using System.Collections.Generic;
using System.Linq;

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

    [JsonProperty("name_localizations")]
    public IReadOnlyDictionary<string, string>? NameLocalizations { get; set; }

    /// <summary>
    /// Creates a new instance of a <see cref="DiscordApplicationCommandOptionChoice"/>.
    /// </summary>
    /// <param name="name">The name of this choice.</param>
    /// <param name="value">The value of this choice.</param>
    /// <param name="localizations">
    /// Localized names for this choice. The keys must be appropriate locales as documented by Discord:
    /// <seealso href="https://discord.com/developers/docs/reference#locales"/>.
    /// </param>
    public DiscordApplicationCommandOptionChoice(string name, object value, IReadOnlyDictionary<string, string>? localizations = null)
    {
        if (value is not (string or long or int or double or float))
        {
            throw new InvalidOperationException
            (
                "Only strings, floating point and integer types may be passed to a command option choice."
            );
        }

        if (name.Length is < 1 or > 100)
        {
            throw new ArgumentException
            (
                "Application command choice name cannot be empty or longer than 100 characters.",
                nameof(name)
            );
        }

        if (value is string val && val.Length > 100)
        {
            throw new ArgumentException("Application command choice value cannot exceed 100 characters.", nameof(value));
        }

        if (localizations is not null && localizations.Values.Any(localized => localized.Length is < 1 or > 100))
        {
            throw new ArgumentException
            (
                "Localized application command choice names cannot be empty or longer than 100 characters.",
                nameof(localizations)
            );
        }

        Name = name;
        Value = value;
        NameLocalizations = localizations;
    }
}
