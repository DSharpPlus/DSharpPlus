using System;

namespace DSharpPlus.Commands.Trees.Metadata;

public sealed class AutocompleteProviderMetadata : INodeMetadataItem
{
    public Type? AutocompleteProvider { get; init; }
}
