namespace DSharpPlus.Commands.Trees.Metadata;

public sealed class MinMaxValueMetadata : INodeMetadataItem
{
    public object? MaxValue { get; init; }

    public object? MinValue { get; init; }
}
