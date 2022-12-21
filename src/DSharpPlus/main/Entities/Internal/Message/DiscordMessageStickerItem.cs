using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// The smallest amount of data required to render a sticker. A partial sticker object.
    /// </summary>
    public sealed record DiscordMessageStickerItem
    {
        /// <summary>
        /// The id of the sticker.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The name of the sticker.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The type of sticker format.
        /// </summary>
        [JsonPropertyName("format_type")]
        public DiscordStickerFormatType FormatType { get; init; }
    }
}
