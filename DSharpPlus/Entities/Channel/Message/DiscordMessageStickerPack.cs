namespace DSharpPlus.Entities;

using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Represents a Discord message sticker pack.
/// </summary>
public sealed class DiscordMessageStickerPack : SnowflakeObject
{
    /// <summary>
    /// Gets the stickers contained in this pack.
    /// </summary>
    public IReadOnlyDictionary<ulong, DiscordMessageSticker> Stickers => _stickers;

    [JsonProperty("stickers")]
    internal Dictionary<ulong, DiscordMessageSticker> _stickers = [];

    /// <summary>
    /// Gets the name of this sticker pack.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; } = default!;

    /// <summary>
    /// Gets the Id of this pack's SKU.
    /// </summary>
    [JsonProperty("sku_id")]
    public ulong SkuId { get; internal set; }

    /// <summary>
    /// Gets the Id of this pack's cover.
    /// </summary>
    [JsonProperty("cover_sticker_id")]
    public ulong CoverStickerId { get; internal set; }

    /// <summary>
    /// Gets the description of this sticker pack.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; internal set; } = default!;

    /// <summary>
    /// Gets the Id of the sticker pack's banner image.
    /// </summary>
    [JsonProperty("banner_asset_id")]
    public ulong BannerAssetId { get; internal set; }

    internal DiscordMessageStickerPack() { }
}
