using System;
using System.IO.Pipelines;

namespace DSharpPlus.Voice.Receivers;

/// <summary>
/// Represents a mechanism to receive PCM audio from a specific user.
/// </summary>
public abstract class UserAudioReceiver
{
    /// <summary>
    /// Provides the audio received from this user.
    /// </summary>
    public abstract PipeReader Reader { get; }
    
    /// <summary>
    /// Indicates whether this user is currently speaking.
    /// </summary>
    public virtual bool IsSpeaking { get; protected internal set; }
    
    /// <summary>
    /// Ingests a frame of opus audio received to make it available to <see cref="Reader"/>.
    /// </summary>
    /// <param name="sequence">The sequence number of this frame.</param>
    /// <param name="timestamp">The timestamp of this frame, relative to when this user first started speaking.</param>
    /// <param name="duration">The duration of this frame.</param>
    /// <param name="audio">The audio frame data.</param>
    protected internal abstract void Ingest(uint sequence, AudioTimestamp timestamp, TimeSpan duration, byte[] audio);

    /// <summary>
    /// Called by the library once the user disconnects.
    /// </summary>
    protected internal abstract void Close();
}
