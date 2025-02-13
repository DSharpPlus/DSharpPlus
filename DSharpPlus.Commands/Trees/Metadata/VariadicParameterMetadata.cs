using System.Collections.Generic;

namespace DSharpPlus.Commands.Trees.Metadata;

public sealed class VariadicParameterMetadata : INodeMetadataItem
{
    public bool IsVariadic { get; init; }

    public int? MaxVariadicArguments { get; init; }

    public int? MinVariadicArguments { get; init; }

    public IReadOnlyList<string> LowercasedParameterNames { get; init; }
}
