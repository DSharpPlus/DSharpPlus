namespace DSharpPlus.Entities;

/// <summary>
/// In order to use this, <see cref="DiscordButtonComponent.CustomId"/> must be set.
/// </summary>
public enum DiscordButtonStyle
{
    /// <summary>
    /// Blurple.
    /// </summary>
    Primary = 1,

    /// <summary>
    /// Grey.
    /// </summary>
    Secondary = 2,

    /// <summary>
    /// Green.
    /// </summary>
    Success = 3,

    /// <summary>
    /// Red.
    /// </summary>
    Danger = 4,

    /// <summary>
    /// Grey, navigates to a URL.
    /// </summary>
    Link = 5
}
