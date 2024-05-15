using System;

namespace DSharpPlus.Commands.Trees.Metadata;

/// <summary>
/// Stores the snake-cased name of a command parameter, for processors that cannot use upper-case names.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class SnakeCasedNameAttribute : Attribute
{
    /// <summary>
    /// The snake-cased name.
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    /// Creates a new instance of this attribute.
    /// </summary>
    public SnakeCasedNameAttribute(string name)
        => this.Name = name;
}
