using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees.Metadata;

public sealed class ChannelTypeMetadata : INodeMetadataItem
{
    public DiscordChannelType ChannelType { get; init; }

    public bool SpreadsToChildren => true;
}
