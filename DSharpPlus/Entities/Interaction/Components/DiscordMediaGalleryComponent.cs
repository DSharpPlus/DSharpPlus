using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.Entities.Interaction.Components;

/// <summary>
/// Represents a gallery of various media.
/// </summary>
public sealed class DiscordMediaGalleryComponent
{
    /// <summary>
    /// Gets the items in the gallery.
    /// </summary>
    public IReadOnlyList<DiscordMediaGalleryItem> Items { get; internal set; }

    /// <summary>
    /// Constructs a new media gallery component.
    /// </summary>
    /// <param name="items">The items of the gallery.</param>
    /// <param name="id">The optional ID of the component.</param>
    public DiscordMediaGalleryComponent(IEnumerable<DiscordMediaGalleryItem> items, uint id=0)
    {
        this.Items = items.ToArray();
        this.Id = id;
    }
}
