
using System;

namespace DSharpPlus.CommandsNext.Attributes;
/// <summary>
/// Gives this command, group, or argument a description, which is used when listing help.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class DescriptionAttribute : Attribute
{
    /// <summary>
    /// Gets the description for this command, group, or argument.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gives this command, group, or argument a description, which is used when listing help.
    /// </summary>
    /// <param name="description"></param>
    public DescriptionAttribute(string description) => Description = description;
}
