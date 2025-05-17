using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees;

[DebuggerDisplay("{ToString()}")]
public record CommandParameter
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required Type Type { get; init; }
    public IReadOnlyList<Attribute> Attributes { get; init; } = new List<Attribute>();
    public Optional<object?> DefaultValue { get; init; } = Optional.FromNoValue<object?>();
    public required Command Parent { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append(this.Parent.FullName);
        stringBuilder.Append('.');
        stringBuilder.Append(this.Name);
        return stringBuilder.ToString();
    }
}
