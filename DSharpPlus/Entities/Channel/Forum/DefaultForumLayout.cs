namespace DSharpPlus.Entities;

/// <summary>
/// The layout type for forum channels.
/// </summary>
public enum DefaultForumLayout
{
    /// <summary>
    /// The channel doesn't have a set layout.
    /// </summary>
    Unset,
    /// <summary>
    /// Posts will be displayed in a list format.
    /// </summary>
    ListView,

    /// <summary>
    /// Posts will be displayed in a grid format that prioritizes image previews over the forum's content.
    /// </summary>
    GalleryView
}
