using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// A container for other components. 
/// </summary>
public class DiscordContainerComponent : DiscordComponent
{
        
    /// <summary>
    /// The accent color for this container, similar to an embed.
    /// </summary>
    public DiscordColor? Color
    {
        get
        {
            return this.color.IsDefined(out int? colorValue)
                ? (DiscordColor)colorValue
                : null;
        }
    }

    [JsonProperty("accent_color", NullValueHandling = NullValueHandling.Include)]
    internal Optional<int?> color;

    /// <summary>
    /// Gets whether this container is spoilered.
    /// </summary>
    [JsonProperty("spoiler", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsSpoilered { get; internal set; }
    
    /// <summary>
    /// Gets the components of this container.
    /// </summary>
    /// <remarks>
    /// At this time, only the following components are allowed:
    /// <list type="bullet">
    /// <see cref="DiscordActionRowComponent"/>
    /// <see cref="DiscordTextDisplayComponent"/>
    /// <see cref="DiscordMediaGalleryComponent"/>
    /// <see cref="DiscordSeparatorComponent"/>
    /// <see cref="DiscordFileComponent"/>
    /// <see cref="DiscordSectionComponent"/>
    /// </list>
    /// </remarks>
    [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordComponent> Components { get; internal set; }

    public DiscordContainerComponent
    (
        IReadOnlyList<DiscordComponent> components,
        bool isSpoilered = false,
        DiscordColor? color = null
    )
        : this()
    {
        this.Components = components;
        this.IsSpoilered = isSpoilered;
        this.color = color?.Value;
        
        ThrowIfUnwrappedComponentsDetected();
    }
    
    internal DiscordContainerComponent() => this.Type = DiscordComponentType.Container;

    [StackTraceHidden]
    [DebuggerStepThrough]
    private void ThrowIfUnwrappedComponentsDetected()
    {
        for (int i = 0; i < this.Components.Count; i++)
        {
            DiscordComponent comp = this.Components[i];

            if (comp is not (DiscordButtonComponent or DiscordSelectComponent))
            {
                continue;
            }

            string compType = comp is DiscordButtonComponent ? "Buttons" : "Selects";
            throw new ArgumentException($"{compType} must be wrapped in an action row. Index: {i}");
        }
    }
}
