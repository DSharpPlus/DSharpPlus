using System;
using System.Collections.Generic;

using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees;

public class ParameterNode
{
    public string Name { get; init; }

    public string LowercasedName { get; init; }

    public string Description { get; init; }

    public Type ParameterType { get; init; }

    public IReadOnlyList<ParameterCheckAttribute> CheckAttributes { get; init; }

    public bool IsRequired { get; init; }

    public Optional<object?> DefaultValue { get; init; } = Optional.FromNoValue<object?>();

    public NodeMetadataCollection Metadata { get; init; }
}
