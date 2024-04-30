namespace DSharpPlus.CommandsNext.Attributes;

using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class CategoryAttribute : Attribute
{
    public string? Name { get; }

    public CategoryAttribute(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "Command category names cannot be null, empty, or all-whitespace.");
        }

        Name = name;
    }
}
