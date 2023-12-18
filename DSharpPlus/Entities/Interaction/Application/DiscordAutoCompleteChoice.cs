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
    public object Value { get; internal set; }

    /// <summary>
    /// Creates a new instance of <see cref="DiscordAutoCompleteChoice"/>.
    /// </summary>
    /// <param name="name">The name of this option, which will be presented to the user.</param>
    /// <param name="value">The value of this option.</param>
    public DiscordAutoCompleteChoice(string name, object value)
    {
        if (value is not (string or int or long))
        {
            throw new ArgumentException($"Object type must be of {typeof(int)} or {typeof(string)} or {typeof(long)}", nameof(value));
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
