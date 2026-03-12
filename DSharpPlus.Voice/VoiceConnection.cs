using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using DSharpPlus.Clients;
using DSharpPlus.Net;
using DSharpPlus.Voice.AudioWriters;
using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.Cryptors;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.MemoryServices;
using DSharpPlus.Voice.MemoryServices.Collections;
using DSharpPlus.Voice.Protocol.RTCP.Payloads;
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
        AudioType type,
        IEnumerable<ulong> usersInCall
    )
    {
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
        this.options = provider.GetRequiredService<IOptions<VoiceOptions>>().Value;
        
        DiscordRestApiClientFactory apiClientFactory = provider.GetRequiredService<DiscordRestApiClientFactory>();
        this.apiClient = apiClientFactory.GetCurrentApplicationClient();

        this.encoder = this.codec.CreateEncoder(bitrate, type);
        this.sendingAudioChannel = Channel.CreateUnbounded<AudioBufferLease>();
        this.connectedUsers = [..usersInCall, userId];
        this.Receiver = new(this.codec, AudioReceiveMode.Process);

        this.userId = userId;
        this.channelId = channelId;
        this.guildId = guildId;

        this.mlsReady = null;

        this.vgwCancellation = new();
        this.heartbeatCancellation = new();
        this.audioCancellation = new();
    }

    // services and stuff we receive from DI for customization purposes
    private readonly IServiceScope serviceScope;

    private readonly ILoggerFactory loggerFactory;
    private readonly IMediaTransportService mediaTransport;
    private readonly IShardOrchestrator shardOrchestrator;
    private readonly IEventDispatcher dispatcher;
    private readonly DiscordRestApiClient apiClient;
    private readonly ITransportService voiceGateway;
    private readonly ICryptorFactory cryptorFactory;
    private readonly IAudioCodec codec;
    private readonly IAudioEncoder encoder;
    private readonly IE2EESession e2ee;
    private readonly IAudioWriterFactory audioWriterFactory;
    private readonly SynchronizedList<ulong> connectedUsers;
    private readonly Channel<AudioBufferLease> sendingAudioChannel;
    private readonly VoiceOptions options;
    private ICryptor cryptor;
    private ILogger logger;
    private AbstractAudioWriter? activeWriter;

    // hooks
    private Func<VoiceDisconnectReason, object?, Task>? disconnectHandler;
    private object? disconnectHandlerState;
    private Func<VoiceUser, object?, Task>? userJoinedHandler;
    private object? userJoinedHandlerState;

    // vgw tracking
    private CancellationTokenSource vgwCancellation;
    private CancellationTokenSource heartbeatCancellation;
    private TaskCompletionSource? mlsReady;
    private int lastSequence = -1;
    private int daveVersion;
    private int pendingHeartbeats;
    private uint pendingTransitionId;
    private int pendingTransitionProtocolVersion;
    private IPEndPoint? localEndpoint;

    // audio session tracking
    private readonly CancellationTokenSource audioCancellation;
    private Task? receiveAudioTask;
    private Task? audioKeepaliveTask;
    private Task? sendAudioTask;
    private readonly ConcurrentDictionary<uint, VoiceUser> users;

    // general connection info
    private string sessionId;
    private string token;
    private string endpoint;
    private readonly ulong userId;
    private readonly ulong channelId;
    private readonly ulong guildId;
    private uint ssrc;
    private bool isDisconnecting;
    private bool isDisposed;

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

    /// <summary>
    /// Creates a new audio writer to send audio through this connection.
    /// </summary>
    public AbstractAudioWriter CreateAudioWriter(AudioFormat format)
    {
        AbstractAudioWriter writer = this.audioWriterFactory.CreateAudioWriter(format, this, this.sendingAudioChannel.Writer);
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

    /// <summary>
    /// Sets a handler for when a new user joins the call and starts speaking for the first time.
    /// </summary>
    /// <param name="handler">A handler function taking in the user and the state parameter provided to this method.</param>
    /// <param name="state">Arbitrary state you want to provide your handler.</param>
    public void SetUserJoinHandler(Func<VoiceUser, object?, Task> handler, object? state = null)
    {
        this.userJoinedHandler = handler;
        this.userJoinedHandlerState = state;
    }

    private async Task RegisterSpeakingStatusAsync(uint ssrc, VoiceUser user)
    {
        this.users.AddOrUpdate(ssrc, user, (_, _) => user);

        if (!this.users.ContainsKey(ssrc))
        {
            await this.userJoinedHandler?.Invoke(user, this.userJoinedHandlerState);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;

        RTCPGoodbyePacket goodbye = new()
        {
            SSRCs = [this.ssrc],
            Reason = "Disconnecting"
        };

        await SendRTCPPacketsAsync([goodbye]);

        this.vgwCancellation.Cancel();
        this.heartbeatCancellation.Cancel();

        await this.voiceGateway.DisconnectAsync(WebSocketCloseStatus.NormalClosure);
        await this.mediaTransport.DisconnectAsync();

        await this.apiClient.ModifyGuildMemberAsync(this.guildId, this.userId, voiceChannelId: null);
        
        this.serviceScope.Dispose();
    }
}
