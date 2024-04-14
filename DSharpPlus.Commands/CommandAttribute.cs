namespace DSharpPlus.Commands;

using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>
    /// The name of the command.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Creates a new instance of the <see cref="CommandAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    public CommandAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "The name of the command cannot be null or whitespace.");
        }
        else if (name.Length is < 1 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "The name of the command must be between 1 and 32 characters.");
        }

        this.Name = name;
    }
}
