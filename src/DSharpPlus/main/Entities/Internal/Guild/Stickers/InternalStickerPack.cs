using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalStickerPack
{
    /// <summary>
    /// The id of the sticker pack.
    /// </summary>
    [JsonPropertyName("id")]
    public required Snowflake Id { get; init; }

    /// <summary>
    /// The stickers in the pack.
    /// </summary>
    [JsonPropertyName("stickers")]
    public required IReadOnlyList<InternalSticker> Stickers { get; init; }

    /// <summary>
    /// The name of the sticker pack.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The id of the pack's SKU.
    /// </summary>
    [JsonPropertyName("sku_id")]
    public required Snowflake SkuId { get; init; }

    /// <summary>
    /// The id of a sticker in the pack which is shown as the pack's icon.
    /// </summary>
    [JsonPropertyName("cover_sticker_id")]
    public Optional<Snowflake> CoverStickerId { get; init; }

    /// <summary>
    /// The description of the sticker pack.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; } 

    /// <summary>
    /// The id of the sticker pack's 
    /// <see href="https://discord.com/developers/docs/reference#image-formatting">banner image</see>.
    /// </summary>
    [JsonPropertyName("banner_image_id")]
    public Optional<Snowflake> BannerAssetId { get; init; }
}
