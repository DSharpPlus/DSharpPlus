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
    public DiscordGuild Guild => (Discord as DiscordClient)!.InternalGetCachedGuild(GuildId);

    public string StickerUrl => $"https://cdn.discordapp.com/stickers/{Id}{(FormatType is DiscordStickerFormat.LOTTIE ? ".json" : ".png")}";

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
        => InternalTags != null ? InternalTags.Split(',') : [];

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

    public string BannerUrl => $"https://cdn.discordapp.com/app-assets/710982414301790216/store/{BannerAssetId}.png?size=4096";

    [JsonProperty("banner_asset_id")]
    internal ulong BannerAssetId { get; set; }

    public bool Equals(DiscordMessageSticker? other) => Id == other?.Id;
    public override bool Equals(object obj) => Equals(obj as DiscordMessageSticker);
    public override string ToString() => $"Sticker {Id}; {Name}; {FormatType}";
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Id);
        hash.Add(CreationTimestamp);
        hash.Add(Discord);
        hash.Add(PackId);
        hash.Add(Name);
        hash.Add(Description);
        hash.Add(Type);
        hash.Add(User);
        hash.Add(Guild);
        hash.Add(StickerUrl);
        hash.Add(GuildId);
        hash.Add(Available);
        hash.Add(SortValue);
        hash.Add(Tags);
        hash.Add(Asset);
        hash.Add(PreviewAsset);
        hash.Add(FormatType);
        hash.Add(InternalTags);
        hash.Add(BannerUrl);
        hash.Add(BannerAssetId);
        return hash.ToHashCode();
    }
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
