namespace DSharpPlus.Entities;


/// <summary>
/// Represents a button's style/color.
/// </summary>
/// <remarks>
/// Link buttons are represented by <see cref="DiscordLinkButtonComponent"/>. Premium buttons are not implemented yet.
/// </remarks>
public enum DiscordButtonStyle : int
{
    /// <summary>
    /// Blurple button.
    /// </summary>
    Primary = 1,

    /// <summary>
    /// Grey button.
    /// </summary>
    Secondary = 2,

    /// <summary>
    /// Green button.
    /// </summary>
    Success = 3,

    /// <summary>
    /// Red button.
    /// </summary>
    Danger = 4,
}
