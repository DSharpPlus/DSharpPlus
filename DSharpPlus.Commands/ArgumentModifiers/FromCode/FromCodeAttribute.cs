namespace DSharpPlus.Commands.ArgumentModifiers;

using System;

/// <summary>
/// Removes the need to manually parse code blocks from a string.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed partial class FromCodeAttribute : Attribute
{
    /// <summary>
    /// The type of code block to accept.
    /// </summary>
    public CodeType CodeType { get; init; }

    /// <summary>
    /// Creates a new <see cref="FromCodeAttribute"/> with the specified <paramref name="codeType"/>.
    /// </summary>
    /// <param name="codeType">The type of code block to accept.</param>
    public FromCodeAttribute(CodeType codeType = CodeType.All) => CodeType = codeType;
}
