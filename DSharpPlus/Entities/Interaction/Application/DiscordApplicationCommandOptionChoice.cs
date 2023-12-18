using System;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a command parameter choice for a <see cref="DiscordApplicationCommandOption"/>.
/// </summary>
public sealed class DiscordApplicationCommandOptionChoice
{
    /// <summary>
    /// Gets the name of this choice parameter.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets the value of this choice parameter. This will either be a type of <see cref="int"/> / <see cref="long"/>, <see cref="double"/> or <see cref="string"/>.
    /// </summary>
    [JsonProperty("value")]
    public object Value { get; set; }

    /// <summary>
    /// Creates a new instance of a <see cref="DiscordApplicationCommandOptionChoice"/>.
    /// </summary>
    /// <param name="name">The name of the parameter choice.</param>
    /// <param name="value">The value of the parameter choice.</param>
    public DiscordApplicationCommandOptionChoice(string name, object value)
    {
        if (!(value is string || value is long || value is int || value is double))
        {
            throw new InvalidOperationException($"Only {typeof(string)}, {typeof(long)}, {typeof(double)} or {typeof(int)} types may be passed to a command option choice.");
        }

        if (name.Length > 100)
        {
            throw new ArgumentException("Application command choice name cannot exceed 100 characters.", nameof(name));
        }

        if (value is string val && val.Length > 100)
        {
            throw new ArgumentException("Application command choice value cannot exceed 100 characters.", nameof(value));
        }

        this.Name = name;
        this.Value = value;
    }
}
