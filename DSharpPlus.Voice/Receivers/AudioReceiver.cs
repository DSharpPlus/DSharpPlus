using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.Receivers;

/// <summary>
/// Represents a mechanism for receiving audio from DSharpPlus.Voice.
/// </summary>
// abstract class so that we can make the library-called methods protected internal, and thus not callable from arbitrary user code
public abstract class AudioReceiver
{
    /// <summary>
    /// Called by the library on connecting or reconnecting to notify the receiver of which users are already present.
    /// </summary>
    protected internal abstract Task Initialize(IReadOnlyList<ulong> joinedUsers);

    /// <summary>
    /// Called by the library when a new user joined the call.
    /// </summary>
    protected internal abstract Task ProcessUserJoinedAsync(ulong id);

    /// <summary>
    /// Called by the library when a user left the call.
    /// </summary>
    protected internal abstract Task ProcessUserLeftAsync(ulong id);

    /// <summary>
    /// Called by the library when a user started speaking and we may now receive audio from them.
    /// </summary>
    protected internal abstract Task ProcessUserStartedSpeakingAsync(ulong id);

    /// <summary>
    /// Called by the library when a user stopped speaking and we should no longer expect audio from them.
    /// </summary>
    protected internal abstract Task ProcessUserStoppedSpeakingAsync(ulong id);

    /// <summary>
    /// Called by the library when audio is received from a user.
    /// </summary>
    /// <param name="speakingUserId">The snowflake identifier of the speaking user.</param>
    /// <param name="sequence">The sequence number of this section of audio.</param>
    /// <param name="timestamp">The RTP timestamp of this section of audio.</param>
    /// <param name="duration">The duration of this section of audio.</param>
    /// <param name="audio">The audio data received.</param>
    protected internal abstract Task ProcessAudioAsync(ulong speakingUserId, uint sequence, AudioTimestamp timestamp, TimeSpan duration, byte[] audio);
}
