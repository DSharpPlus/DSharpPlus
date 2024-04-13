namespace DSharpPlus.Entities;

/// <summary>
/// Represents the video quality mode of a voice channel. This is applicable to voice channels only.
/// </summary>
public enum DiscordVideoQualityMode : int
{
    /// <summary>
    /// Indicates that the video quality is automatically chosen, or there is no value set.
    /// </summary>
    Auto = 1,

    /// <summary>
    /// Indicates that the video quality is 720p.
    /// </summary>
    Full = 2,
}
