using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public class DiscordSectionComponent : DiscordComponent
{
    public new DiscordComponentType Type => DiscordComponentType.Section;

    /// <summary>
    /// Gets the accessory for this section.
    ///
    /// Accessories take the place of a thumbnail (that is, are positioned as a thumbnail would be) regardless of component.
    /// At this time, only <see cref="DiscordButtonComponent"/> and <see cref="DiscordThumbnailComponent"/> are supported.
    /// </summary>
    [JsonProperty("accessory", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordComponent? Accessory { get; internal set; }
    
    /// <summary>
    /// Gets the components for this section.
    /// As of now, this is only text components, but may allow for more components in the future.
    /// </summary>
    /// <remarks>This is a Discord limitation.</remarks>
    // To the chagrin of Dv8, we're hard-coding this to be text components
    // Because I honestly have no idea what he expects us to do otherwise;
    // runtime validation has the same issue (unchangeable without updating) but worse (needs to be validated everywhere).
    [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordTextDisplayComponent> Components { get; internal set; }
    
    /// <summary>
    /// Constructs a new section component.
    /// </summary>
    /// <param name="sections">The sections (generally text) that this section contains.</param>
    /// <param name="accessory">The accessory to this section. At this time, this must either be a <see cref="DiscordThumbnailComponent"/> or a <see cref="DiscordButtonComponent"/>.</param>
    public DiscordSectionComponent(IEnumerable<DiscordTextDisplayComponent> sections, DiscordComponent accessory)
    {
        this.Accessory = accessory;
        this.Components = sections.ToArray();
    }
    
    internal DiscordSectionComponent() => this.Type = DiscordComponentType.Section;

}
