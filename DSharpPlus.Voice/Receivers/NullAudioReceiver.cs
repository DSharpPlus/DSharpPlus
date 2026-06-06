using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.Receivers;

/// <summary>
/// Provides an audio receiver that does absolutely nothing with the received audio.
/// </summary>
public sealed class NullAudioReceiver : AudioReceiver
{
    /// <inheritdoc/>
    protected internal override Task Initialize(IReadOnlyList<ulong> joinedUsers) 
        => Task.CompletedTask;

    /// <inheritdoc/>
    protected internal override Task ProcessAudioAsync(ulong speakingUserId, uint sequence, AudioTimestamp timestamp, TimeSpan duration, byte[] audio)
        => Task.CompletedTask;

    /// <inheritdoc/>
    protected internal override Task ProcessUserJoinedAsync(ulong id)
        => Task.CompletedTask;

    /// <inheritdoc/>
    protected internal override Task ProcessUserLeftAsync(ulong id)
        => Task.CompletedTask;

    /// <inheritdoc/>
    protected internal override Task ProcessUserStartedSpeakingAsync(ulong id)
        => Task.CompletedTask;

    /// <inheritdoc/>
    protected internal override Task ProcessUserStoppedSpeakingAsync(ulong id)
        => Task.CompletedTask;
}
