namespace DSharpPlus.Entities.Internal;

/// <summary>
/// The smallest amount of data required to render a sticker. A partial sticker object.
/// </summary>
public sealed record InternalStickerItem
{
    /// <summary>
    /// The id of the sticker.
    /// </summary>
    public required Snowflake Id { get; init; }

    /// <summary>
    /// The name of the sticker.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The type of sticker format.
    /// </summary>
    public required DiscordStickerFormatType FormatType { get; init; }
}
