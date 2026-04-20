using System;

using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Provides the time at which a voice channel session started.
/// </summary>
public sealed class VoiceChannelStartTimeUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The voice channel whose start time was updated.
    /// </summary>
    public DiscordChannel Channel { get; internal set; }

    /// <summary>
    /// The containing guild.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// The updated voice channel start time. Null if the session ended.
    /// </summary>
    public DateTimeOffset? StartTime { get; internal set; }
}
