using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a gallery of various media.
/// </summary>
public sealed class DiscordMediaGalleryComponent : DiscordComponent
{
        
    /// <summary>
    /// Gets the items in the gallery.
    /// </summary>
    [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordMediaGalleryItem> Items { get; internal set; }

    /// <inheritdoc cref="DiscordMediaGalleryComponent(IEnumerable{DiscordMediaGalleryItem}, int)"/>
    public DiscordMediaGalleryComponent(params IEnumerable<DiscordMediaGalleryItem> items)
        : this(items, 0)
    {

    }

    /// <summary>
    /// Constructs a new media gallery component.
    /// </summary>
    /// <param name="items">The items of the gallery.</param>
    /// <param name="id">The optional ID of the component.</param>
    public DiscordMediaGalleryComponent(IEnumerable<DiscordMediaGalleryItem> items, int id = 0)
        : this()
    {
        this.Items = items.ToArray();
        this.Id = id;
    }
    
    internal DiscordMediaGalleryComponent() => this.Type = DiscordComponentType.MediaGallery;
}
