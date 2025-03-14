using System;

namespace DSharpPlus.Commands;

/// <summary>
/// Specifies the description of a given command or parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
public sealed class DescriptionAttribute : Attribute
{
    /// <summary>
    /// The description of the command or parameter.
    /// </summary>
    public string Description { get; internal set; }

    public DescriptionAttribute(string description)
        => this.Description = description;
}
