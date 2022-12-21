namespace DSharpPlus.Entities;

/// <summary>
/// The camera video quality mode of the voice channel, <see cref="Auto"/> when not present
/// </summary>
public enum DiscordChannelVideoQualityMode
{
    /// <summary>
    /// Discord chooses the quality for optimal performance.
    /// </summary>
    Auto = 1,

    /// <summary>
    /// 720p.
    /// </summary>
    Full = 2
}
