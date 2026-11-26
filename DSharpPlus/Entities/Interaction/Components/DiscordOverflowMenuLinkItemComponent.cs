using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an overflow menu item linking to a specified URL rather than executing a component interaction.
/// </summary>
public sealed class DiscordOverflowMenuLinkItemComponent : DiscordOverflowMenuItemComponent
{
    /// <summary>
    /// The url this link item is associated with.
    /// </summary>
    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public string Url { get; internal set; }

    internal DiscordOverflowMenuLinkItemComponent()
        : base()
        => this.Style = DiscordOverflowMenuItemStyle.Link;

    /// <summary>
    /// Creates a new link item.
    /// </summary>
    /// <param name="url">The url this item points to.</param>
    /// <param name="label">The label of this item.</param>
    /// <param name="description">An optional short description of this item.</param>
    /// <param name="disabled">Indicates whether this item is disabled. Defaults to false.</param>
    /// <param name="emoji">An optional emoji for this item.</param>
    public DiscordOverflowMenuLinkItemComponent(string url, string label, string? description = null, bool disabled = false, DiscordComponentEmoji? emoji = null)
        : this()
    {
        this.Url = url;
        this.Label = label;
        this.Description = description;
        this.Disabled = disabled;
        this.Emoji = emoji;
    }
}
