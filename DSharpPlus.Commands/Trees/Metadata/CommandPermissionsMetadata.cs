using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees.Metadata;

public sealed class CommandPermissionsMetadata : INodeMetadataItem
{
    public required DiscordPermissions BotPermissions { get; init; }

    public required DiscordPermissions UserPermissions { get; init; }

    public bool SpreadsToChildren => true;
}
