using System.Collections.Generic;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// A section for components (as of now, just text) and an accessory on the side.
/// </summary>
public class DiscordSectionComponent : DiscordComponent
{
    
    /// <summary>
    /// Gets the accessory for this section.
    ///</summary>
    /// <remarks>
    /// Accessories take the place of a thumbnail (that is, are positioned as a thumbnail would be) regardless of component.
    /// At this time, only <see cref="DiscordButtonComponent"/> and <see cref="DiscordThumbnailComponent"/> are supported.
    /// </remarks>
    [JsonProperty("accessory", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordComponent? Accessory { get; internal set; }
    
    /// <summary>
    /// Gets the components for this section.
    /// As of now, this is only text components, but may allow for more components in the future.
    /// </summary>
    /// <remarks>This is a Discord limitation.</remarks>
    [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordComponent> Components { get; internal set; }

    /// <inheritdoc cref="DiscordSectionComponent(IReadOnlyList{DiscordComponent}, DiscordComponent)"/>
    public DiscordSectionComponent(DiscordComponent textDisplayComponent, DiscordComponent accessory)
        : this([textDisplayComponent], accessory)
    {
    }

    /// <summary>
    /// Constructs a new section component.
    /// </summary>
    /// <param name="sections">The sections (generally text) that this section contains.</param>
    /// <param name="accessory"> The accessory to this section.
    /// At this time, this must either be a <see cref="DiscordThumbnailComponent"/> or a <see cref="DiscordButtonComponent"/>.</param>
    public DiscordSectionComponent(IReadOnlyList<DiscordComponent> sections, DiscordComponent accessory)
        : this()
    {
        this.Accessory = accessory;
        this.Components = sections;
    }
    
    internal DiscordSectionComponent() => this.Type = DiscordComponentType.Section;
}
