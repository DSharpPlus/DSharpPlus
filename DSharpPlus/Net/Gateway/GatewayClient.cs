using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace DSharpPlus.Net.Gateway;

/// <inheritdoc/>
public sealed class GatewayClient : IGatewayClient
{
    private readonly ILogger<IGatewayClient> logger;
    private readonly ITransportService transportService;
    private readonly ChannelWriter<GatewayPayload> eventWriter;
    private readonly GatewayClientOptions options;
    private readonly string token;
    private readonly bool compress;

    private int remainingOutboundPayloads = 120;
    private DateTimeOffset lastOutboundPayloadReset = DateTimeOffset.UtcNow;
    private SpinLock resetLock = new();

    private int lastReceivedSequence = 0;
    private string? resumeUrl;
    private string? sessionId;
    private string? reconnectUrl;

    private GatewayIdentify? identify;
    private CancellationTokenSource gatewayTokenSource;

    public GatewayClient
    (
        [FromKeyedServices("DSharpPlus.Gateway.EventChannel")]
        Channel<GatewayPayload> eventChannel,
        
        ITransportService transportService,
        ILogger<IGatewayClient> logger,
        IOptions<TokenContainer> tokenContainer,
        PayloadDecompressor decompressor,
        IOptions<GatewayClientOptions> options
    )
    {
        this.transportService = transportService;
        this.eventWriter = eventChannel.Writer;
        this.logger = logger;
        this.token = tokenContainer.Value.GetToken();
        this.gatewayTokenSource = new();
        this.compress = decompressor.CompressionLevel == GatewayCompressionLevel.Payload;
        this.options = options.Value;
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
        this.reconnectUrl = url;
        await this.transportService.ConnectAsync(url);

        TransportFrame initialFrame = await this.transportService.ReadAsync();

        if (!initialFrame.TryGetMessage(out string? hello))
        {
            await HandleErrorAndAttemptToReconnectAsync(initialFrame);
        }

        GatewayPayload? helloEvent = JsonConvert.DeserializeObject<GatewayPayload>(hello);

        if (helloEvent is not { OpCode: GatewayOpCode.Hello, Data: GatewayHello helloPayload })
        {
            throw new InvalidDataException($"Expected HELLO payload from Discord, received {hello}");
        }

        this.logger.LogTrace
        (
            "Received hello event, starting heartbeating with an interval of {interval} and identifying.",
            TimeSpan.FromMilliseconds(helloPayload.HeartbeatInterval)
        );

        _ = HeartbeatAsync(helloPayload.HeartbeatInterval, this.gatewayTokenSource.Token);
        _ = HandleEventsAsync(this.gatewayTokenSource.Token);

        GatewayIdentify identify = new()
        {
            Token = this.token,
            Compress = this.compress,
            LargeThreshold = this.options.LargeThreshold,
            ShardInfo = shardInfo,
            Presence = new()
            {
                Activity = new TransportActivity(activity),
                Status = status ?? DiscordUserStatus.Online,
                IdleSince = idleSince?.ToUnixTimeMilliseconds()
            },
            Intents = this.options.Intents
        };

        this.identify = identify;

        await WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(identify)));

        this.logger.LogTrace("Identified with the Discord gateway");
    }

    /// <inheritdoc/>
    public async ValueTask DisconnectAsync()
    {
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
        } while (await timer.WaitForNextTickAsync(ct));
    }

    /// <summary>
    /// Handles events incoming from Discord.
    /// </summary>
    private async Task HandleEventsAsync(CancellationToken ct)
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

                    ReadyPayload readyPayload = (ReadyPayload)payload.Data;
                    this.resumeUrl = readyPayload.ResumeGatewayUrl;
                    this.sessionId = readyPayload.SessionId;

                    this.logger.LogTrace("Received READY, the gateway is now operational.");

                    break;

                // TODO: heartbeat tracking
                case GatewayOpCode.HeartbeatAck:
                    break;

                case GatewayOpCode.InvalidSession:

                    this.logger.LogTrace("Received INVALID_SESSION, resumable: {Resumable}", (bool)payload.Data);
                    bool success = (bool)payload.Data ? await TryResumeAsync() : await TryReconnectAsync();

                    if (!success)
                    {
                        this.logger.LogError("The session was invalidated and resuming/reconnecting failed.");
                    }

                    break;

                case GatewayOpCode.Reconnect:

                    this.logger.LogTrace("Received RECONNECT");
                    
                    if (!await TryReconnectAsync())
                    {
                        this.logger.LogError("A reconnection attempt requested by Discord failed.");
                    }

                    break;
            }

            await this.eventWriter.WriteAsync(payload, ct);
        }

        this.eventWriter.Complete();
    }

    /// <summary>
    /// Attempts to resume a connection, returning whether this was successful.
    /// </summary>
    private async Task<bool> TryResumeAsync()
    {
        if (this.resumeUrl is null || this.sessionId is null)
        {
            return await TryReconnectAsync();
        }

        try
        {
            await this.transportService.DisconnectAsync(WebSocketCloseStatus.NormalClosure);
            await this.transportService.ConnectAsync(this.resumeUrl);
            await WriteAsync
            (
                Encoding.UTF8.GetBytes
                (
                    JsonConvert.SerializeObject
                    (
                        new GatewayResume
                        {
                            SequenceNumber = this.lastReceivedSequence,
                            Token = this.token,
                            SessionId = this.sessionId
                        }
                    )
                )
            );

            this.logger.LogTrace("Resumed an existing gateway session.");
        }
        catch
        {
            return await TryReconnectAsync();
        }

        return true;
    }

    /// <summary>
    /// Attempts to reconnect to the gateway, returning whether this was successful.
    /// </summary>
    private async Task<bool> TryReconnectAsync()
    {
        try
        {
            this.gatewayTokenSource.Cancel();
            this.gatewayTokenSource = new();
            await this.transportService.DisconnectAsync(WebSocketCloseStatus.NormalClosure);
            await this.transportService.ConnectAsync(this.reconnectUrl!);

            TransportFrame initialFrame = await this.transportService.ReadAsync();

            if (!initialFrame.TryGetMessage(out string? hello))
            {
                await HandleErrorAndAttemptToReconnectAsync(initialFrame);
            }

            GatewayPayload? helloEvent = JsonConvert.DeserializeObject<GatewayPayload>(hello);

            if (helloEvent is not { OpCode: GatewayOpCode.Hello, Data: GatewayHello helloPayload })
            {
                throw new InvalidDataException($"Expected HELLO payload from Discord, received {hello}");
            }

            this.logger.LogTrace
            (
                "Received hello event, starting heartbeating with an interval of {interval} and identifying.",
                TimeSpan.FromMilliseconds(helloPayload.HeartbeatInterval)
            );

            _ = HeartbeatAsync(helloPayload.HeartbeatInterval, this.gatewayTokenSource.Token);
            _ = HandleEventsAsync(this.gatewayTokenSource.Token);

            await WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.identify)));

            this.logger.LogTrace("Identified with the Discord gateway");
        }
        catch
        {
            return false;
        }

        return true;
    }

    private async Task HandleErrorAndAttemptToReconnectAsync(TransportFrame frame)
    {
        if (frame.TryGetException<WebSocketException>(out _))
        {
            await TryResumeAsync();
        }
        else if (frame.TryGetException(out _))
        {
            await TryReconnectAsync();
        }
        else if (frame.TryGetErrorCode(out int errorCode))
        {
            _ = errorCode switch
            {
                >= 4000 and <= 4003 => await TryResumeAsync(),
                >= 4005 and <= 4009 => await TryResumeAsync(),
                _ => await TryReconnectAsync()
            };
        }
    }
}
