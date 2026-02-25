using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.Receivers;

namespace DSharpPlus.Voice;

/// <summary>
/// Provides all incoming audio data from a connection.
/// </summary>
public sealed class AudioReceiver
{
    private readonly ConcurrentDictionary<VoiceUser, IUserAudioReceiver> receivers = [];
    private readonly ConcurrentDictionary<int, VoiceUser> ssrcs = [];
    private readonly IAudioCodec codec;

    /// <summary>
    /// The default receive mode to apply to newly joining users. Defaults to ignoring their audio.
    /// </summary>
    public Func<VoiceUser, AudioReceiveMode> DefaultReceiveMode { get; set; }

    /// <summary>
    /// Provides the receivers for each user in the voice chat.
    /// </summary>
    public IReadOnlyDictionary<VoiceUser, IUserAudioReceiver> Receivers => this.receivers;

    internal AudioReceiver(IAudioCodec codec, AudioReceiveMode defaultReceiveMode)
    {
        this.codec = codec;
        this.DefaultReceiveMode = _ => defaultReceiveMode;
    }

    internal void IngestAudio(int ssrc, uint timestamp, ushort sequence, byte[] audio)
    {
        if (!this.ssrcs.TryGetValue(ssrc, out VoiceUser? user))
        {
            // race condition: we're still waiting on their ssrc but they already started speaking
            // [TODO] consider buffering such received packets?
            return;
        }

        if (!this.receivers.TryGetValue(user, out IUserAudioReceiver? receiver))
        {
            // same race condition as above
            return;
        }

        receiver.Ingest(sequence, timestamp, audio);
    }

    internal void IntroduceNewUser(VoiceUser user)
    {
        AudioReceiveMode desiredReceiveMode = this.DefaultReceiveMode(user);

        IUserAudioReceiver receiver = desiredReceiveMode switch
        {
            AudioReceiveMode.Discard => new NullAudioReceiver(),
            AudioReceiveMode.Process => new RealtimeAudioReceiver(this.codec.CreateDecoder()),
            _ => new RealtimeAudioReceiver(this.codec.CreateDecoder())
        };

        this.ssrcs.AddOrUpdate(user.Ssrc.Value, user, (_, _) => user);
        this.receivers.AddOrUpdate(user, receiver, (_, _) => receiver);
    }
}
