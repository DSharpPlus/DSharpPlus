namespace DSharpPlus.Lavalink.EventArgs;

using System;
using DSharpPlus.AsyncEvents;

/// <summary>
/// Represents arguments for player update event.
/// </summary>
[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
public sealed class PlayerUpdateEventArgs : AsyncEventArgs
{
    /// <summary>
    /// Gets the timestamp at which this event was emitted.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the position in the playback stream.
    /// </summary>
    public TimeSpan Position { get; }

    /// <summary>
    /// Gets the player that emitted this event.
    /// </summary>
    public LavalinkGuildConnection Player { get; }

    internal PlayerUpdateEventArgs(LavalinkGuildConnection lvl, DateTimeOffset timestamp, TimeSpan position)
    {
        Player = lvl;
        Timestamp = timestamp;
        Position = position;
    }
}
