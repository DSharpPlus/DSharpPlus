using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Clients;
using DSharpPlus.Voice.Cryptors;
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
        string sessionId,
        ulong userId,
        ulong channelId,
        ulong guildId,
        string voiceToken,
        string endpoint,
        uint ssrc
    )
    {
        this.userId = userId;
        this.channelId = channelId;
        this.guildId = guildId;
    }

    private string sessionId;
    
    private string token;
    private string endpoint;


    private readonly ulong userId;
    private readonly ulong channelId;
    private readonly ulong guildId;

    private IPEndPoint remoteUdpEndpoint;

    private uint ssrc;
    private bool isSpeaking;

    private int lastSequence;

    private readonly ConcurrentDictionary<ulong, VoiceUser> otherUsers = []; 

    private readonly CancellationTokenSource cancellationTokenSource = new();

    private ILogger logger;
    private readonly ILoggerFactory loggerFactory;

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

    private readonly IServiceScope serviceScope;
    private readonly IMediaTransportService mediaTransport;
    private readonly IShardOrchestrator shardOrchestrator;
    private readonly IEventDispatcher dispatcher;
    private readonly ITransportService voiceGateway;
    private readonly ICryptorFactory cryptorFactory;
    private ICryptor cryptor;

    private Task heartbeatTask;
    private Task vgwTask;

    /// <summary>
    /// Gets the writer currently used for this connection.
    /// </summary>
    internal AbstractAudioWriter ActiveWriter { get; private set; }

    /// <summary>
    /// Cleanup
    /// </summary>
    public void Dispose()
    {
        this.cancellationTokenSource.Cancel();
        this.cancellationTokenSource.Dispose();

        this.sendHeartbeatTask.Dispose();
        this.sendAudioTask.Dispose();
    }
}
