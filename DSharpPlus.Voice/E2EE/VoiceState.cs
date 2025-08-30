using DSharpPlus.Voice.Transport;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.Cryptors;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Voice.Models;
using System;

/// <summary>
/// Stores all state data related to voice connections to discord
/// </summary>
public class VoiceState : IDisposable
{
    /// <summary>
    /// Internal constructor as this type should be 
    /// created using its corresponding factory
    /// </summary>
    internal VoiceState() => _ = Task.Run(SendHeartbeatTaskAsync);

    /// <summary>
    /// CancellationToken for when we dispose of this class
    /// </summary>
    private readonly CancellationTokenSource cancellationTokenSource = new();

    /// <summary>
    /// Backing field for IsSpeaking property
    /// </summary>
    private bool isSpeaking = false;

    /// <summary>
    /// Backing field for HeartbeatInterval
    /// </summary>
    private PeriodicTimer? heartbeatTimer;

    /// <summary>
    /// Backing field for HeartbeatInterval
    /// </summary>
    private uint heartbeatInterval = 0;

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
            this.heartbeatTimer = new PeriodicTimer(System.TimeSpan.FromMilliseconds(value));
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
    /// End 2 End Encryption status
    /// </summary>
    public required bool E2EEIsActive { get; set; }

    /// <summary>
    /// Gets or sets whether the bot is sending voice data
    /// </summary>
    public bool IsSpeaking => this.isSpeaking;

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
    /// Cryptor to use for voice data
    /// </summary>
    public ICryptor? VoiceCryptor { get; set; }

    /// <summary>
    /// Sends the VoiceNegotiationTransportService a payload letting
    /// discord know we are starting to send voice data
    /// </summary>
    /// <returns></returns>
    public async Task OnStartSpeakingAsync()
    {
        await this.VoiceNegotiationTransportService.SendAsync<VoiceSpeakingPayload>(new()
        {
            Data = new VoiceSpeakingData
            {
                Delay = 0,
                Speaking = 5,
                Ssrc = this.Ssrc,
            }
        });

        this.isSpeaking = true;
    }

    /// <summary>
    /// Sends the VoiceNegotiationTransportService a payload letting
    /// discord know we are done sending voice data
    /// </summary>
    /// <returns></returns>
    public async Task OnStopSpeakingAsync()
    {
        await this.VoiceNegotiationTransportService.SendAsync<VoiceSpeakingPayload>(new()
        {
            Data = new VoiceSpeakingData
            {
                Delay = 0,
                Speaking = 0,
                Ssrc = this.Ssrc,
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
        await this.VoiceNegotiationTransportService.SendAsync<VoiceHeartbeatPayload>(new()
        {
            OpCode = 1,
            Sequence = this.SequenceNumber
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
