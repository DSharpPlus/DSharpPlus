using DSharpPlus.Net;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a thumbnail in an embed.
/// </summary>
public sealed class DiscordEmbedThumbnail
{
    /// <summary>
    /// Gets the source url of the thumbnail (only https).
    /// </summary>
    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUri Url { get; internal set; }

    /// <summary>
    /// Gets a proxied url of the thumbnail.
    /// </summary>
    [JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUri ProxyUrl { get; internal set; }

    /// <summary>
    /// Gets the height of the thumbnail.
    /// </summary>
    [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
    public int Height { get; internal set; }

    /// <summary>
    /// Gets the width of the thumbnail.
    /// </summary>
    [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
    public int Width { get; internal set; }

    internal DiscordEmbedThumbnail() { }
}
