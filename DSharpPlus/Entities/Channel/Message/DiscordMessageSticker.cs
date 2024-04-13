using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord Sticker.
/// </summary>
public class DiscordMessageSticker : SnowflakeObject, IEquatable<DiscordMessageSticker>
{
    /// <summary>
    /// Gets the Pack ID of this sticker.
    /// </summary>
    [JsonProperty("pack_id")]
    public ulong PackId { get; internal set; }

    /// <summary>
    /// Gets the Name of the sticker.
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; internal set; }

    /// <summary>
    /// Gets the Description of the sticker.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; internal set; }

    /// <summary>
    /// Gets the type of sticker.
    /// </summary>
    [JsonProperty("type")]
    public DiscordStickerType Type { get; internal set; }

    /// <summary>
    /// For guild stickers, gets the user that made the sticker.
    /// </summary>
    [JsonProperty("user")]
    public DiscordUser? User { get; internal set; }

    /// <summary>
    /// Gets the guild associated with this sticker, if any.
    /// </summary>
    public DiscordGuild Guild => (this.Discord as DiscordClient)!.InternalGetCachedGuild(this.GuildId);

    public string StickerUrl => $"https://cdn.discordapp.com/stickers/{this.Id}{this.GetFileTypeExtension()}";

    /// <summary>
    /// Gets the Id of the sticker this guild belongs to, if any.
    /// </summary>
    [JsonProperty("guild_id")]
    public ulong? GuildId { get; internal set; }

    /// <summary>
    /// Gets whether this sticker is available. Only applicable to guild stickers.
    /// </summary>
    [JsonProperty("available")]
    public bool Available { get; internal set; }

    /// <summary>
    /// Gets the sticker's sort order, if it's in a pack.
    /// </summary>
    [JsonProperty("sort_value")]
    public int SortValue { get; internal set; }

    /// <summary>
    /// Gets the list of tags for the sticker.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<string> Tags
        => this.InternalTags != null ? this.InternalTags.Split(',') : [];

    /// <summary>
    /// Gets the asset hash of the sticker.
    /// </summary>
    [JsonProperty("asset")]
    public string? Asset { get; internal set; }

    /// <summary>
    /// Gets the preview asset hash of the sticker.
    /// </summary>
    [JsonProperty("preview_asset", NullValueHandling = NullValueHandling.Ignore)]
    public string? PreviewAsset { get; internal set; }

    /// <summary>
    /// Gets the Format type of the sticker.
    /// </summary>
    [JsonProperty("format_type")]
    public DiscordStickerFormat FormatType { get; internal set; }

    [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
    internal string? InternalTags { get; set; }

    public string BannerUrl => $"https://cdn.discordapp.com/app-assets/710982414301790216/store/{this.BannerAssetId}.png?size=4096";

    [JsonProperty("banner_asset_id")]
    internal ulong BannerAssetId { get; set; }

    public bool Equals(DiscordMessageSticker? other) => this.Id == other?.Id;

    public override string ToString() => $"Sticker {this.Id}; {this.Name}; {this.FormatType}";

    private string GetFileTypeExtension() => this.FormatType switch
    {
        DiscordStickerFormat.PNG or DiscordStickerFormat.APNG => ".png",
        DiscordStickerFormat.LOTTIE => ".json",
        _ => ".png"
    };
}

public enum DiscordStickerType
{
    Standard = 1,
    Guild = 2
}

public enum DiscordStickerFormat
{
    PNG = 1,
    APNG = 2,
    LOTTIE = 3
}
