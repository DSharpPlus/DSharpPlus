using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordStickerPack
    {
        /// <summary>
        /// The id of the sticker pack.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The stickers in the pack.
        /// </summary>
        [JsonProperty("stickers", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordSticker> Stickers { get; init; } = Array.Empty<DiscordSticker>();

        /// <summary>
        /// The name of the sticker pack.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The id of the pack's SKU.
        /// </summary>
        [JsonProperty("sku_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake SkuId { get; init; } = null!;

        /// <summary>
        /// The id of a sticker in the pack which is shown as the pack's icon.
        /// </summary>
        [JsonProperty("cover_sticker_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> CoverStickerId { get; init; }

        /// <summary>
        /// The description of the sticker pack.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; init; } = null!;

        /// <summary>
        /// The id of the sticker pack's <see href="https://discord.com/developers/docs/reference#image-formatting">banner image</see>.
        /// </summary>
        [JsonProperty("banner_image_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> BannerAssetId { get; init; }
    }
}
