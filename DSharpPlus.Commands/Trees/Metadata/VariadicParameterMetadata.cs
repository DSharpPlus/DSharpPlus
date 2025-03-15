using System.Collections.Generic;

namespace DSharpPlus.Commands.Trees.Metadata;

public sealed class VariadicParameterMetadata : INodeMetadataItem
{
    internal List<string> lowercasedNames = [];

    public int? MaxVariadicArguments { get; init; }

    public int? MinVariadicArguments { get; init; }

    public IReadOnlyList<string> LowercasedParameterNames 
    { 
        get => this.lowercasedNames; 
        init => this.lowercasedNames = [.. value]; 
    }
}
