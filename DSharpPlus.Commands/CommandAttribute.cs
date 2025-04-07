using System;
using System.Linq;

namespace DSharpPlus.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>
    /// The name of the command. This may be multiple names, in which case the containing type is set to represent a nested command.
    /// </summary>
    public string[] Names { get; init; }

    /// <summary>
    /// Creates a new instance of the <see cref="CommandAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    public CommandAttribute(params string[] name)
    {
        if (name.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentNullException(nameof(name), "The name of the command cannot be null or whitespace.");
        }

        this.Names = name;
    }
}
