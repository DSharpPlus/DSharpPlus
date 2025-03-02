using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an item in a media gallery.
/// </summary>
public sealed class DiscordMediaGalleryItem
{
    /// <summary>
    /// Gets the media item in the gallery.
    /// </summary>
    public DiscordUnfurledMediaItem Media { get; internal set; }

    /// <summary>
    /// Gets the description (alt text) of the media item.
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Gets whether the media item is spoilered.
    /// </summary>
    [JsonProperty("spoilered")]
    public bool IsSpoilered { get; internal set; }

    /// <summary>
    /// Constructs a new media gallery item.
    /// </summary>
    /// <param name="url">The URL of the media item. This must be a direct link to media, or an attachment:// reference.</param>
    /// <param name="description">The description (alt text) of the media item.</param>
    /// <param name="isSpoilered">Whether the attachment is spoilered.</param>
    public DiscordMediaGalleryItem(string url, string description, bool isSpoilered)
    {
        this.Media = new(url);
        this.Description = description;
        this.IsSpoilered = isSpoilered;
    }
}
