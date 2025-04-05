using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an unfurled url; can be arbitrary URL or attachment:// schema. 
/// </summary>
public sealed class DiscordUnfurledMediaItem
{
    /// <summary>
    /// Gets the URL of the media item.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; internal set; }

    public DiscordUnfurledMediaItem(string url) 
        => this.Url = url;
}
