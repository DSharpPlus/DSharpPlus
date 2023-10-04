using DSharpPlus.AsyncEvents;

namespace DSharpPlus.Lavalink.EventArgs;

internal enum EventType
{
    TrackStartEvent,
    TrackEndEvent,
    TrackExceptionEvent,
    TrackStuckEvent,
    WebSocketClosedEvent
}

/// <summary>
/// Represents arguments for a track playback start event.
/// </summary>
public sealed class TrackStartEventArgs : AsyncEventArgs
{
    /// <summary>
    /// Gets the track that started playing.
    /// </summary>
    public LavalinkTrack Track { get; }

    /// <summary>
    /// Gets the player that started playback.
    /// </summary>
    public LavalinkGuildConnection Player { get; }

    internal TrackStartEventArgs(LavalinkGuildConnection lvl, LavalinkTrack track)
    {
        this.Track = track;
        this.Player = lvl;
    }
}

internal struct TrackFinishData
{
    public string Track { get; set; }
    public TrackEndReason Reason { get; set; }
}

/// <summary>
/// Represents arguments for a track playback finish event.
/// </summary>
public sealed class TrackFinishEventArgs : AsyncEventArgs
{
    /// <summary>
    /// Gets the track that finished playing.
    /// </summary>
    public LavalinkTrack Track { get; }

    /// <summary>
    /// Gets the reason why the track stopped playing.
    /// </summary>
    public TrackEndReason Reason { get; }

    /// <summary>
    /// Gets the player that finished playback.
    /// </summary>
    public LavalinkGuildConnection Player { get; }

    internal TrackFinishEventArgs(LavalinkGuildConnection lvl, LavalinkTrack track, TrackEndReason reason)
    {
        this.Track = track;
        this.Reason = reason;
        this.Player = lvl;
    }
}

/// <summary>
/// Represents a reason why a track finished playing.
/// </summary>
public enum TrackEndReason
{
    /// <summary>
    /// This means that the track itself emitted a terminator. This is usually caused by the track reaching the end,
    /// however it will also be used when it ends due to an exception.
    /// </summary>
    Finished,
    /// <summary>
    /// This means that the track failed to start, throwing an exception before providing any audio.
    /// </summary>
    LoadFailed,
    /// <summary>
    /// The track was stopped due to the player being stopped by either calling stop() or playTrack(null).
    /// </summary>
    Stopped,
    /// <summary>
    /// The track stopped playing because a new track started playing. Note that with this reason, the old track will still
    /// play until either its buffer runs out or audio from the new track is available.
    /// </summary>
    Replaced,
    /// <summary>
    /// The track was stopped because the cleanup threshold for the audio player was reached. This triggers when the amount
    /// of time passed since the last call to AudioPlayer#provide() has reached the threshold specified in player manager
    /// configuration. This may also indicate either a leaked audio player which was discarded, but not stopped.
    /// </summary>
    Cleanup
}

internal struct TrackStuckData
{
    public long Threshold { get; set; }
    public string Track { get; set; }
}

/// <summary>
/// Represents event arguments for a track stuck event.
/// </summary>
public sealed class TrackStuckEventArgs : AsyncEventArgs
{
    /// <summary>
    /// Gets the millisecond threshold for the stuck event.
    /// </summary>
    public long ThresholdMilliseconds { get; }

    /// <summary>
    /// Gets the track that got stuck.
    /// </summary>
    public LavalinkTrack Track { get; }

    /// <summary>
    /// Gets the player that got stuck.
    /// </summary>
    public LavalinkGuildConnection Player { get; }

    internal TrackStuckEventArgs(LavalinkGuildConnection lvl, long thresholdMs, LavalinkTrack track)
    {
        this.ThresholdMilliseconds = thresholdMs;
        this.Track = track;
        this.Player = lvl;
    }
}

internal struct TrackExceptionData
{
    public string Error { get; set; }
    public string Track { get; set; }
}

/// <summary>
/// Represents event arguments for a track exception event.
/// </summary>
public sealed class TrackExceptionEventArgs : AsyncEventArgs
{
    /// <summary>
    /// Gets the error that occurred during playback.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Gets the track that got stuck.
    /// </summary>
    public LavalinkTrack Track { get; }

    /// <summary>
    /// Gets the player that got stuck.
    /// </summary>
    public LavalinkGuildConnection Player { get; }

    internal TrackExceptionEventArgs(LavalinkGuildConnection lvl, string error, LavalinkTrack track)
    {
        this.Error = error;
        this.Track = track;
        this.Player = lvl;
    }
}
