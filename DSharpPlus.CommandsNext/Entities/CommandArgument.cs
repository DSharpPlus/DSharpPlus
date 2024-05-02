
using System;
using System.Collections.Generic;

namespace DSharpPlus.CommandsNext;
public sealed class CommandArgument
{
    /// <summary>
    /// Gets this argument's name.
    /// </summary>
    public string Name { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets this argument's type.
    /// </summary>
    public Type Type { get; internal set; } = null!;

    /// <summary>
    /// Gets or sets whether this argument is an array argument.
    /// </summary>
    internal bool _isArray { get; set; } = false;

    /// <summary>
    /// Gets whether this argument is optional.
    /// </summary>
    public bool IsOptional { get; internal set; }

    /// <summary>
    /// Gets this argument's default value.
    /// </summary>
    public object? DefaultValue { get; internal set; }

    /// <summary>
    /// Gets whether this argument catches all remaining arguments.
    /// </summary>
    public bool IsCatchAll { get; internal set; }

    /// <summary>
    /// Gets this argument's description.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    /// Gets the custom attributes attached to this argument.
    /// </summary>
    public IReadOnlyCollection<Attribute> CustomAttributes { get; internal set; } = Array.Empty<Attribute>();
}
