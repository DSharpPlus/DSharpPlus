using System;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an item in an overflow menu.
/// </summary>
public class DiscordOverflowMenuItemComponent : DiscordComponent
{
    /// <summary>
    /// The label of this menu item.
    /// </summary>
    [JsonProperty("label")]
    public string Label { get; internal set; }

    /// <summary>
    /// The visual style of this menu item.
    /// </summary>
    [JsonProperty("style")]
    public DiscordOverflowMenuItemStyle Style { get; internal set; }

    /// <summary>
    /// An optional short description for this item.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; internal set; }

    /// <summary>
    /// The component emoji to show with this item.
    /// </summary>
    [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordComponentEmoji? Emoji { get; internal set; }

    /// <summary>
    /// Indicates whether this menu item is disabled.
    /// </summary>
    [JsonProperty("disabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool Disabled { get; internal set; }

    internal DiscordOverflowMenuItemComponent()
        => this.Type = DiscordComponentType.OverflowMenuItem;

    /// <summary>
    /// Creates a new overflow menu item.
    /// </summary>
    /// <param name="customId">The custom ID of this item.</param>
    /// <param name="label">The label of this item.</param>
    /// <param name="style">The style this item will be shown in.</param>
    /// <param name="description">An optional short description of this item.</param>
    /// <param name="emoji">An optional emoji for this item.</param>
    /// <param name="disabled">Indicates whether this item is disabled. Defaults to false.</param>
    /// <param name="id">A numeric ID for this item.</param>
    public DiscordOverflowMenuItemComponent
    (
        string customId,
        string label, 
        DiscordOverflowMenuItemStyle style, 
        string? description = null, 
        DiscordComponentEmoji? emoji = null, 
        bool disabled = false,
        int id = 0
    )
        : this()
    {
        if (style == DiscordOverflowMenuItemStyle.Link)
        {
            throw new ArgumentException("Use DiscordOverflowMenuLinkItemComponent for link items.");
        }

        this.CustomId = customId;
        this.Label = label;
        this.Style = style;
        this.Description = description;
        this.Emoji = emoji;
        this.Disabled = disabled;
        this.Id = id;
    }
}
