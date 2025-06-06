using System;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an option for a user to select for auto-completion.
/// </summary>
public sealed class DiscordAutoCompleteChoice
{
    /// <summary>
    /// Gets the name of this option which will be presented to the user.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the value of this option. This may be a string or an integer.
    /// </summary>
    [JsonProperty("value")]
    public object? Value { get; internal set; }

    [JsonConstructor]
    private DiscordAutoCompleteChoice() => this.Name = null!;

    /// <summary>
    /// Creates a new instance of <see cref="DiscordAutoCompleteChoice"/>.
    /// </summary>
    private DiscordAutoCompleteChoice(string name)
    {
        if (name.Length is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "Application command choice name cannot be empty or exceed 100 characters.");
        }

        this.Name = name;
    }

    /// <inheritdoc cref="DiscordAutoCompleteChoice(string)"/>
    /// <param name="name">The name of this option, which will be presented to the user.</param>
    /// <param name="value">The value of this option.</param>
    public DiscordAutoCompleteChoice(string name, object? value) : this(name)
    {
        this.Value = value switch
        {
            string s => CheckStringValue(s),
            byte b => b,
            sbyte sb => sb,
            short s => s,
            ushort us => us,
            int i => this.Value = i,
            uint ui => this.Value = ui,
            long l => this.Value = l,
            ulong ul => this.Value = ul,
            double d => this.Value = d,
            float f => this.Value = f,
            decimal dec => this.Value = dec,
            null => null,
            _ => throw new ArgumentException("Invalid value type.", nameof(value))
        };
    }

    private static string CheckStringValue(string value)
    {
        return value.Length > 100
            ? throw new ArgumentException("Application command choice value cannot exceed 100 characters.", nameof(value))
            : value;
    }
}
