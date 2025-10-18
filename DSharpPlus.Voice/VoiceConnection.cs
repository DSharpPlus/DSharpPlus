using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Cryptors;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.Transport;
using DSharpPlus.Voice.Transport.Models.VoicePayloads;
using DSharpPlus.Voice.Transport.Models.VoicePayloads.Bidirectional;
using DSharpPlus.Voice.Transport.Models.VoicePayloads.Inbound;
using DSharpPlus.Voice.Transport.Models.VoicePayloads.Outbound;

namespace DSharpPlus.Voice;

/// <summary>
/// Stores all state data related to voice connections to discord
/// </summary>
public sealed class VoiceConnection : IDisposable
{
    /// <summary>
    /// Internal constructor as this type should be 
    /// created using its corresponding factory
    /// </summary>
    internal VoiceConnection() => _ = Task.Run(SendHeartbeatTaskAsync);

    /// <summary>
    /// CancellationToken for when we dispose of this class
    /// </summary>
    private readonly CancellationTokenSource cancellationTokenSource = new();

    // backing fields for properties below
    private uint heartbeatInterval = 0;
    private PeriodicTimer? heartbeatTimer;
    private ushort sequenceNumber = unchecked((ushort)Random.Shared.NextInt64());

    /// <summary>
    /// The delay in milliseconds between sending heartbeat payloads
    /// On set we start a timer to send the heartbeat to discord
    /// </summary>
    public uint HeartbeatInterval
    {
        get => this.heartbeatInterval;

        set
        {
            this.heartbeatInterval = value;
            this.heartbeatTimer?.Dispose();
            this.heartbeatTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(value));
        }
    }

    /// <summary>
    /// MediaTransportService for sending voice data frames
    /// </summary>
    public IMediaTransportService MediaTransportService { get; set; }

    /// <summary>
    /// SessionId for dicord voice connection
    /// </summary>
    public required string SessionId { get; set; }

    /// <summary>
    /// UserId for dicord voice connection
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// VoiceToken for dicord voice connection
    /// </summary>
    public required string VoiceToken { get; set; }

    /// <summary>
    /// Endpoint for dicord voice connection
    /// </summary>
    public required string Endpoint { get; set; }

    /// <summary>
    /// Connected Server(Guild)Id for dicord voice connection
    /// </summary>
    public required string ServerId { get; set; }

    /// <summary>
    /// Channel the discord voice connection was created in and is connected to
    /// </summary>
    public required string ConnectedChannel { get; set; }

    /// <summary>
    /// Indicates whether end-to-end encryption is enabled for this connection.
    /// </summary>
    public required bool E2EEIsActive { get; set; }

    /// <summary>
    /// Gets or sets whether the bot is sending voice data
    /// </summary>
    public bool IsSpeaking { get; private set; }

    /// <summary>
    /// Maps all known users id's to their corresponding ssrc value
    /// </summary>
    public ConcurrentDictionary<ulong, int> SsrcMap { get; set; } = new();

    /// <summary>
    /// The sequence number used for resuming a connection with the discord voice gateway
    /// </summary>
    public ushort SequenceNumber => this.VoiceNegotiationTransportService?.SequenceNumber ?? 0;

    /// <summary>
    /// ID for our voice data packets
    /// </summary>
    public uint Ssrc { get; set; }

    /// <summary>
    /// A dictionary that maps all know users to their UserInVoice object
    /// </summary>
    public ConcurrentDictionary<ulong, UserInVoice> OtherUsersInVoice { get; set; } = [];

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
    /// Gets a sequence number for the next audio packet to be sent.
    /// </summary>
    public ushort AudioSequenceNumber
    {
        get
        {
            ushort sequence = this.sequenceNumber;
            this.sequenceNumber++;
            return sequence;
        }
    }

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
                Ssrc = this.Ssrc,
            }
        });

        this.IsSpeaking = true;
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
                Ssrc = this.Ssrc,
            }
        });

        this.IsSpeaking = false;
    }

    /// <summary>
    /// Sends the heartbeat message to discord
    /// </summary>
    /// <returns></returns>
    private async Task SendHeartbeatAsync()
    {
        await this.VoiceNegotiationTransportService.SendAsync<DiscordGatewayMessage<VoiceHeartbeatAckData>>(new()
        {
            OpCode = (int)VoiceGatewayOpcode.Heartbeat,
            Data = new()
            { 
                SequenceAck = this.SequenceNumber,
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
            while (await (this.heartbeatTimer?.WaitForNextTickAsync(this.cancellationTokenSource.Token) ?? ValueTask.FromResult(false)))
            {
                await SendHeartbeatAsync();
            }

            // We get here if we do not have a heartbeat timer setup
            // We delay until one exists
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
    }
}
