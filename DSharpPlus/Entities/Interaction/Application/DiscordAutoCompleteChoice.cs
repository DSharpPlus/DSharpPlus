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

    [JsonConstructor]
    private DiscordAutoCompleteChoice()
    {
        this.Name = null!;
        this.Value = null!;
    }

    /// <summary>
    /// Creates a new instance of <see cref="DiscordAutoCompleteChoice"/>.
    /// </summary>
    private DiscordAutoCompleteChoice(string name)
    {
        if (name.Length is < 1 or > 100)
        {
            throw new ArgumentException("Application command choice name cannot be empty or exceed 100 characters.", nameof(name));
        }

        this.Name = name;
        this.Value = null!;
    }

    /// <inheritdoc cref="DiscordAutoCompleteChoice(string)"/>
    /// <param name="name">The name of this option, which will be presented to the user.</param>
    /// <param name="value">The value of this option.</param>
    public DiscordAutoCompleteChoice(string name, string value) : this(name)
    {
        if (value.Length > 100)
        {
            throw new ArgumentException("Application command choice value cannot exceed 100 characters.", nameof(value));
        }

        this.Value = value;
    }

    /// <inheritdoc cref="DiscordAutoCompleteChoice(string, string)"/>
    public DiscordAutoCompleteChoice(string name, int value) : this(name) => this.Value = value;

    /// <inheritdoc cref="DiscordAutoCompleteChoice(string, string)"/>
    public DiscordAutoCompleteChoice(string name, long value) : this(name) => this.Value = value;

    /// <inheritdoc cref="DiscordAutoCompleteChoice(string, string)"/>
    public DiscordAutoCompleteChoice(string name, double value) : this(name) => this.Value = value;

    /// <inheritdoc cref="DiscordAutoCompleteChoice(string, string)"/>
    public DiscordAutoCompleteChoice(string name, float value) : this(name) => this.Value = value;
}
