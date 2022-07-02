using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// The smallest amount of data required to render a sticker. A partial sticker object.
    /// </summary>
    public sealed record DiscordStickerItem
    {
        /// <summary>
        /// The id of the sticker.
        /// </summary>
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The name of the sticker.
        /// </summary>
        public string Name { get; init; } = null!;

        /// <summary>
        /// The type of sticker format.
        /// </summary>
        public DiscordStickerFormatType FormatType { get; init; }
    }
}
