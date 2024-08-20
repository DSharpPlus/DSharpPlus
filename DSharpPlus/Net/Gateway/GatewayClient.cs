using System;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Gateway.Compression;
using DSharpPlus.Net.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Gateway;

/// <inheritdoc cref="IGatewayClient"/>
public sealed class GatewayClient : IGatewayClient
{
    private ILogger logger;
    private readonly IGatewayController controller;
    private readonly ITransportService transportService;
    private readonly ChannelWriter<GatewayPayload> eventWriter;
    private readonly GatewayClientOptions options;
    private readonly ILoggerFactory factory;
    private readonly string token;
    private readonly bool compress;

    private DateTimeOffset lastSentHeartbeat = DateTimeOffset.UtcNow;
    private int pendingHeartbeats;

    private int remainingOutboundPayloads = 120;
    private DateTimeOffset lastOutboundPayloadReset = DateTimeOffset.UtcNow;
    private SpinLock resetLock = new();

    private int lastReceivedSequence = 0;
    private string? resumeUrl;
    private string? sessionId;
    private string? reconnectUrl;
    private ShardInfo? shardInfo;

    private GatewayPayload? identify;
    private CancellationTokenSource gatewayTokenSource;

    /// <inheritdoc/>
    public bool IsConnected { get; private set; }

    /// <inheritdoc/>
    public TimeSpan Ping { get; private set; }

    /// <inheritdoc/>
    public int ShardId => this.shardInfo?.ShardId ?? 0;

    public GatewayClient
    (
        [FromKeyedServices("DSharpPlus.Gateway.EventChannel")]
        Channel<GatewayPayload> eventChannel,
        
        ITransportService transportService,
        IOptions<TokenContainer> tokenContainer,
        IPayloadDecompressor decompressor,
        IOptions<GatewayClientOptions> options,
        IGatewayController controller,
        ILoggerFactory factory
    )
    {
        this.transportService = transportService;
        this.eventWriter = eventChannel.Writer;
        this.factory = factory;
        this.token = tokenContainer.Value.GetToken();
        this.gatewayTokenSource = null!;
        this.compress = !decompressor.IsTransportCompression;
        this.options = options.Value;
        this.controller = controller;

        this.logger = factory.CreateLogger("DSharpPlus.Net.Gateway.IGatewayClient - invalid shard");
    }

    /// <inheritdoc/>
    public async ValueTask ConnectAsync
    (
        string url,
        DiscordActivity? activity = null,
        DiscordUserStatus? status = null,
        DateTimeOffset? idleSince = null,
        ShardInfo? shardInfo = null
    )
    {
        this.logger = shardInfo is null
            ? this.factory.CreateLogger("DSharpPlus.Net.Gateway.IGatewayClient")
            : this.factory.CreateLogger($"DSharpPlus.Net.Gateway.IGatewayClient - Shard {shardInfo.ShardId}");

        this.shardInfo = shardInfo;
        this.reconnectUrl = url;

        for (uint i = 0; i < this.options.MaxReconnects; i++)
        {
            try
            {
                this.gatewayTokenSource = new();
                await this.transportService.ConnectAsync(url, shardInfo?.ShardId);

                TransportFrame initialFrame = await this.transportService.ReadAsync();

                if (!initialFrame.TryGetMessage(out string? hello))
                {
                    await HandleErrorAndAttemptToReconnectAsync(initialFrame);
                }

                GatewayPayload? helloEvent = JsonConvert.DeserializeObject<GatewayPayload>(hello);

                if (helloEvent is not { OpCode: GatewayOpCode.Hello })
                {
                    this.logger.LogWarning("Expected HELLO payload from Discord, received {NotQuiteHello}", hello);
                    continue;
                }

                GatewayHello helloPayload = ((JObject)helloEvent.Data).ToDiscordObject<GatewayHello>();

                this.logger.LogTrace
                (
                    "Received hello event, starting heartbeating with an interval of {interval} and identifying.",
                    TimeSpan.FromMilliseconds(helloPayload.HeartbeatInterval)
                );

                _ = HeartbeatAsync(helloPayload.HeartbeatInterval, this.gatewayTokenSource.Token);
                _ = HandleEventsAsync(this.gatewayTokenSource.Token);

                StatusUpdate? statusUpdate;

                if (activity is null && status is null && idleSince is null)
                {
                    statusUpdate = null;
                }
                else
                {
                    statusUpdate = new();

                    if (activity is not null)
                    {
                        statusUpdate.Activity = new TransportActivity(activity);
                    }

                    if (status is not null)
                    {
                        statusUpdate.Status = status.Value;
                    }

                    if (idleSince is not null)
                    {
                        statusUpdate.IdleSince = idleSince.Value.ToUnixTimeMilliseconds();
                    }
                }

                GatewayIdentify inner = new()
                {
                    Token = this.token,
                    Compress = this.compress,
                    LargeThreshold = this.options.LargeThreshold,
                    ShardInfo = shardInfo,
                    Presence = statusUpdate,
                    Intents = this.options.Intents
                };

                GatewayPayload identify = new()
                {
                    OpCode = GatewayOpCode.Identify,
                    Data = inner
                };

                this.identify = identify;

                await WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(identify)));

                this.logger.LogTrace("Identified with the Discord gateway");
                break;
            }
            catch (Exception e)
            {
                this.logger.LogError(exception: e, "Encountered an error while connecting.");
                await Task.Delay(this.options.GetReconnectionDelay(i));
                continue;
            }
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisconnectAsync()
    {
        this.IsConnected = false;
        this.gatewayTokenSource.Cancel();
        await this.transportService.DisconnectAsync(WebSocketCloseStatus.NormalClosure);
    }

    /// <inheritdoc/>
    public async ValueTask WriteAsync(byte[] payload)
    {
        if (DateTimeOffset.UtcNow >= this.lastOutboundPayloadReset.AddMinutes(1))
        {
            this.lastOutboundPayloadReset = DateTimeOffset.UtcNow;
            this.remainingOutboundPayloads = 120;
        }

        int remaining = Interlocked.Decrement(ref this.remainingOutboundPayloads);

        while (remaining < 0)
        {
            await Task.Delay(this.lastOutboundPayloadReset.AddMinutes(1) - DateTimeOffset.UtcNow);

            bool taken = false;
            this.resetLock.TryEnter(ref taken);

            // assume that another thread is taking care of this. wait until the lock is free to continue (this is horrible)
            if (!taken)
            {
                this.resetLock.Enter(ref taken);
                this.resetLock.Exit();
            }
            else
            {
                this.lastOutboundPayloadReset = DateTimeOffset.UtcNow;
                this.remainingOutboundPayloads = 120;
            }

            remaining = Interlocked.Decrement(ref this.remainingOutboundPayloads);
        }

        await this.transportService.WriteAsync(payload);
    }

    /// <summary>
    /// Handles dispatching heartbeats to Discord.
    /// </summary>
    private async Task HeartbeatAsync(int heartbeatInterval, CancellationToken ct)
    {
        double jitter = Random.Shared.NextDouble() * 0.95;

        await Task.Delay((int)(heartbeatInterval * jitter), ct);
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(heartbeatInterval));

        do
        {
            await WriteAsync(Encoding.UTF8.GetBytes($"{{\"op\":1,\"d\":{this.lastReceivedSequence}}}"));
            this.logger.LogTrace("Heartbeat sent with sequence number {Sequence}.", this.lastReceivedSequence);

            this.lastSentHeartbeat = DateTimeOffset.UtcNow;
            this.pendingHeartbeats++;

            if (this.pendingHeartbeats > 5)
            {
                _ = this.controller.ZombiedAsync(this);
            }
        } while (await timer.WaitForNextTickAsync(ct));
    }

    /// <summary>
    /// Handles events incoming from Discord.
    /// </summary>
    private async Task HandleEventsAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                TransportFrame frame = await this.transportService.ReadAsync();
                GatewayPayload? payload;

                if (!frame.TryGetMessage(out string? data))
                {
                    await HandleErrorAndAttemptToReconnectAsync(frame);
                    continue;
                }

                payload = JsonConvert.DeserializeObject<GatewayPayload>(data);

                if (payload is null)
                {
                    this.logger.LogError("Received invalid inbound event: {Data}", data);
                    continue;
                }

                this.lastReceivedSequence = payload.Sequence ?? this.lastReceivedSequence;

                switch (payload.OpCode)
                {
                    case GatewayOpCode.Dispatch when payload.EventName is "READY":

                        ReadyPayload readyPayload = ((JObject)payload.Data).ToDiscordObject<ReadyPayload>();

                        this.resumeUrl = readyPayload.ResumeGatewayUrl;
                        this.sessionId = readyPayload.SessionId;

                        payload = new ShardIdContainingGatewayPayload
                        {
                            Data = payload.Data,
                            EventName = payload.EventName,
                            OpCode = payload.OpCode,
                            Sequence = payload.Sequence,
                            ShardId = this.ShardId
                        };

                        this.IsConnected = true;

                        this.logger.LogTrace("Received READY, the gateway is now operational.");

                        break;

                    case GatewayOpCode.Resume:

                        payload = new ShardIdContainingGatewayPayload
                        {
                            Data = payload.Data,
                            EventName = payload.EventName,
                            OpCode = payload.OpCode,
                            Sequence = payload.Sequence,
                            ShardId = this.ShardId
                        };

                        break;

                    case GatewayOpCode.HeartbeatAck:

                        this.Ping = DateTimeOffset.UtcNow - this.lastSentHeartbeat;
                        this.pendingHeartbeats = 0;

                        // Task is not awaited to dont block gw recieve loop
                        _ = this.controller.HeartbeatedAsync(this);

                        continue;

                    case GatewayOpCode.InvalidSession:

                        this.logger.LogTrace("Received INVALID_SESSION, resumable: {Resumable}", (bool)payload.Data);
                        bool success = (bool)payload.Data ? await TryResumeAsync() : await TryReconnectAsync();

                        if (!success)
                        {
                            this.logger.LogError("The session was invalidated and resuming/reconnecting failed.");
                            _ = this.controller.SessionInvalidatedAsync(this);
                        }

                        break;

                    case GatewayOpCode.Reconnect:

                        this.logger.LogTrace("Received RECONNECT");
                        _ = this.controller.ReconnectRequestedAsync(this);

                        if (!(this.options.AutoReconnect && await TryReconnectAsync()))
                        {
                            this.logger.LogError("A reconnection attempt requested by Discord failed.");
                            _ = this.controller.ReconnectFailedAsync(this);
                        }

                        continue;
                }

                await this.eventWriter.WriteAsync(payload, ct);
            }
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "An exception occurred in event handling.");
        }
    }

    /// <summary>
    /// Attempts to resume a connection, returning whether this was successful.
    /// </summary>
    private async Task<bool> TryResumeAsync()
    {
        if (this.resumeUrl is null || this.sessionId is null)
        {
            return this.options.AutoReconnect && await TryReconnectAsync();
        }

        try
        {
            for (uint i = 0; i < this.options.MaxReconnects; i++)
            {
                this.logger.LogTrace("Attempting resume, attempt {Attempt}", i + 1);

                try
                {
                    this.IsConnected = false;

                    await this.transportService.DisconnectAsync(WebSocketCloseStatus.NormalClosure);
                    await this.transportService.ConnectAsync(this.resumeUrl, this.shardInfo?.ShardId);

                    await WriteAsync
                    (
                        Encoding.UTF8.GetBytes
                        (
                            JsonConvert.SerializeObject
                            (
                                new GatewayPayload
                                {
                                    OpCode = GatewayOpCode.Resume,
                                    Data = new GatewayResume
                                    {
                                        SequenceNumber = this.lastReceivedSequence,
                                        Token = this.token,
                                        SessionId = this.sessionId
                                    }
                                }
                            )
                        )
                    );

                    this.logger.LogTrace("Resumed an existing gateway session.");
                    this.IsConnected = true;
                    break;
                }
                catch (WebSocketException e) when (e.InnerException is HttpRequestException)
                {
                    // no internet connection, but we can still try to resume later
                    TimeSpan delay = this.options.GetReconnectionDelay(i);

                    this.logger.LogWarning("Internet connection interrupted, waiting for {Delay}", delay);

                    await Task.Delay(delay);
                    continue;
                }
                catch
                {
                    throw;
                }
            }
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to resume an existing gateway session.");
            return this.options.AutoReconnect && await TryReconnectAsync();
        }

        return true;
    }

    /// <summary>
    /// Attempts to reconnect to the gateway, returning whether this was successful.
    /// </summary>
    private async Task<bool> TryReconnectAsync()
    {
        for (uint i = 0; i < this.options.MaxReconnects; i++)
        {
            this.logger.LogTrace("Attempting reconnect, attempt {Attempt}", i + 1);

            try
            {
                this.IsConnected = false;

                try
                {
                    this.gatewayTokenSource.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // thrown if gatewayTokenSource was disposed when cancelling, ignore since we create a new one anyway
                }

                this.gatewayTokenSource = new();

                // ensure we're disconnected no matter what the previous state was
                await this.transportService.DisconnectAsync(WebSocketCloseStatus.NormalClosure);

                await this.transportService.ConnectAsync(this.reconnectUrl!, this.shardInfo?.ShardId);

                TransportFrame initialFrame = await this.transportService.ReadAsync();

                if (!initialFrame.TryGetMessage(out string? hello))
                {
                    await HandleErrorAndAttemptToReconnectAsync(initialFrame);
                }

                GatewayPayload? helloEvent = JsonConvert.DeserializeObject<GatewayPayload>(hello);

                if (helloEvent is not { OpCode: GatewayOpCode.Hello })
                {
                    throw new InvalidDataException($"Expected HELLO payload from Discord, received {hello}");
                }

                GatewayHello helloPayload = ((JObject)helloEvent.Data).ToDiscordObject<GatewayHello>();

                this.logger.LogTrace
                (
                    "Received hello event, starting heartbeating with an interval of {interval} and identifying.",
                    TimeSpan.FromMilliseconds(helloPayload.HeartbeatInterval)
                );

                this.IsConnected = true;
                _ = HeartbeatAsync(helloPayload.HeartbeatInterval, this.gatewayTokenSource.Token);
                _ = HandleEventsAsync(this.gatewayTokenSource.Token);

                await WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.identify)));

                this.logger.LogTrace("Identified with the Discord gateway");
                return true;
            }
            catch (Exception e)
            {
                TimeSpan delay = this.options.GetReconnectionDelay(i);

                this.logger.LogError(e, "Reconnecting failed, waiting for {Delay}", delay);

                await Task.Delay(delay);
                continue;
            }
        }

        return false;
    }

    private async Task HandleErrorAndAttemptToReconnectAsync(TransportFrame frame)
    {
        if (frame.TryGetException<WebSocketException>(out _))
        {
            await TryResumeAsync();
        }
        else if (frame.TryGetException(out _) && this.options.AutoReconnect)
        {
            await TryReconnectAsync();
        }
        else if (frame.TryGetErrorCode(out int errorCode))
        {
            _ = errorCode switch
            {
                >= 4000 and <= 4003 => await TryResumeAsync(),
                >= 4005 and <= 4009 => await TryResumeAsync(),
                _ => this.options.AutoReconnect && await TryReconnectAsync()
            };
        }
    }

    /// <inheritdoc/>
    public async ValueTask ReconnectAsync() 
        => _ = await TryReconnectAsync();
}
