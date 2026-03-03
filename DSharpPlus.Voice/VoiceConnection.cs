using System;
using System.Threading.Channels;
using System.Threading.Tasks;

using DSharpPlus.Clients;
using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.Cryptors;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.MemoryServices;
using DSharpPlus.Voice.Transport;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice;

/// <summary>
/// Stores all state data related to voice connections to discord
/// </summary>
public sealed partial class VoiceConnection : IDisposable
{
    public VoiceConnection
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

        this.encoder = this.codec.CreateEncoder(bitrate, type, isStreamingConnection);
        this.sendingAudioChannel = Channel.CreateUnbounded<AudioBufferLease>();

        this.userId = userId;
        this.channelId = channelId;
        this.guildId = guildId;
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
    private readonly Channel<AudioBufferLease> sendingAudioChannel;
    private ICryptor cryptor;
    private ILogger logger;
    private AudioClient audioClient;
    private AudioReceiver receiver;
    private AbstractAudioWriter activeWriter;

    // vgw tracking
    private Task heartbeatTask;
    private Task vgwTask;
    private int lastSequence;
    private int daveVersion;
    private DateTimeOffset lastSentHeartbeat;
    private int pendingHeartbeats;

    // general connection info
    private string sessionId;
    private string token;
    private string endpoint;
    private readonly ulong userId;
    private readonly ulong channelId;
    private readonly ulong guildId;

    private uint ssrc;

    /// <summary>
    /// Cleanup
    /// </summary>
    public void Dispose()
    {
        this.serviceScope.Dispose();
        this.audioClient.Dispose();

        this.heartbeatTask.Dispose();
        this.vgwTask.Dispose();
    }
}
