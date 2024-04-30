namespace DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

/// <summary>
/// Adds aliases to this command or group.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class AliasesAttribute : Attribute
{
    /// <summary>
    /// Gets this group's aliases.
    /// </summary>
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>
    /// Adds aliases to this command or group.
    /// </summary>
    /// <param name="aliases">Aliases to add to this command or group.</param>
    public AliasesAttribute(params string[] aliases)
    {
        if (aliases.Any(xa => xa == null || xa.Any(xc => char.IsWhiteSpace(xc))))
        {
            throw new ArgumentException("Aliases cannot contain whitespace characters or null strings.", nameof(aliases));
        }

        Aliases = new ReadOnlyCollection<string>(aliases);
    }
}
