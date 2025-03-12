using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees.Metadata;

public sealed class ChannelTypeMetadata : INodeMetadataItem
{
    public IReadOnlyList<DiscordChannelType> PermissibleChannelTypes { get; init; }

    public bool SpreadsToChildren => true;
}
