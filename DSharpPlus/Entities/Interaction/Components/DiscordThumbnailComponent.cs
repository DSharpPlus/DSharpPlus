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
    [JsonProperty("spoilered", NullValueHandling = NullValueHandling.Ignore)]
    public bool Spoiler { get; internal set; }

    public DiscordThumbnailComponent(DiscordUnfurledMediaItem media, string? description = null, bool spoiler = false)
    {
        Media = media;
        Description = description;
        Spoiler = spoiler;
    }

    internal DiscordThumbnailComponent() => this.Type = DiscordComponentType.Thumbnail;

}
