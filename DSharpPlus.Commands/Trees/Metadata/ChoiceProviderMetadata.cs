using System;

namespace DSharpPlus.Commands.Trees.Metadata;

internal class ChoiceProviderMetadata : INodeMetadataItem
{
    public Type? ChoiceProvider { get; init; }
}
