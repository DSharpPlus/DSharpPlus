using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.Receivers;

/// <summary>
/// An audio receiver that passes 
/// </summary>
public sealed class DefaultAudioReceiver : IAudioReceiver
{
    public Task Initialize(IReadOnlyList<ulong> joinedUsers) => throw new NotImplementedException();
    public Task ProcessAudioAsync(ulong speakingUserId, uint sequence, AudioTimestamp timestamp, TimeSpan duration, ReadOnlyMemory<byte> audio) => throw new NotImplementedException();
    public Task ProcessUserJoinedAsync(ulong id) => throw new NotImplementedException();
    public Task ProcessUserLeftAsync(ulong id) => throw new NotImplementedException();
    public Task ProcessUserStartedSpeakingAsync(ulong id) => throw new NotImplementedException();
    public Task ProcessUserStoppedSpeakingAsync(ulong id) => throw new NotImplementedException();
}
