using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Cryptors;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.Protocol.Gateway.DaveV1;
using DSharpPlus.Voice.Protocol.Gateway.DaveV1.Bidirectional;
using DSharpPlus.Voice.Protocol.Gateway.DaveV1.Inbound;
using DSharpPlus.Voice.Protocol.Gateway.DaveV1.Outbound;
using DSharpPlus.Voice.Transport;

namespace DSharpPlus.Voice;

/// <summary>
/// Stores all state data related to voice connections to discord
/// </summary>
public sealed partial class VoiceConnection : IDisposable
{
    public VoiceConnection
    (
        string sessionId,
        ulong userId,
        ulong channelId,
        ulong guildId,
        string voiceToken,
        string endpoint,
        uint ssrc
    )
    {
        this.sessionId = sessionId;
        this.userId = userId;
        this.channelId = channelId;
        this.guildId = guildId;

        this.voiceToken = voiceToken;
        this.endpoint = endpoint;
        this.ssrc = ssrc;

        // it'll be set when we start heartbeating, this is safe
        this.heartbeatTimer = default!;
        this.sendTimer = new(TimeSpan.FromMilliseconds(20));
        this.sendHeartbeatTask = Task.Run(SendHeartbeatTaskAsync);
    }

    private readonly string sessionId;
    private readonly ulong userId;
    private readonly ulong channelId;
    private readonly ulong guildId;

    private readonly string voiceToken;
    private readonly string endpoint;
    private readonly uint ssrc;

    private bool isE2ee;
    private bool isSpeaking;

    private ushort lastSeenSequenceNumber;

    private readonly ConcurrentDictionary<ulong, VoiceUser> otherUsers = []; 

    private readonly CancellationTokenSource cancellationTokenSource = new();

    // backing fields for properties below
    private PeriodicTimer heartbeatTimer;
    private readonly PeriodicTimer sendTimer;

    private readonly Task sendHeartbeatTask;
    private readonly Task sendAudioTask;

    /// <summary>
    /// The delay, in milliseconds, at which we send heartbeats to the voice gateway.
    /// </summary>
    internal uint HeartbeatInterval
    {
        get;

        set
        {
            field = value;
            this.heartbeatTimer?.Dispose();
            this.heartbeatTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(value));
        }
    } = 10000;

    /// <summary>
    /// MediaTransportService for sending voice data frames
    /// </summary>
    public IMediaTransportService MediaTransportService { get; set; }

    /// <summary>
    /// Keeps track of DaveState and handles events
    /// </summary>
    public DaveStateHandler? DaveStateHandler { get; set; }

    /// <summary>
    /// Handles the connection to discords voice gateway and negotiates the voice connection state.
    /// </summary>
    public ITransportService? VoiceNegotiationTransportService { get; set; }

    /// <summary>
    /// The cryptor in use for transport encryption.
    /// </summary>
    public ICryptor? VoiceCryptor { get; set; }

    /// <summary>
    /// Gets the writer currently used for this connection.
    /// </summary>
    internal AbstractAudioWriter ActiveWriter { get; private set; }

    /// <summary>
    /// Indicates to Discord that we are ready to send audio.
    /// </summary>
    public async Task StartSpeakingAsync()
    {
        await this.VoiceNegotiationTransportService.SendAsync<DiscordGatewayMessage<VoiceSpeakingData>>(new()
        {
            Data = new()
            {
                Delay = 0,
                Speaking = VoiceSpeakingFlags.Microphone,
                Ssrc = this.ssrc,
            }
        });

        this.isSpeaking = true;
    }

    /// <summary>
    /// Indicates to Discord that we will not send further audio.
    /// </summary>
    /// <remarks>
    /// DSharpPlus.Voice does not use this method on its own; it's perfectly fine to just not send audio.
    /// </remarks>
    public async Task StopSpeakingAsync()
    {
        await this.VoiceNegotiationTransportService.SendAsync<DiscordGatewayMessage<VoiceSpeakingData>>(new()
        {
            Data = new()
            {
                Delay = 0,
                Speaking = VoiceSpeakingFlags.None,
                Ssrc = this.ssrc,
            }
        });

        this.isSpeaking = false;
    }

    /// <summary>
    /// Sends the heartbeat message to discord
    /// </summary>
    /// <returns></returns>
    private async Task SendHeartbeatAsync()
    {
        await this.VoiceNegotiationTransportService.SendAsync<DiscordGatewayMessage<VoiceHeartbeatAckData>>(new()
        {
            Opcode = (int)VoiceGatewayOpcode.Heartbeat,
            Data = new()
            { 
                SequenceAck = this.lastSeenSequenceNumber,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            }
        });
    }

    /// <summary>
    /// A task that will periodically send the heartbeat payload to discord
    /// </summary>
    /// <returns></returns>
    private async Task SendHeartbeatTaskAsync()
    {
        while (!this.cancellationTokenSource.IsCancellationRequested)
        {
            while (await this.heartbeatTimer.WaitForNextTickAsync(this.cancellationTokenSource.Token))
            {
                await SendHeartbeatAsync();
            }

            // We get here if we do not have a heartbeat timer set up, so we delay until one exists
            await Task.Delay(10000);
        }
    }

    /// <summary>
    /// Cleanup
    /// </summary>
    public void Dispose()
    {
        this.cancellationTokenSource.Cancel();
        this.cancellationTokenSource.Dispose();
        this.DaveStateHandler.Dispose();
        this.MediaTransportService.Dispose();

        this.sendHeartbeatTask.Dispose();
        this.sendAudioTask.Dispose();
    }
}
