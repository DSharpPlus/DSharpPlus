using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalGuildPreview
{
    /// <summary>
    /// The guild id.
    /// </summary>
    [JsonPropertyName("id")]
    public required Snowflake Id { get; init; }

    /// <summary>
    /// The guild name (2-100 characters).
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The <see href="https://discord.com/developers/docs/reference#image-formatting">icon hash</see>.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    /// <summary>
    /// The <see href="https://discord.com/developers/docs/reference#image-formatting">splash hash</see>.
    /// </summary>
    [JsonPropertyName("splash")]
    public string? Splash { get; init; }

    /// <summary>
    /// The <see href="https://discord.com/developers/docs/reference#image-formatting">discovery splash hash</see>.
    /// </summary>
    [JsonPropertyName("discovery_splash")]
    public string? DiscoverySplash { get; init; }

    /// <summary>
    /// The guild's custom emojis.
    /// </summary>
    [JsonPropertyName("emojis")]
    public required IReadOnlyList<InternalEmoji> Emojis { get; init; } 

    /// <summary>
    /// The enabled guild features.
    /// </summary>
    /// <remarks>
    /// See <see cref="InternalGuildFeature"/> for more information.
    /// </remarks>
    [JsonPropertyName("features")]
    public required IReadOnlyList<string> Features { get; init; } 

    /// <summary>
    /// The approximate number of members in this guild.
    /// </summary>
    [JsonPropertyName("approximate_member_count")]
    public required int ApproximateMemberCount { get; init; }

    /// <summary>
    /// The approximate number of online members in this guild.
    /// </summary>
    [JsonPropertyName("approximate_presence_count")]
    public required int ApproximatePresenceCount { get; init; }

    /// <summary>
    /// The description for the guild.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The guild's custom stickers.
    /// </summary>
    [JsonPropertyName("stickers")]
    public required IReadOnlyList<InternalSticker> Stickers { get; init; } 
}
