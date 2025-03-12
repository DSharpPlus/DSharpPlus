namespace DSharpPlus.Commands.Trees.Metadata;

public sealed class NsfwChannelMetadata : INodeMetadataItem
{
    public bool ExcludeDms { get; init; }

    public bool SpreadsToChildren => true;
}
