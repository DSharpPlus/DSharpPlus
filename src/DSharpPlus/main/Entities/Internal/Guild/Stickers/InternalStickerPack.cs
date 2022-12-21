using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalStickerPack
    {
        /// <summary>
        /// The id of the sticker pack.
        /// </summary>
        [JsonPropertyName("id")]
        public InternalSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The stickers in the pack.
        /// </summary>
        [JsonPropertyName("stickers")]
        public IReadOnlyList<InternalSticker> Stickers { get; init; } = Array.Empty<InternalSticker>();

        /// <summary>
        /// The name of the sticker pack.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The id of the pack's SKU.
        /// </summary>
        [JsonPropertyName("sku_id")]
        public InternalSnowflake SkuId { get; init; } = null!;

        /// <summary>
        /// The id of a sticker in the pack which is shown as the pack's icon.
        /// </summary>
        [JsonPropertyName("cover_sticker_id")]
        public Optional<InternalSnowflake> CoverStickerId { get; init; }

        /// <summary>
        /// The description of the sticker pack.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; init; } = null!;

        /// <summary>
        /// The id of the sticker pack's <see href="https://discord.com/developers/docs/reference#image-formatting">banner image</see>.
        /// </summary>
        [JsonPropertyName("banner_image_id")]
        public Optional<InternalSnowflake> BannerAssetId { get; init; }
    }
}
