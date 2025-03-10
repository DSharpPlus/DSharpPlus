using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a thumbnail.
/// </summary>
public class DiscordThumbnailComponent : DiscordComponent
{
    /// <summary>
    /// The image for this thumbnail.
    /// </summary>
    [JsonProperty("media", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUnfurledMediaItem Media { get; internal set; }
    
    /// <summary>
    /// Gets the description (alt-text) for this thumbnail.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; internal set; }
    
    /// <summary>
    /// Gets whether this thumbnail is spoilered.
    /// </summary>
    [JsonProperty("spoiler", NullValueHandling = NullValueHandling.Ignore)]
    public bool Spoiler { get; internal set; }

    public DiscordThumbnailComponent(string url, string? description = null, bool spoiler = false)
        : this(new DiscordUnfurledMediaItem(url), description, spoiler)
    {

    }

    public DiscordThumbnailComponent(DiscordUnfurledMediaItem media, string? description = null, bool spoiler = false)
        : this()
    {
        this.Media = media;
        this.Description = description;
        this.Spoiler = spoiler;
    }

    internal DiscordThumbnailComponent() => this.Type = DiscordComponentType.Thumbnail;

}
