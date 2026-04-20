using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Provides the updated voice channel status.
/// </summary>
public sealed class VoiceChannelStatusUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The voice channel whose status was updated.
    /// </summary>
    public DiscordChannel Channel { get; internal set; }

    /// <summary>
    /// The containing guild.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// The updated voice channel status. Null if the status was cleared.
    /// </summary>
    public string? Status { get; internal set; }
}
