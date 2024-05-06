using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;
using DSharpPlus.Net.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus;

public sealed partial class DiscordClient
{
    #region Private Fields

    private int heartbeatInterval;
    private DateTimeOffset lastHeartbeat;

    internal static readonly DateTimeOffset discordEpoch = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private int skippedHeartbeats = 0;
    private long lastSequence;

    internal IWebSocketClient webSocketClient;
    private PayloadDecompressor payloadDecompressor;

    private CancellationTokenSource cancelTokenSource;
    private CancellationToken cancelToken;

    #endregion

    #region Connection Semaphore

    private static ConcurrentDictionary<ulong, SocketLock> socketLocks { get; } = new ConcurrentDictionary<ulong, SocketLock>();
    private ManualResetEventSlim sessionLock { get; } = new ManualResetEventSlim(true);

    #endregion

    #region Internal Connection Methods

    private Task InternalReconnectAsync(bool startNewSession = false, int code = 1000, string message = "")
    {
        if (startNewSession)
        {
            this.sessionId = null;
        }

        _ = this.webSocketClient.DisconnectAsync(code, message);
        return Task.CompletedTask;
    }

    /* GATEWAY VERSION IS IN THIS METHOD!! If you need to update the Gateway Version, look for gwuri ~Velvet */
    internal async Task InternalConnectAsync()
    {
        SocketLock socketLock = null;
        try
        {
            if (this.GatewayInfo == null)
            {
                await InternalUpdateGatewayAsync();
            }

            await InitializeAsync();

            socketLock = GetSocketLock();
            await socketLock.LockAsync();
        }
        catch
        {
            socketLock?.UnlockAfter(TimeSpan.Zero);
            throw;
        }

        if (!this.Presences.ContainsKey(this.CurrentUser.Id))
        {
            this.presences[this.CurrentUser.Id] = new DiscordPresence
            {
                Discord = this,
                RawActivity = new TransportActivity(),
                Activity = new DiscordActivity(),
                Status = DiscordUserStatus.Online,
                InternalUser = new TransportUser
                {
                    Id = this.CurrentUser.Id,
                    Username = this.CurrentUser.Username,
                    Discriminator = this.CurrentUser.Discriminator,
                    AvatarHash = this.CurrentUser.AvatarHash
                }
            };
        }
        else
        {
            DiscordPresence pr = this.presences[this.CurrentUser.Id];
            pr.RawActivity = new TransportActivity();
            pr.Activity = new DiscordActivity();
            pr.Status = DiscordUserStatus.Online;
        }

        Volatile.Write(ref this.skippedHeartbeats, 0);

        this.webSocketClient = this.Configuration.WebSocketClientFactory(this.Configuration.Proxy);
        this.payloadDecompressor = this.Configuration.GatewayCompressionLevel != GatewayCompressionLevel.None
            ? new PayloadDecompressor(this.Configuration.GatewayCompressionLevel)
            : null;

        this.cancelTokenSource = new CancellationTokenSource();
        this.cancelToken = this.cancelTokenSource.Token;

        this.webSocketClient.Connected += SocketOnConnect;
        this.webSocketClient.Disconnected += SocketOnDisconnect;
        this.webSocketClient.MessageReceived += SocketOnMessage;
        this.webSocketClient.ExceptionThrown += SocketOnException;

        QueryUriBuilder gwuri;

        if (this.gatewayResumeUrl is not null && !string.IsNullOrWhiteSpace(this.sessionId))
        {
            gwuri = new QueryUriBuilder(this.gatewayResumeUrl);
        }
        else
        {
            gwuri = new QueryUriBuilder(this.GatewayUri.ToString());
        }
        
        gwuri.AddParameter("v", "10")
             .AddParameter("encoding", "json");
        
        if (this.Configuration.GatewayCompressionLevel == GatewayCompressionLevel.Stream)
        {
            gwuri.AddParameter("compress", "zlib-stream");
        }

        await this.webSocketClient.ConnectAsync(new Uri(gwuri.Build()));

        Task SocketOnConnect(IWebSocketClient sender, SocketEventArgs e)
        {
            return this.socketOpened.InvokeAsync(this, e);
        }

        async Task SocketOnMessage(IWebSocketClient sender, SocketMessageEventArgs e)
        {
            string msg = null;
            if (e is SocketTextMessageEventArgs etext)
            {
                msg = etext.Message;
            }
            else if (e is SocketBinaryMessageEventArgs ebin) // :DDDD
            {
                using MemoryStream ms = new();
                if (!this.payloadDecompressor.TryDecompress(new ArraySegment<byte>(ebin.Message), ms))
                {
                    this.Logger.LogError(LoggerEvents.WebSocketReceiveFailure, "Payload decompression failed");
                    return;
                }

                ms.Position = 0;
                using StreamReader sr = new(ms, Utilities.UTF8);
                msg = await sr.ReadToEndAsync();
            }

            try
            {
                this.Logger.LogTrace(LoggerEvents.GatewayWsRx, "{WebsocketPayload}", msg);
                await HandleSocketMessageAsync(msg);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(LoggerEvents.WebSocketReceiveFailure, ex, "Socket handler suppressed an exception");
            }
        }

        Task SocketOnException(IWebSocketClient sender, SocketErrorEventArgs e)
        {
            return this.socketErrored.InvokeAsync(this, e);
        }

        async Task SocketOnDisconnect(IWebSocketClient sender, SocketCloseEventArgs e)
        {
            // release session and connection
            this.ConnectionLock.Set();
            this.sessionLock.Set();

            if (!this.disposed)
            {
                this.cancelTokenSource.Cancel();
            }

            this.Logger.LogDebug(LoggerEvents.ConnectionClose, "Connection closed ({CloseCode}, '{CloseMessage}')", e.CloseCode, e.CloseMessage);
            await this.socketClosed.InvokeAsync(this, e);

            if (this.Configuration.AutoReconnect && (e.CloseCode <= 4003 || (e.CloseCode >= 4005 && e.CloseCode <= 4009) || e.CloseCode >= 5000))
            {
                this.Logger.LogCritical(LoggerEvents.ConnectionClose, "Connection terminated ({CloseCode}, '{CloseMessage}'), reconnecting", e.CloseCode, e.CloseMessage);

                if (this.status == null)
                {
                    await ConnectAsync();
                }
                else
                    if (this.status.IdleSince.HasValue)
                {
                    await ConnectAsync(this.status.activity, this.status.Status, Utilities.GetDateTimeOffsetFromMilliseconds(this.status.IdleSince.Value));
                }
                else
                {
                    await ConnectAsync(this.status.activity, this.status.Status);
                }
            }
            else
            {
                this.Logger.LogInformation(LoggerEvents.ConnectionClose, "Connection terminated ({CloseCode}, '{CloseMessage}')", e.CloseCode, e.CloseMessage);
            }
        }
    }

    #endregion

    #region WebSocket (Events)

    internal async Task HandleSocketMessageAsync(string data)
    {
        GatewayPayload? payload = JsonConvert.DeserializeObject<GatewayPayload>(data);
        this.lastSequence = payload.Sequence ?? this.lastSequence;
        switch (payload.OpCode)
        {
            case GatewayOpCode.Dispatch:
                await HandleDispatchAsync(payload);
                break;

            case GatewayOpCode.Heartbeat:
                await OnHeartbeatAsync((long)payload.Data);
                break;

            case GatewayOpCode.Reconnect:
                await OnReconnectAsync();
                break;

            case GatewayOpCode.InvalidSession:
                await OnInvalidateSessionAsync((bool)payload.Data);
                break;

            case GatewayOpCode.Hello:
                await OnHelloAsync((payload.Data as JObject).ToDiscordObject<GatewayHello>());
                break;

            case GatewayOpCode.HeartbeatAck:
                await OnHeartbeatAckAsync();
                break;

            default:
                this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Unknown Discord opcode: {Op}\nPayload: {Payload}", payload.OpCode, payload.Data);
                break;
        }
    }

    internal async Task OnHeartbeatAsync(long seq)
    {
        this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HEARTBEAT (OP1)");
        await SendHeartbeatAsync(seq);
    }

    internal async Task OnReconnectAsync()
    {
        this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received RECONNECT (OP7)");
        await InternalReconnectAsync(code: 4000, message: "OP7 acknowledged");
    }

    internal async Task OnInvalidateSessionAsync(bool data)
    {
        // begin a session if one is not open already
        if (this.sessionLock.Wait(0))
        {
            this.sessionLock.Reset();
        }

        // we are sending a fresh resume/identify, so lock the socket
        SocketLock socketLock = GetSocketLock();
        await socketLock.LockAsync();
        socketLock.UnlockAfter(TimeSpan.FromSeconds(5));

        if (data)
        {
            this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received INVALID_SESSION (OP9, true)");
            await Task.Delay(6000);
            await SendResumeAsync();
        }
        else
        {
            this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received INVALID_SESSION (OP9, false)");
            this.sessionId = null;
            await SendIdentifyAsync(this.status);
        }
    }

    internal async Task OnHelloAsync(GatewayHello hello)
    {
        this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HELLO (OP10)");

        if (this.sessionLock.Wait(0))
        {
            this.sessionLock.Reset();
            GetSocketLock().UnlockAfter(TimeSpan.FromSeconds(5));
        }
        else
        {
            this.Logger.LogWarning(LoggerEvents.SessionUpdate, "Attempt to start a session while another session is active");
            return;
        }

        Interlocked.CompareExchange(ref this.skippedHeartbeats, 0, 0);
        this.heartbeatInterval = hello.HeartbeatInterval;
        _ = Task.Run(HeartbeatLoopAsync, this.cancelToken);

        if (string.IsNullOrEmpty(this.sessionId))
        {
            await SendIdentifyAsync(this.status);
        }
        else
        {
            await SendResumeAsync();
        }
    }

    internal async Task OnHeartbeatAckAsync()
    {
        Interlocked.Decrement(ref this.skippedHeartbeats);

        int ping = (int)(DateTime.Now - this.lastHeartbeat).TotalMilliseconds;

        this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HEARTBEAT_ACK (OP11, {Heartbeat}ms)", ping);

        Volatile.Write(ref this.ping, ping);

        HeartbeatEventArgs args = new()
        {
            Ping = this.Ping,
            Timestamp = DateTimeOffset.Now
        };

        await this.heartbeated.InvokeAsync(this, args);
    }

    internal async Task HeartbeatLoopAsync()
    {
        this.Logger.LogDebug(LoggerEvents.Heartbeat, "Heartbeat task started");
        CancellationToken token = this.cancelToken;
        try
        {
            while (true)
            {
                await SendHeartbeatAsync(this.lastSequence);
                await Task.Delay(this.heartbeatInterval, token);
                token.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException) { }
    }

    #endregion

    #region Internal Gateway Methods

    internal async Task InternalUpdateStatusAsync(DiscordActivity activity, DiscordUserStatus? userStatus, DateTimeOffset? idleSince)
    {
        if (activity != null && activity.Name != null && activity.Name.Length > 128)
        {
            throw new Exception("Game name can't be longer than 128 characters!");
        }

        long? since_unix = idleSince != null ? Utilities.GetUnixTime(idleSince.Value) : null;
        DiscordActivity act = activity ?? new DiscordActivity();

        StatusUpdate status = new()
        {
            Activity = new TransportActivity(act),
            IdleSince = since_unix,
            IsAFK = idleSince != null,
            Status = userStatus ?? DiscordUserStatus.Online
        };

        // Solution to have status persist between sessions
        this.status = status;
        GatewayPayload status_update = new()
        {
            OpCode = GatewayOpCode.StatusUpdate,
            Data = status
        };

        string statusstr = JsonConvert.SerializeObject(status_update);

        await SendRawPayloadAsync(statusstr);

        if (!this.presences.TryGetValue(this.CurrentUser.Id, out DiscordPresence pr))
        {
            this.presences[this.CurrentUser.Id] = new DiscordPresence
            {
                Discord = this,
                Activity = act,
                Status = userStatus ?? DiscordUserStatus.Online,
                InternalUser = new TransportUser { Id = this.CurrentUser.Id }
            };
        }
        else
        {
            pr.Activity = act;
            pr.Status = userStatus ?? pr.Status;
        }
    }

    internal async Task SendHeartbeatAsync(long seq)
    {
        bool more_than_5 = Volatile.Read(ref this.skippedHeartbeats) > 5;
        bool guilds_comp = Volatile.Read(ref this.guildDownloadCompleted);

        if (guilds_comp && more_than_5)
        {
            this.Logger.LogCritical(LoggerEvents.HeartbeatFailure, "Server failed to acknowledge more than 5 heartbeats - connection is zombie");

            ZombiedEventArgs args = new()
            {
                Failures = Volatile.Read(ref this.skippedHeartbeats),
                GuildDownloadCompleted = true
            };
            await this.zombied.InvokeAsync(this, args);

            await InternalReconnectAsync(code: 4001, message: "Too many heartbeats missed");

            return;
        }

        if (!guilds_comp && more_than_5)
        {
            ZombiedEventArgs args = new()
            {
                Failures = Volatile.Read(ref this.skippedHeartbeats),
                GuildDownloadCompleted = false
            };
            await this.zombied.InvokeAsync(this, args);

            this.Logger.LogWarning(LoggerEvents.HeartbeatFailure, "Server failed to acknowledge more than 5 heartbeats, but the guild download is still running - check your connection speed");
        }

        Volatile.Write(ref this.lastSequence, seq);
        this.Logger.LogTrace(LoggerEvents.Heartbeat, "Sending heartbeat");
        GatewayPayload heartbeat = new()
        {
            OpCode = GatewayOpCode.Heartbeat,
            Data = seq
        };
        string heartbeat_str = JsonConvert.SerializeObject(heartbeat);
        await SendRawPayloadAsync(heartbeat_str);

        this.lastHeartbeat = DateTimeOffset.Now;

        Interlocked.Increment(ref this.skippedHeartbeats);
    }

    internal async Task SendIdentifyAsync(StatusUpdate status)
    {
        GatewayIdentify identify = new()
        {
            Token = Utilities.GetFormattedToken(this),
            Compress = this.Configuration.GatewayCompressionLevel == GatewayCompressionLevel.Payload,
            LargeThreshold = this.Configuration.LargeThreshold,
            ShardInfo = new ShardInfo
            {
                ShardId = this.Configuration.ShardId,
                ShardCount = this.Configuration.ShardCount
            },
            Presence = status,
            Intents = this.Configuration.Intents
        };
        GatewayPayload payload = new()
        {
            OpCode = GatewayOpCode.Identify,
            Data = identify
        };
        string payloadstr = JsonConvert.SerializeObject(payload);
        await SendRawPayloadAsync(payloadstr);

        this.Logger.LogDebug(LoggerEvents.Intents, "Registered gateway intents ({Intents})", this.Configuration.Intents);
    }

    internal async Task SendResumeAsync()
    {
        GatewayResume resume = new()
        {
            Token = Utilities.GetFormattedToken(this),
            SessionId = this.sessionId,
            SequenceNumber = Volatile.Read(ref this.lastSequence)
        };
        GatewayPayload resume_payload = new()
        {
            OpCode = GatewayOpCode.Resume,
            Data = resume
        };
        string resumestr = JsonConvert.SerializeObject(resume_payload);

        await SendRawPayloadAsync(resumestr);
    }

    internal async Task InternalUpdateGatewayAsync()
    {
        Net.GatewayInfo info = await GetGatewayInfoAsync();
        this.GatewayInfo = info;
        this.GatewayUri = new Uri(info.Url);
    }

    internal async Task SendRawPayloadAsync(string jsonPayload)
    {
        this.Logger.LogTrace(LoggerEvents.GatewayWsTx, "{WebsocketPayload}", jsonPayload);
        await this.webSocketClient.SendMessageAsync(jsonPayload);
    }

    /// <summary>
    /// Sends a raw payload to the gateway. This method is not recommended for use unless you know what you're doing.
    /// </summary>
    /// <param name="opCode">The opcode to send to the Discord gateway.</param>
    /// <param name="data">The data to deserialize.</param>
    /// <typeparam name="T">The type of data that the object belongs to.</typeparam>
    /// <returns>A task representing the payload being sent.</returns>
    [Obsolete("This method should not be used unless you know what you're doing. Instead, look towards the other explicitly implemented methods which come with client-side validation.")]
    public Task SendPayloadAsync<T>(GatewayOpCode opCode, T data) => SendPayloadAsync(opCode, (object?)data);

    /// <inheritdoc cref="SendPayloadAsync{T}(GatewayOpCode, T)"/>
    [Obsolete("This method should not be used unless you know what you're doing. Instead, look towards the other explicitly implemented methods which come with client-side validation.")]
    public Task SendPayloadAsync(GatewayOpCode opCode, object? data = null)
    {
        GatewayPayload payload = new()
        {
            OpCode = opCode,
            Data = data
        };

        string payloadString = DiscordJson.SerializeObject(payload);
        return SendRawPayloadAsync(payloadString);
    }
    #endregion

    #region Semaphore Methods

    private SocketLock GetSocketLock()
        => socketLocks.GetOrAdd(this.CurrentApplication.Id, appId => new SocketLock(appId, this.GatewayInfo.SessionBucket.MaxConcurrency));

    #endregion
}
