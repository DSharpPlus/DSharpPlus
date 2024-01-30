namespace DSharpPlus.Commands.Trees;

using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

public record CommandParameter
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required Type Type { get; init; }
    public IReadOnlyList<Attribute> Attributes { get; init; } = new List<Attribute>();
    public Optional<object?> DefaultValue { get; init; } = Optional.FromNoValue<object?>();
}
