using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Voice.Codec;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice.Receivers;

/// <summary>
/// An audio receiver that passes opus data along to per-user receivers.
/// </summary>
public sealed class DefaultAudioReceiver : AudioReceiver
{
    private readonly ConcurrentDictionary<ulong, UserAudioReceiver> receivers = [];
    private readonly IAudioCodec codec;
    private readonly ILogger<AudioReceiver> logger;

    public DefaultAudioReceiver(IAudioCodec codec, ILogger<AudioReceiver> logger)
    {
        this.codec = codec;
        this.logger = logger;
    }

    /// <summary>
    /// Fired when a user started speaking.
    /// </summary>
    public event Func<ulong, Task> UserStartedSpeaking;

    /// <summary>
    /// Fired when a user stopped speaking.
    /// </summary>
    public event Func<ulong, Task> UserStoppedSpeaking;

    /// <summary>
    /// Fired when a user joined the call.
    /// </summary>
    public event Func<ulong, Task> UserJoined;

    /// <summary>
    /// Fired when a user left the call.
    /// </summary>
    public event Func<ulong, Task> UserLeft;

    /// <summary>
    /// Controls the default receive mode for newly speaking users. Defaults to discarding their audio. Use
    /// <see cref="UpdateUserReceiver"/> to update a specific user and <see cref="UpdateAllReceiverModes"/> to update all users. 
    /// </summary>
    public UserAudioReceiveMode DefaultReceiveMode { get; set; } = UserAudioReceiveMode.Discard;

    /// <summary>
    /// The currently active receivers.
    /// </summary>
    public IReadOnlyDictionary<ulong, UserAudioReceiver> Receivers => this.receivers;

    /// <summary>
    /// Updates the receive mode for a specific user.
    /// </summary>
    /// <param name="id">The ID of that user.</param>
    /// <param name="mode">The new receive mode to use for this user.</param>
    /// <returns>The old receiver with whatever stale data it may contain, and the new receiver to use.</returns>
    public (UserAudioReceiver oldReceiver, UserAudioReceiver newReceiver) UpdateUserReceiver(ulong id, UserAudioReceiveMode mode)
    {
        UserAudioReceiver oldReceiver = this.receivers[id];
        UserAudioReceiver newReceiver = CreateNewReceiver(mode);

        this.receivers[id] = newReceiver;
        oldReceiver.Close();

        return (oldReceiver, newReceiver);
    }

    /// <summary>
    /// Updates the receive mode for all users. Please note that this will destroy all old receivers.
    /// </summary>
    /// <param name="mode">The new receive mode to use.</param>
    public void UpdateAllReceiverModes(UserAudioReceiveMode mode)
    {
        foreach (ulong id in this.receivers.Keys)
        {
            UserAudioReceiver receiver = CreateNewReceiver(mode);
            UserAudioReceiver oldReceiver = this.receivers[id];

            this.receivers[id] = receiver;
            oldReceiver.Close();
        }
    }

    /// <inheritdoc/>
    protected internal override Task ProcessAudioAsync(ulong speakingUserId, uint sequence, AudioTimestamp timestamp, TimeSpan duration, byte[] audio)
    {
        this.receivers.TryGetValue(speakingUserId, out UserAudioReceiver? receiver);

        receiver?.Ingest(sequence, timestamp, duration, audio);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected internal override Task Initialize(IReadOnlyList<ulong> joinedUsers)
    {
        foreach (ulong id in joinedUsers)
        {
            UserAudioReceiver receiver = CreateNewReceiver(this.DefaultReceiveMode);
            receiver.IsSpeaking = false;

            this.receivers.TryAdd(id, receiver);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected internal override async Task ProcessUserJoinedAsync(ulong id)
    {
        UserAudioReceiver receiver = CreateNewReceiver(this.DefaultReceiveMode);
        receiver.IsSpeaking = false;

        this.receivers.TryAdd(id, receiver);

        await this.UserJoined(id);
    }

    /// <inheritdoc/>
    protected internal override async Task ProcessUserLeftAsync(ulong id)
    {
        this.receivers.Remove(id, out UserAudioReceiver? receiver);

        receiver?.Close();

        await this.UserLeft(id);
    }

    /// <inheritdoc/>
    protected internal override async Task ProcessUserStartedSpeakingAsync(ulong id)
    {
        this.receivers.TryGetValue(id, out UserAudioReceiver? receiver);

        receiver?.IsSpeaking = true;

        await this.UserStartedSpeaking(id);
    }

    /// <inheritdoc/>
    protected internal override async Task ProcessUserStoppedSpeakingAsync(ulong id)
    {
        this.receivers.TryGetValue(id, out UserAudioReceiver? receiver);

        receiver?.IsSpeaking = false;

        await this.UserStoppedSpeaking(id);
    }

    private UserAudioReceiver CreateNewReceiver(UserAudioReceiveMode mode)
    {
        return mode switch
        {
            UserAudioReceiveMode.Discard => new NullUserAudioReceiver(),
            UserAudioReceiveMode.Process => new RealtimeUserAudioReceiver(this.codec.CreateDecoder()),
            _ => new RealtimeUserAudioReceiver(this.codec.CreateDecoder())
        };
    }
}
