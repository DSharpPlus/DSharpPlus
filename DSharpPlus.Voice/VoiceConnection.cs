using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Clients;
using DSharpPlus.Voice.AudioWriters;
using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.Cryptors;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.MemoryServices.Channels;
using DSharpPlus.Voice.MemoryServices.Collections;
using DSharpPlus.Voice.Metrics;
using DSharpPlus.Voice.Protocol;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.Bidirectional;
using DSharpPlus.Voice.Protocol.RTCP.Payloads;
using DSharpPlus.Voice.Protocol.RTP;
using DSharpPlus.Voice.Receivers;
using DSharpPlus.Voice.Transport;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Voice;

/// <summary>
/// Stores all state data related to voice connections to discord
/// </summary>
public sealed partial class VoiceConnection : IAsyncDisposable
{
    internal VoiceConnection
    (
        IServiceScope scope,
        ulong userId,
        ulong channelId,
        ulong guildId,
        int bitrate,
        IEnumerable<ulong> usersInCall,
        Type receiverType,
        VoiceConnectionOptions options
    )
    {
        if (!receiverType.IsAssignableTo(typeof(AudioReceiver)))
        {
            throw new InvalidOperationException("An audio receiver must implement DSharpPlus.Voice.Receivers.IAudioReceiver.");
        }

        this.serviceScope = scope;
        IServiceProvider provider = scope.ServiceProvider;

        this.loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        this.mediaTransport = provider.GetRequiredService<IMediaTransportService>();
        this.shardOrchestrator = provider.GetRequiredService<IShardOrchestrator>();
        this.dispatcher = provider.GetRequiredService<IEventDispatcher>();
        this.voiceGateway = provider.GetRequiredService<ITransportService>();
        this.cryptorFactory = provider.GetRequiredService<ICryptorFactory>();
        this.audioWriterFactory = provider.GetRequiredService<IAudioWriterFactory>();
        this.codec = provider.GetRequiredService<IAudioCodec>();
        this.e2ee = provider.GetRequiredService<IE2EESession>();
        this.globalOptions = provider.GetRequiredService<IOptions<VoiceOptions>>().Value;
        this.Receiver = (AudioReceiver)provider.GetRequiredService(receiverType);
        this.metrics = provider.GetRequiredService<VoiceMetrics>();
        this.repository = provider.GetRequiredService<IVoiceConnectionRepository>();

        this.metrics.SetChannelId(channelId);

        this.selfMute = options.SelfMute;
        this.selfDeafen = options.SelfDeafen;
        this.noLongerSelfMuted = new();

        this.encoder = this.codec.CreateEncoder(bitrate, options.AudioType);
        this.sendingAudioChannel = new();
        this.connectedUsers = [..usersInCall, userId];
        this.ssrcs = [];
        this.voiceUsers = [];
        this.bitrate = int.Min(bitrate, options.MaxBitrate);
        this.bitrateLimit = options.MaxBitrate;
        this.rtcpReportTimer = new(TimeSpan.FromSeconds(5));
        this.timestamp = new();
        this.pauseTransmissionWhenAlone = options.PauseTransmissionIfAlone;
        this.noLongerAlone = new();
        this.audioType = options.AudioType;

        if (!this.pauseTransmissionWhenAlone)
        {
            this.noLongerAlone.TrySetResult();
        }

        this.userId = userId;
        this.ChannelId = channelId;
        this.guildId = guildId;

        this.mlsReady = null;

        this.vgwCancellation = new();
        this.heartbeatCancellation = new();
        this.audioCancellation = new();

        if (options.ReceiverSetup is not null)
        {
            options.ReceiverSetup(this.Receiver);
        }

        this.repository.RegisterConnection(this.guildId, this);
    }

    // services and stuff we receive from DI for customization purposes
    private readonly IServiceScope serviceScope;

    private readonly ILoggerFactory loggerFactory;
    private readonly IMediaTransportService mediaTransport;
    private readonly IShardOrchestrator shardOrchestrator;
    private readonly IEventDispatcher dispatcher;
    private readonly ITransportService voiceGateway;
    private readonly ICryptorFactory cryptorFactory;
    private readonly IAudioCodec codec;
    private readonly IAudioEncoder encoder;
    private readonly IE2EESession e2ee;
    private readonly IAudioWriterFactory audioWriterFactory;
    private readonly IVoiceConnectionRepository repository;
    private SynchronizedList<ulong> connectedUsers;
    private readonly AudioChannel sendingAudioChannel;
    private readonly VoiceOptions globalOptions;
    private ICryptor cryptor;
    private ILogger logger;
    private AudioWriter? activeWriter;

    // hooks
    private Func<VoiceDisconnectReason, object?, Task>? disconnectHandler;
    private object? disconnectHandlerState;

    // vgw tracking
    private CancellationTokenSource vgwCancellation;
    private CancellationTokenSource heartbeatCancellation;
    private TaskCompletionSource? mlsReady;
    private int lastSequence = -1;
    private int daveVersion;
    private int pendingHeartbeats;
    private uint? pendingTransitionId;
    private int? pendingTransitionProtocolVersion;
    private IPEndPoint? localEndpoint;
    private bool selfMute;
    private bool selfDeafen;
    private TaskCompletionSource noLongerSelfMuted;
    private readonly object awaitMoveInstructionState = new();
    private TaskCompletionSource? awaitMoveInstruction;
    private bool isReady;

    // audio session tracking
    private readonly CancellationTokenSource audioCancellation;
    private Task? receiveAudioTask;
    private Task? audioKeepaliveTask;
    private Task? sendAudioTask;
    private Task? rtcpTask;
    private readonly ConcurrentDictionary<uint, ulong> ssrcs;
    private readonly ConcurrentDictionary<ulong, VoiceUser> voiceUsers;
    private readonly int bitrateLimit;
    private int bitrate;
    private readonly PeriodicTimer rtcpReportTimer;
    private int averageRTCPPacketSize;
    private uint packetsSent;
    private uint opusBytesSent;
    private RTPTimestamp timestamp;
    private readonly bool pauseTransmissionWhenAlone;
    private TaskCompletionSource noLongerAlone;
    private readonly AudioType audioType;
    private bool isAudioProvidedFasterThanRealtime;
    private bool isAudioProbablyRealtime;
    private bool isAudioSpiraling;

    // general connection info
    private string sessionId;
    private string token;
    private string endpoint;
    private readonly ulong userId;
    private readonly ulong guildId;
    private uint ssrc;
    private bool isDisconnecting;
    private bool isDisposed;
    private readonly VoiceMetrics metrics;

    /// <summary>
    /// Gets the current channel ID of this connection;
    /// </summary>
    public ulong ChannelId { get; private set; }

    /// <summary>
    /// Indicates whether we are currently sending audio.
    /// </summary>
    public bool IsSpeaking { get; private set; }

    /// <summary>
    /// Indicates the latency between the client and the voice server. For latency between this client and other clients, please refer to
    /// the connection metrics.
    /// </summary>
    public TimeSpan AudioConnectionLatency { get; private set; }

    /// <summary>
    /// Provides a mechanism for receiving audio from Discord.
    /// </summary>
    public AudioReceiver Receiver { get; }

    internal async Task MoveChannelAsync(ulong newChannelId, IEnumerable<ulong> userIds)
    {
        this.ChannelId = newChannelId;
        this.connectedUsers = [..userIds];

        this.IsSpeaking = false;
        this.isReady = false;

        this.awaitMoveInstruction ??= new(this.awaitMoveInstructionState);
        this.awaitMoveInstruction.TrySetResult();

        await ReconnectInternalAsync(true);
    }

    internal void SetChannelMaxBitrate(int bitrate)
    {
        this.bitrate = int.Min(bitrate, this.bitrateLimit);
        this.logger.LogDebug("Updating bitrate to {bitrate}", this.bitrate);

        if (this.activeWriter is OggOpusAudioWriter)
        {
            this.logger.LogDebug("Currently using ogg/opus playback, the bitrate setting will not be respected.");
        }

        this.codec.GetEncoder().SetBitrate(this.bitrate);
    }

    /// <summary>
    /// Gets tracked metrics for the current connection.
    /// </summary>
    public VoiceMetricsCollection GetVoiceMetrics()
        => this.metrics.GetVoiceMetrics();

    /// <summary>
    /// Creates a new audio writer to send audio through this connection.
    /// </summary>
    public AudioWriter CreateAudioWriter(AudioFormat format)
    {
        AudioWriter writer = this.audioWriterFactory.CreateAudioWriter(format, this.sendingAudioChannel.Writer);
        this.activeWriter = writer;
        return writer;
    }

    /// <summary>
    /// Sets a handler for when the connection gets severed.
    /// </summary>
    /// <param name="handler">A handler function taking in the reason for disconnecting and the state parameter provided to this method.</param>
    /// <param name="state">Arbitrary state you want to provide your handler.</param>
    public void SetDisconnectHandler(Func<VoiceDisconnectReason, object?, Task> handler, object? state = null)
    {
        this.disconnectHandler = handler;
        this.disconnectHandlerState = state;
    }

    private async Task RegisterSpeakingStatusAsync(uint ssrc, ulong userId, bool isSpeaking)
    {
        this.ssrcs.TryAdd(ssrc, userId);

        if (this.voiceUsers.TryGetValue(userId, out VoiceUser? user))
        {
            user.IsSpeaking = isSpeaking;
        }
        else
        {
            this.voiceUsers.TryAdd(userId, new()
            {
                SSRC = ssrc,
                UserId = userId,
                IsSpeaking = isSpeaking
            });
        }

        if (isSpeaking)
        {
            await this.Receiver.ProcessUserStartedSpeakingAsync(userId);
        }
        else
        {
            await this.Receiver.ProcessUserStoppedSpeakingAsync(userId);
            this.voiceUsers[userId].IndicateStoppedSpeaking();
        }
    }

    /// <summary>
    /// Sets whether the bot should mute itself in the current voice channel.
    /// </summary>
    public async Task SetSelfMuteStatusAsync(bool selfMute)
    {
        this.selfMute = selfMute;

        VoiceStateUpdateEvent vsUpdate = new()
        {
            Data = new()
            {
                GuildId = this.guildId,
                ChannelId = this.ChannelId,
                Mute = this.selfMute,
                Deafen = this.selfDeafen
            }
        };

        await this.shardOrchestrator.SendOutboundEventAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(vsUpdate)), this.guildId);
        await SendSpeakingStatusAsync(selfMute ? VoiceSpeakingFlags.None : VoiceSpeakingFlags.Microphone);

        if (selfMute)
        {
            this.activeWriter?.SignalSilence();

            // if necessary, replace the task completion source, but it may not actually be necessary if the user double-called this
            if (this.noLongerSelfMuted.Task.IsCompleted)
            {
                this.noLongerSelfMuted = new();
            }
        }
        else
        {
            this.noLongerSelfMuted.TrySetResult();
        }
    }

    /// <summary>
    /// Sets whether the bot should deafen itself in the current voice channel.
    /// </summary>
    public async Task SetSelfDeafenStatusAsync(bool selfDeafen)
    {
        this.selfDeafen = selfDeafen;

        VoiceStateUpdateEvent vsUpdate = new()
        {
            Data = new()
            {
                GuildId = this.guildId,
                ChannelId = this.ChannelId,
                Mute = this.selfMute,
                Deafen = this.selfDeafen
            }
        };

        await this.shardOrchestrator.SendOutboundEventAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(vsUpdate)), this.guildId);
    }

    private async Task<IPEndPoint> PerformIPDiscoveryAsync(uint ssrc)
    {
        byte[] request = new byte[74];
        Span<byte> requestSpan = request;

        ArrayPoolBufferWriter<byte> receive = new();

        requestSpan.Clear();

        // request type 1, request length 70 after the length field
        BinaryPrimitives.WriteUInt16BigEndian(requestSpan, 1);
        BinaryPrimitives.WriteUInt16BigEndian(requestSpan[2..], 70);
        BinaryPrimitives.WriteUInt32BigEndian(requestSpan[4..], ssrc);

        await this.mediaTransport.SendAsync(request);
        await this.mediaTransport.ReceiveAsync(receive);

        // ensure the packet is well-formed

        if (BinaryPrimitives.ReadUInt16BigEndian(receive.WrittenSpan) != 2 || BinaryPrimitives.ReadUInt16BigEndian(receive.WrittenSpan[2..]) != 70)
        {
            throw new InvalidDataException("The IP discovery response was malformed.");
        }

        if (BinaryPrimitives.ReadUInt32BigEndian(receive.WrittenSpan[4..]) != ssrc)
        {
            throw new InvalidDataException("Reveived IP discovery packet with a different SSRC from ours.");
        }

        // the IP address is fixed-length, length-prefixed and null-terminated for. some reason. find the null-terminator and slice it off
        ReadOnlySpan<byte> addressSpan = receive.WrittenSpan[8..^2];
        int nullTerminator = addressSpan.IndexOf((byte)0);

        if (nullTerminator == -1)
        {
            throw new InvalidDataException("The received IP address was not null-terminated.");
        }

        addressSpan = addressSpan[..nullTerminator];
        IPAddress address = IPAddress.Parse(addressSpan);
        ushort port = BinaryPrimitives.ReadUInt16BigEndian(receive.WrittenSpan[72..]);

        return new(address, port);
    }

    public async ValueTask DisposeAsync()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;
        this.isDisconnecting = true;

        RTCPGoodbyePacket goodbye = new()
        {
            SSRCs = [this.ssrc],
            Reason = "Disconnecting"
        };

        await SendRTCPPacketsAsync([goodbye]);

        this.vgwCancellation.Cancel();
        this.heartbeatCancellation.Cancel();
        this.audioCancellation.Cancel();

        await this.voiceGateway.DisconnectAsync(WebSocketCloseStatus.NormalClosure);
        await this.mediaTransport.DisconnectAsync();

        this.sendingAudioChannel.Clear();

        VoiceStateUpdateEvent update = new()
        {
            Data = new()
            {
                GuildId = this.guildId,
                ChannelId = null,
                Mute = this.selfMute,
                Deafen = this.selfDeafen
            }
        };

        await this.shardOrchestrator.SendOutboundEventAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(update)), this.guildId);

        this.metrics.CloseConnection();
        this.repository.UnregisterConnection(this.guildId);
        
        this.serviceScope.Dispose();
    }
}
