using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// The smallest amount of data required to render a sticker. A partial sticker object.
    /// </summary>
    public sealed record DiscordMessageStickerItem
    {
        /// <summary>
        /// The id of the sticker.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The name of the sticker.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The type of sticker format.
        /// </summary>
        [JsonProperty("format_type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordStickerFormatType FormatType { get; init; }
    }
}
