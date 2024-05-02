
using DSharpPlus.Net;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;
/// <summary>
/// Represents an image in an embed.
/// </summary>
public sealed class DiscordEmbedImage
{
    /// <summary>
    /// Gets the source url of the image.
    /// </summary>
    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUri? Url { get; internal set; }

    /// <summary>
    /// Gets a proxied url of the image.
    /// </summary>
    [JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUri? ProxyUrl { get; internal set; }

    /// <summary>
    /// Gets the height of the image.
    /// </summary>
    [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
    public int Height { get; internal set; }

    /// <summary>
    /// Gets the width of the image.
    /// </summary>
    [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
    public int Width { get; internal set; }

    internal DiscordEmbedImage() { }
}
