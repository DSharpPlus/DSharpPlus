using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.Receivers;

/// <summary>
/// Represents a mechanism for receiving audio from DSharpPlus.Voice.
/// </summary>
public interface IAudioReceiver
{
    /// <summary>
    /// Called by the library on connecting or reconnecting to notify the receiver of which users are already present.
    /// </summary>
    public Task Initialize(IReadOnlyList<ulong> joinedUsers);

    /// <summary>
    /// Called by the library when a new user joined the call.
    /// </summary>
    public Task ProcessUserJoinedAsync(ulong id);

    /// <summary>
    /// Called by the library when a user left the call.
    /// </summary>
    public Task ProcessUserLeftAsync(ulong id);

    /// <summary>
    /// Called by the library when a user started speaking and we may now receive audio from them.
    /// </summary>
    public Task ProcessUserStartedSpeakingAsync(ulong id);

    /// <summary>
    /// Called by the library when a user stopped speaking and we should no longer expect audio from them.
    /// </summary>
    public Task ProcessUserStoppedSpeakingAsync(ulong id);

    /// <summary>
    /// Called by the library when audio is received from a user.
    /// </summary>
    /// <param name="speakingUserId">The snowflake identifier of the speaking user.</param>
    /// <param name="sequence">The sequence number of this section of audio.</param>
    /// <param name="timestamp">The RTP timestamp of this section of audio.</param>
    /// <param name="duration">The duration of this section of audio.</param>
    /// <param name="audio">The audio data received.</param>
    public Task ProcessAudioAsync(ulong speakingUserId, uint sequence, AudioTimestamp timestamp, TimeSpan duration, ReadOnlyMemory<byte> audio);
}
