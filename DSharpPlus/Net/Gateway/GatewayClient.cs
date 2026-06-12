using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.RateLimiting;
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
    private readonly ITransportService transportService;
    private readonly ChannelWriter<GatewayPayload> eventWriter;
    private readonly GatewayClientOptions options;
    private readonly ILoggerFactory factory;
    private readonly RateLimiter ratelimiter;
    private readonly IPayloadDecompressor decompressor;

    private readonly string token;
    private readonly bool compress;

    private DateTimeOffset lastSentHeartbeat = DateTimeOffset.UtcNow;
    private int pendingHeartbeats;

    private int lastReceivedSequence = 0;
    private string? resumeUrl;
    private string? sessionId;
    private ShardInfo? shardInfo;

    private string reconnectUrl;
    private DiscordActivity? activity = null;
    private DiscordUserStatus? status = null;
    private DateTimeOffset? idleSince = null;

    private CancellationTokenSource gatewayTokenSource;
    private TaskCompletionSource<GatewayConnectionFrame> gatewayTask;
    private bool closureRequested = false;

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
        ILoggerFactory factory
    )
    {
        this.transportService = transportService;
        this.eventWriter = eventChannel.Writer;
        this.factory = factory;
        this.token = tokenContainer.Value.GetToken();
        this.gatewayTokenSource = null!;
        this.compress = !decompressor.IsTransportCompression;
        this.decompressor = decompressor;
        this.options = options.Value;

        // 120/s is the gateway limit; there's no particular reason why i chose 2400 for the queue limit, if necessary it can be increased
        this.ratelimiter = new FixedWindowRateLimiter(new()
        {
            Window = TimeSpan.FromSeconds(1),
            PermitLimit = 120,
            QueueLimit = 2400
        });

        this.logger = factory.CreateLogger("DSharpPlus.Net.Gateway.IGatewayClient - invalid shard");
    }

    /// <inheritdoc/>
    public async Task<GatewayConnectionFrame> ConnectAsync
    (
        string url,
        DiscordActivity? activity = null,
        DiscordUserStatus? status = null,
        DateTimeOffset? idleSince = null,
        ShardInfo? shardInfo = null
    )
    {
        this.reconnectUrl = url;
        this.activity = activity;
        this.status = status;
        this.idleSince = idleSince;

        this.closureRequested = false;

        this.gatewayTask = new();

        this.logger = shardInfo is null
            ? this.factory.CreateLogger("DSharpPlus.Net.Gateway.IGatewayClient")
            : this.factory.CreateLogger($"DSharpPlus.Net.Gateway.IGatewayClient - Shard {shardInfo.ShardId}");

        this.shardInfo = shardInfo;

        this.transportService.Initialize
        (
            shardInfo is null
                ? "DSharpPlus.Net.Gateway.ITransportService"
                : $"DSharpPlus.Net.Gateway.ITransportService - Shard {shardInfo.ShardId}",
            this.decompressor
        );

        for (uint i = 0; i < this.options.MaxReconnects; i++)
        {
            try
            {
                this.gatewayTokenSource = new();
                await this.transportService.ConnectAsync(url);

                TransportFrame initialFrame = await this.transportService.ReadAsync();
                GatewayPayload? helloEvent = await ProcessAndDeserializeTransportFrameAsync(initialFrame);

                if (helloEvent is not { OpCode: GatewayOpCode.Hello })
                {
                    this.logger.LogWarning("Expected HELLO payload from Discord");
                    continue;
                }

                GatewayHello helloPayload = ((JObject)helloEvent.Data).ToDiscordObject<GatewayHello>();

                this.logger.LogDebug
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

                await WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(identify)));

                this.logger.LogDebug("Identified with the Discord gateway");
                break;
            }
            catch (Exception e)
            {
                TimeSpan delay = this.options.GetReconnectionDelay(i);

                if (e is not WebSocketException { InnerException: HttpRequestException or SocketException })
                {
                    this.logger.LogError(exception: e, "Encountered an error while connecting, waiting for {delay} and retrying.", delay);
                }
                else
                {
                    this.logger.LogWarning("Severed internet connection detected, waiting for {delay} and retrying.", delay);
                }

                await Task.Delay(delay);
                continue;
            }
        }

        return await this.gatewayTask.Task;
    }

    /// <inheritdoc/>
    public async Task DisconnectAsync()
    {
        this.closureRequested = true;
        this.IsConnected = false;
        await this.gatewayTokenSource.CancelAsync();
        await this.transportService.DisconnectAsync(WebSocketCloseStatus.NormalClosure);
    }

    /// <inheritdoc/>
    public async Task WriteAsync(byte[] payload)
    {
        using RateLimitLease lease = await this.ratelimiter.AcquireAsync();

        try
        {
            await this.transportService.WriteAsync(payload);
        }
        catch (ObjectDisposedException) 
        {
            // we are in the middle of a reconnect
            this.logger.LogDebug("Attempted to send a payload across the gateway while reconnecting");
        }
        catch (InvalidOperationException)
        {
            // something perished. try reconnecting?
            this.logger.LogDebug("The connection entered an invalid state, reconnecting.");
            await TryResumeAsync();
        }
        catch (OperationCanceledException)
        {
            // either discord is being slow (this might be bad but in an outage all bets are off) or we got disconnected,
            // reconnect
            this.logger.LogWarning("The connection is excessively slow or dropped, reconnecting.");
            await TryResumeAsync();
        }
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
            try
            {
                await SendSingleHeartbeatAsync();

                if (this.pendingHeartbeats > 5)
                {
                    this.logger.LogInformation("The connection zombied, attempting to resume");
                    await TryResumeAsync();

                    return;
                }
            }
            catch (WebSocketException e)
            {
                this.logger.LogWarning("The connection died or entered an invalid state, reconnecting. Exception: {ExceptionMessage}", e.Message);
                await TryResumeAsync();

                return;
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "An error occurred while sending a heartbeat.");
                await TerminateCurrentFrameAsync(GatewayDisconnectReason.InternalError, exception: e);
            }
            
        } while (await timer.WaitForNextTickAsync(ct));
    }

    private async Task SendSingleHeartbeatAsync()
    {
        await WriteAsync
        (
            Encoding.UTF8.GetBytes
            (
                JsonConvert.SerializeObject
                (
                    new GatewayPayload
                    {
                        OpCode = GatewayOpCode.Heartbeat,
                        Data = this.lastReceivedSequence
                    }
                )
            )
        );

        this.logger.LogTrace("Heartbeat sent with sequence number {Sequence}.", this.lastReceivedSequence);

        this.lastSentHeartbeat = DateTimeOffset.UtcNow;
        this.pendingHeartbeats++;
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
                TransportFrame frame;

                try
                {
                    frame = await this.transportService.ReadAsync();
                }
                catch (InvalidOperationException)
                {
                    // we're in the middle of a reconnect, it's whatever
                    continue;
                }

                GatewayPayload? payload = await ProcessAndDeserializeTransportFrameAsync(frame);

                if (payload is null)
                {
                    continue;
                }

                this.lastReceivedSequence = payload.Sequence ?? this.lastReceivedSequence;

                await HandleEventCoreAsync(payload);
            }
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "An exception occurred in event handling.");
            await TerminateCurrentFrameAsync(GatewayDisconnectReason.InternalError, exception: e);
        }
    }

    private async Task HandleEventCoreAsync(GatewayPayload payload)
    {
        switch (payload.OpCode)
        {
            case GatewayOpCode.Dispatch when payload.EventName is "READY":

                ReadyPayload readyPayload = ((JObject)payload.Data).ToDiscordObject<ReadyPayload>();

                QueryUriBuilder resumeUrlBuilder = new(readyPayload.ResumeGatewayUrl);
                
                resumeUrlBuilder.AddParameter("v", "10")
                    .AddParameter("encoding", "json");

                if (this.decompressor.IsTransportCompression)
                {
                    resumeUrlBuilder.AddParameter("compress", this.decompressor.Name);
                }

                this.resumeUrl = resumeUrlBuilder.Build();
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

                this.logger.LogDebug("Received READY, the gateway is now operational.");

                break;

            case GatewayOpCode.HeartbeatAck:

                this.Ping = DateTimeOffset.UtcNow - this.lastSentHeartbeat;
                this.pendingHeartbeats = 0;

                return;

            case GatewayOpCode.Heartbeat:

                try
                {
                    await SendSingleHeartbeatAsync();
                }
                catch (InvalidOperationException)
                {
                    // ignore, race condition
                }
                catch (Exception e)
                {
                    await TerminateCurrentFrameAsync(GatewayDisconnectReason.InternalError, exception: e);
                }

                return;

            case GatewayOpCode.InvalidSession:

                this.logger.LogDebug("Received INVALID_SESSION, resumable: {Resumable}", (bool)payload.Data);
                bool success = (bool)payload.Data && await TryResumeAsync();

                if (!success)
                {
                    await TerminateCurrentFrameAsync(GatewayDisconnectReason.SessionInvalidated);
                }

                return;

            case GatewayOpCode.Reconnect:

                this.logger.LogDebug("Received RECONNECT, attempting to resume");
                await TryResumeAsync();

                return;
        }

        await this.eventWriter.WriteAsync(payload, CancellationToken.None);
    }

    /// <summary>
    /// Attempts to resume a connection, returning whether this was successful.
    /// </summary>
    private async Task<bool> TryResumeAsync()
    {
        if (this.resumeUrl is null || this.sessionId is null)
        {
            return false;
        }

        try
        {
            this.IsConnected = false;
            await this.gatewayTokenSource.CancelAsync();

            this.gatewayTokenSource = new();

            this.logger.LogDebug("Attempting to resume an existing gateway session.");

            await this.transportService.DisconnectAsync((WebSocketCloseStatus)4000);
            await this.transportService.ConnectAsync(this.resumeUrl);

            TransportFrame helloFrame = await this.transportService.ReadAsync();
            GatewayPayload? helloPayload = await ProcessAndDeserializeTransportFrameAsync(helloFrame);

            if (helloPayload is not { OpCode: GatewayOpCode.Hello })
            {
                this.logger.LogWarning("Received invalid opcode {op} while resuming", helloPayload.OpCode);
                await TerminateCurrentFrameAsync(GatewayDisconnectReason.UnknownError);

                return false;
            }

            GatewayHello hello = ((JObject)helloPayload.Data).ToDiscordObject<GatewayHello>();

            this.logger.LogDebug
            (
                "Received hello event, starting heartbeating with an interval of {interval} and resuming.",
                TimeSpan.FromMilliseconds(hello.HeartbeatInterval)
            );

            _ = HeartbeatAsync(hello.HeartbeatInterval, this.gatewayTokenSource.Token);

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

            while (true)
            {
                TransportFrame frame = await this.transportService.ReadAsync();
                GatewayPayload? payload = await ProcessAndDeserializeTransportFrameAsync(frame);

                if (payload is null)
                {
                    continue;
                }

                this.lastReceivedSequence = payload.Sequence ?? this.lastReceivedSequence;

                if (payload is { OpCode: GatewayOpCode.Dispatch, EventName: "RESUMED" })
                {
                    this.logger.LogDebug("Successfully resumed the previous gateway session.");

                    _ = HandleEventsAsync(this.gatewayTokenSource.Token);

                    return true;
                }
                else if (payload.OpCode == GatewayOpCode.InvalidSession)
                {
                    this.logger.LogDebug("The previous gateway session was invalidated, could not resume.");
                    await TerminateCurrentFrameAsync(GatewayDisconnectReason.SessionInvalidated);

                    return false;
                }

                await HandleEventCoreAsync(payload);
            }
        }
        // on windows, we get HttpRequestException, on linux either of SocketException or WebSocketException in these cases
        catch (WebSocketException e) when (e.InnerException is HttpRequestException or SocketException or WebSocketException)
        {
            this.logger.LogWarning("Internet connection interrupted.");
            await TerminateCurrentFrameAsync(GatewayDisconnectReason.ConnectionSevered);
            
            return false;
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to resume an existing gateway session.");
            await TerminateCurrentFrameAsync(GatewayDisconnectReason.InternalError, exception: e);

            return false;
        }
    }

    private async Task HandleErrorAndAttemptToResumeAsync(TransportFrame frame)
    {
        if(this.closureRequested)
        {
            this.logger.LogDebug("Connection was requested to be closed, ignoring any errors.");
            return;
        }

        if (frame.TryGetException<WebSocketException>(out _))
        {
            await TryResumeAsync();
        }
        else if (frame.TryGetErrorCode(out int errorCode))
        {
            this.logger.LogInformation("Received error code {Code} from gateway websocket.", errorCode);

            if (errorCode is 4004 or (>= 4010 and <= 4014))
            {
                this.logger.LogError("Irrecoverable close code {code} received.", errorCode);
                await TerminateCurrentFrameAsync(GatewayDisconnectReason.IrrecoverableCloseCode, closeCode: (GatewayCloseCode)errorCode);
            }
            
            bool success = errorCode switch
            {
                < 4000 => await HandleSystemErrorAsync(errorCode),
                (>= 4000 and <= 4002) or 4005 or 4008 => await TryResumeAsync(),
                _ => false
            };

            if (!success)
            {
                this.logger.LogDebug("An attempt to recover from close code {Code} failed.", errorCode);
                await TerminateCurrentFrameAsync(GatewayDisconnectReason.RecoverableCloseCode, closeCode: (GatewayCloseCode)errorCode);
            }
        }
    }


    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    private async ValueTask<GatewayPayload?> ProcessAndDeserializeTransportFrameAsync(TransportFrame frame)
    {
        GatewayPayload? payload;

        if (!frame.IsSuccess)
        {
            await HandleErrorAndAttemptToResumeAsync(frame);
            return null;
        }

        if (frame.TryGetMessage(out byte[]? message))
        {
            using MemoryStream memoryStream = new(message);
            using StreamReader reader = new(memoryStream);
            using JsonReader jsonReader = new JsonTextReader(reader);

            JsonSerializer serializer = new();
            payload = serializer.Deserialize<GatewayPayload>(jsonReader);

            if (payload is null)
            {
                this.logger.LogError("Received invalid inbound event: {Data}", Encoding.UTF8.GetString(message));
                return null;
            }
        }
        else
        {
            this.logger.LogCritical("Unrecognized transport frame encountered: {Frame}", frame);
            return null;
        }

        return payload;
    }

    private async Task<bool> HandleSystemErrorAsync(int errorCode)
    {
        // 3000 Unauthorized and 3003 Forbidden are a problem we can't fix, error and return to the user
        if (errorCode is 3000 or 3003)
        {
            this.logger.LogError("Encountered irrecoverable gateway close code {CloseCode}", errorCode);
            await TerminateCurrentFrameAsync(GatewayDisconnectReason.IrrecoverableCloseCode, closeCode: (GatewayCloseCode)errorCode);
            return false;
        }

        // else, try to reconnect if so requested
        if (!this.closureRequested)
        {
            return await TryResumeAsync();
        }

        this.logger.LogDebug("Gateway shutdown in progress, not reconnecting on recoverable close code {CloseCode}", errorCode);
        await TerminateCurrentFrameAsync(GatewayDisconnectReason.UserRequested);
        return false;
    }

    // this can be called however often we want in one teardown, only the first TrySetResult will succeed
    private async Task TerminateCurrentFrameAsync(GatewayDisconnectReason reason, Exception? exception = null, GatewayCloseCode? closeCode = null)
    {
        this.gatewayTask.TrySetResult(new()
        {
            DisconnectReason = reason,
            Exception = exception,
            CloseCode = closeCode,
            ShardId = this.ShardId
        });

        this.closureRequested = true;
        this.IsConnected = false;
        await this.gatewayTokenSource.CancelAsync();
    }

    /// <inheritdoc/>
    public async Task<GatewayConnectionFrame> ReconnectAsync()
    {
        await this.transportService.DisconnectAsync(WebSocketCloseStatus.Empty);

        return await ConnectAsync
        (
            this.reconnectUrl,
            this.activity,
            this.status,
            this.idleSince,
            this.shardInfo
        );
    }
}
