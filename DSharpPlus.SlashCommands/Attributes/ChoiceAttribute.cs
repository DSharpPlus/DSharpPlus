using System;

namespace DSharpPlus.SlashCommands;

/// <summary>
/// Adds a choice for this slash command option.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class ChoiceAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the choice.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the value of the choice.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Adds a choice to the slash command option.
    /// </summary>
    /// <param name="name">The name of the choice.</param>
    /// <param name="value">The value of the choice.</param>
    public ChoiceAttribute(string name, string value)
    {
        this.Name = name;
        this.Value = value;
    }

    /// <summary>
    /// Adds a choice to the slash command option.
    /// </summary>
    /// <param name="name">The name of the choice.</param>
    /// <param name="value">The value of the choice.</param>
    public ChoiceAttribute(string name, long value)
    {
        this.Name = name;
        this.Value = value;
    }

    /// <summary>
    /// Adds a choice to the slash command option.
    /// </summary>
    /// <param name="name">The name of the choice.</param>
    /// <param name="value">The value of the choice.</param>
    public ChoiceAttribute(string name, double value)
    {
        this.Name = name;
        this.Value = value;
    }
}
