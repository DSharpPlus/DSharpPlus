using System;
using System.Net.WebSockets;
using System.Threading.Channels;
using System.Threading.Tasks;

using DSharpPlus.Clients;
using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.Cryptors;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.MemoryServices;
using DSharpPlus.Voice.MemoryServices.Collections;
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
        bool isStreamingConnection
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
        this.codec = provider.GetRequiredService<IAudioCodec>();
        this.e2ee = provider.GetRequiredService<IE2EESession>();
        this.options = provider.GetRequiredService<IOptions<VoiceOptions>>().Value;

        this.encoder = this.codec.CreateEncoder(bitrate, type, isStreamingConnection);
        this.sendingAudioChannel = Channel.CreateUnbounded<AudioBufferLease>();
        this.connectedUsers = [];

        this.userId = userId;
        this.channelId = channelId;
        this.guildId = guildId;

        this.mlsReady = null;
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
    private readonly SynchronizedList<ulong> connectedUsers;
    private readonly Channel<AudioBufferLease> sendingAudioChannel;
    private readonly VoiceOptions options;
    private ICryptor cryptor;
    private ILogger logger;
    private AudioClient audioClient;
    private AudioReceiver receiver;
    private AbstractAudioWriter activeWriter;

    // vgw tracking
    private TaskCompletionSource? mlsReady;
    private Task heartbeatTask;
    private Task vgwTask;
    private int lastSequence = -1;
    private int daveVersion;
    private int pendingHeartbeats;
    private uint pendingTransitionId;
    private int pendingTransitionProtocolVersion;

    // general connection info
    private string sessionId;
    private string token;
    private string endpoint;
    private readonly ulong userId;
    private readonly ulong channelId;
    private readonly ulong guildId;

    private uint ssrc;

    /// <summary>
    /// Indicates whether we are currently sending audio.
    /// </summary>
    public bool IsSpeaking { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await this.voiceGateway.DisconnectAsync(WebSocketCloseStatus.NormalClosure);
        await this.mediaTransport.DisconnectAsync();
        
        this.serviceScope.Dispose();
        this.audioClient.Dispose();

        this.heartbeatTask.Dispose();
        this.vgwTask.Dispose();
    }
}
