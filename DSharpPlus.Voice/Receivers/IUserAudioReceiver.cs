using System.ComponentModel;
using System.IO.Pipelines;

namespace DSharpPlus.Voice.Receivers;

/// <summary>
/// Represents a mechanism to receive PCM audio from a specific user.
/// </summary>
public interface IUserAudioReceiver
{
    /// <summary>
    /// Provides the audio received from this user.
    /// </summary>
    public PipeReader Reader { get; }
    
    /// <summary>
    /// Indicates whether this user is currently speaking.
    /// </summary>
    public bool IsSpeaking { get; }
    
    /// <summary>
    /// Ingests a frame of opus audio received to make it available to <see cref="Reader"/>. Do not call this manually.
    /// </summary>
    /// <param name="sequence">The sequence number of this frame.</param>
    /// <param name="timestamp">The timestamp of this frame.</param>
    /// <param name="audio">The audio frame data.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Ingest(ushort sequence, uint timestamp, byte[] audio);

    /// <summary>
    /// Called by the library once the user disconnects. Do not call this manually.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Close();
}
