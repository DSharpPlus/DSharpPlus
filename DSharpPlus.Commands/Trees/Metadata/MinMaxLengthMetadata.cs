namespace DSharpPlus.Commands.Trees.Metadata;

public sealed class MinMaxLengthMetadata : INodeMetadataItem
{
    public int? MaxLength { get; init; }

    public int? MinLength { get; init; }
}
