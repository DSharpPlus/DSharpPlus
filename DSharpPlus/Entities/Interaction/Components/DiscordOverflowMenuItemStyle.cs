namespace DSharpPlus.Entities;

/// <summary>
/// Represents the style of an overflow menu item
/// </summary>
public enum DiscordOverflowMenuItemStyle
{
    /// <summary>
    /// The default style.
    /// </summary>
    Default = 1,

    /// <summary>
    /// A style indicating that this action could be dangerous.
    /// </summary>
    Danger = 2,

    /// <summary>
    /// An item linking to an external URL.
    /// </summary>
    Link = 3
}
