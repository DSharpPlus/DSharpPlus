namespace DSharpPlus.Commands.Trees.Metadata;

public sealed class LocationMetadata : INodeMetadataItem
{
    /// <summary>
    /// Specifies whether this predicate is fulfilled within guilds.
    /// </summary>
    public bool IsAllowedInGuilds { get; init; } = true;

    /// <summary>
    /// Specifies whether this predicate is fulfilled within DMs between an user and the bot.
    /// </summary>
    public bool IsAllowedInBotDms { get; init; }

    /// <summary>
    /// Specifies whether this predicate is fulfilled within DMs between users without the bot.
    /// </summary>
    public bool IsAllowedInOtherDms { get; init; }

    public bool SpreadsToChildren => true;
}
