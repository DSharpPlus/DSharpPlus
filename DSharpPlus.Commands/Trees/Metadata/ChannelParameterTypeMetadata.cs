using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees.Metadata;

public sealed class ChannelParameterTypeMetadata
{
    public IReadOnlyList<DiscordChannelType> PermissibleChannelTypes { get; init; }
}
