namespace DSharpPlus.Commands;
using System;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ParameterAttribute : Attribute
{
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Creates a new instance of the <see cref="CommandAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    public ParameterAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "The name of the parameter cannot be null or whitespace.");
        }
        else if (name.Length is < 1 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "The name of the parameter must be between 1 and 32 characters.");
        }

        Name = name;
    }
}
