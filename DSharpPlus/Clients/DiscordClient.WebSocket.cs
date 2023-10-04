using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;
using DSharpPlus.Net.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus
{
    public sealed partial class DiscordClient
    {
        #region Private Fields

        private int _heartbeatInterval;
        private DateTimeOffset _lastHeartbeat;
        private Task _heartbeatTask;

        internal static readonly DateTimeOffset _discordEpoch = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private int _skippedHeartbeats = 0;
        private long _lastSequence;

        internal IWebSocketClient _webSocketClient;
        private PayloadDecompressor _payloadDecompressor;

        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;

        #endregion

        #region Connection Semaphore

        private static ConcurrentDictionary<ulong, SocketLock> _socketLocks { get; } = new ConcurrentDictionary<ulong, SocketLock>();
        private ManualResetEventSlim _sessionLock { get; } = new ManualResetEventSlim(true);

        #endregion

        #region Internal Connection Methods

        private Task InternalReconnectAsync(bool startNewSession = false, int code = 1000, string message = "")
        {
            if (startNewSession)
                this._sessionId = null;

            _ = this._webSocketClient.DisconnectAsync(code, message);
            return Task.CompletedTask;
        }

        /* GATEWAY VERSION IS IN THIS METHOD!! If you need to update the Gateway Version, look for gwuri ~Velvet */
        internal async Task InternalConnectAsync()
        {
            SocketLock socketLock = null;
            try
            {
                if (this.GatewayInfo == null)
                    await this.InternalUpdateGatewayAsync();
                await this.InitializeAsync();

                socketLock = this.GetSocketLock();
                await socketLock.LockAsync();
            }
            catch
            {
                socketLock?.UnlockAfter(TimeSpan.Zero);
                throw;
            }

            if (!this.Presences.ContainsKey(this.CurrentUser.Id))
            {
                this._presences[this.CurrentUser.Id] = new DiscordPresence
                {
                    Discord = this,
                    RawActivity = new TransportActivity(),
                    Activity = new DiscordActivity(),
                    Status = UserStatus.Online,
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
                var pr = this._presences[this.CurrentUser.Id];
                pr.RawActivity = new TransportActivity();
                pr.Activity = new DiscordActivity();
                pr.Status = UserStatus.Online;
            }

            Volatile.Write(ref this._skippedHeartbeats, 0);

            this._webSocketClient = this.Configuration.WebSocketClientFactory(this.Configuration.Proxy);
            this._payloadDecompressor = this.Configuration.GatewayCompressionLevel != GatewayCompressionLevel.None
                ? new PayloadDecompressor(this.Configuration.GatewayCompressionLevel)
                : null;

            this._cancelTokenSource = new CancellationTokenSource();
            this._cancelToken = this._cancelTokenSource.Token;

            this._webSocketClient.Connected += SocketOnConnect;
            this._webSocketClient.Disconnected += SocketOnDisconnect;
            this._webSocketClient.MessageReceived += SocketOnMessage;
            this._webSocketClient.ExceptionThrown += SocketOnException;

            QueryUriBuilder gwuri = new QueryUriBuilder(this.GatewayUri.ToString())
                .AddParameter("v", "10")
                .AddParameter("encoding", "json");

            if (this.Configuration.GatewayCompressionLevel == GatewayCompressionLevel.Stream)
            {
                gwuri.AddParameter("compress", "zlib-stream");
            }

            await this._webSocketClient.ConnectAsync(new Uri(gwuri.Build()));

            Task SocketOnConnect(IWebSocketClient sender, SocketEventArgs e)
                => this._socketOpened.InvokeAsync(this, e);

            async Task SocketOnMessage(IWebSocketClient sender, SocketMessageEventArgs e)
            {
                string msg = null;
                if (e is SocketTextMessageEventArgs etext)
                {
                    msg = etext.Message;
                }
                else if (e is SocketBinaryMessageEventArgs ebin) // :DDDD
                {
                    using var ms = new MemoryStream();
                    if (!this._payloadDecompressor.TryDecompress(new ArraySegment<byte>(ebin.Message), ms))
                    {
                        this.Logger.LogError(LoggerEvents.WebSocketReceiveFailure, "Payload decompression failed");
                        return;
                    }

                    ms.Position = 0;
                    using var sr = new StreamReader(ms, Utilities.UTF8);
                    msg = await sr.ReadToEndAsync();
                }

                try
                {
                    this.Logger.LogTrace(LoggerEvents.GatewayWsRx, msg);
                    await this.HandleSocketMessageAsync(msg);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(LoggerEvents.WebSocketReceiveFailure, ex, "Socket handler suppressed an exception");
                }
            }

            Task SocketOnException(IWebSocketClient sender, SocketErrorEventArgs e)
                => this._socketErrored.InvokeAsync(this, e);

            async Task SocketOnDisconnect(IWebSocketClient sender, SocketCloseEventArgs e)
            {
                // release session and connection
                this.ConnectionLock.Set();
                this._sessionLock.Set();

                if (!this._disposed)
                    this._cancelTokenSource.Cancel();

                this.Logger.LogDebug(LoggerEvents.ConnectionClose, "Connection closed ({CloseCode}, '{CloseMessage}')", e.CloseCode, e.CloseMessage);
                await this._socketClosed.InvokeAsync(this, e);

                if (this.Configuration.AutoReconnect && (e.CloseCode <= 4003 || (e.CloseCode >= 4005 && e.CloseCode <= 4009) || e.CloseCode >= 5000))
                {
                    this.Logger.LogCritical(LoggerEvents.ConnectionClose, "Connection terminated ({CloseCode}, '{CloseMessage}'), reconnecting", e.CloseCode, e.CloseMessage);

                    if (this._status == null)
                        await this.ConnectAsync();
                    else
                        if (this._status.IdleSince.HasValue)
                        await this.ConnectAsync(this._status._activity, this._status.Status, Utilities.GetDateTimeOffsetFromMilliseconds(this._status.IdleSince.Value));
                    else
                        await this.ConnectAsync(this._status._activity, this._status.Status);
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
            var payload = JsonConvert.DeserializeObject<GatewayPayload>(data);
            this._lastSequence = payload.Sequence ?? this._lastSequence;
            switch (payload.OpCode)
            {
                case GatewayOpCode.Dispatch:
                    await this.HandleDispatchAsync(payload);
                    break;

                case GatewayOpCode.Heartbeat:
                    await this.OnHeartbeatAsync((long)payload.Data);
                    break;

                case GatewayOpCode.Reconnect:
                    await this.OnReconnectAsync();
                    break;

                case GatewayOpCode.InvalidSession:
                    await this.OnInvalidateSessionAsync((bool)payload.Data);
                    break;

                case GatewayOpCode.Hello:
                    await this.OnHelloAsync((payload.Data as JObject).ToDiscordObject<GatewayHello>());
                    break;

                case GatewayOpCode.HeartbeatAck:
                    await this.OnHeartbeatAckAsync();
                    break;

                default:
                    this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Unknown Discord opcode: {Op}\nPayload: {Payload}", payload.OpCode, payload.Data);
                    break;
            }
        }

        internal async Task OnHeartbeatAsync(long seq)
        {
            this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HEARTBEAT (OP1)");
            await this.SendHeartbeatAsync(seq);
        }

        internal async Task OnReconnectAsync()
        {
            this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received RECONNECT (OP7)");
            await this.InternalReconnectAsync(code: 4000, message: "OP7 acknowledged");
        }

        internal async Task OnInvalidateSessionAsync(bool data)
        {
            // begin a session if one is not open already
            if (this._sessionLock.Wait(0))
                this._sessionLock.Reset();

            // we are sending a fresh resume/identify, so lock the socket
            var socketLock = this.GetSocketLock();
            await socketLock.LockAsync();
            socketLock.UnlockAfter(TimeSpan.FromSeconds(5));

            if (data)
            {
                this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received INVALID_SESSION (OP9, true)");
                await Task.Delay(6000);
                await this.SendResumeAsync();
            }
            else
            {
                this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received INVALID_SESSION (OP9, false)");
                this._sessionId = null;
                await this.SendIdentifyAsync(this._status);
            }
        }

        internal async Task OnHelloAsync(GatewayHello hello)
        {
            this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HELLO (OP10)");

            if (this._sessionLock.Wait(0))
            {
                this._sessionLock.Reset();
                this.GetSocketLock().UnlockAfter(TimeSpan.FromSeconds(5));
            }
            else
            {
                this.Logger.LogWarning(LoggerEvents.SessionUpdate, "Attempt to start a session while another session is active");
                return;
            }

            Interlocked.CompareExchange(ref this._skippedHeartbeats, 0, 0);
            this._heartbeatInterval = hello.HeartbeatInterval;
            this._heartbeatTask = Task.Run(this.HeartbeatLoopAsync, this._cancelToken);

            if (string.IsNullOrEmpty(this._sessionId))
                await this.SendIdentifyAsync(this._status);
            else
                await this.SendResumeAsync();
        }

        internal async Task OnHeartbeatAckAsync()
        {
            Interlocked.Decrement(ref this._skippedHeartbeats);

            var ping = (int)(DateTime.Now - this._lastHeartbeat).TotalMilliseconds;

            this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HEARTBEAT_ACK (OP11, {Heartbeat}ms)", ping);

            Volatile.Write(ref this._ping, ping);

            var args = new HeartbeatEventArgs
            {
                Ping = this.Ping,
                Timestamp = DateTimeOffset.Now
            };

            await this._heartbeated.InvokeAsync(this, args);
        }

        internal async Task HeartbeatLoopAsync()
        {
            this.Logger.LogDebug(LoggerEvents.Heartbeat, "Heartbeat task started");
            var token = this._cancelToken;
            try
            {
                while (true)
                {
                    await this.SendHeartbeatAsync(this._lastSequence);
                    await Task.Delay(this._heartbeatInterval, token);
                    token.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException) { }
        }

        #endregion

        #region Internal Gateway Methods

        internal async Task InternalUpdateStatusAsync(DiscordActivity activity, UserStatus? userStatus, DateTimeOffset? idleSince)
        {
            if (activity != null && activity.Name != null && activity.Name.Length > 128)
                throw new Exception("Game name can't be longer than 128 characters!");

            var since_unix = idleSince != null ? (long?)Utilities.GetUnixTime(idleSince.Value) : null;
            var act = activity ?? new DiscordActivity();

            var status = new StatusUpdate
            {
                Activity = new TransportActivity(act),
                IdleSince = since_unix,
                IsAFK = idleSince != null,
                Status = userStatus ?? UserStatus.Online
            };

            // Solution to have status persist between sessions
            this._status = status;
            var status_update = new GatewayPayload
            {
                OpCode = GatewayOpCode.StatusUpdate,
                Data = status
            };

            var statusstr = JsonConvert.SerializeObject(status_update);

            await this.SendRawPayloadAsync(statusstr);

            if (!this._presences.ContainsKey(this.CurrentUser.Id))
            {
                this._presences[this.CurrentUser.Id] = new DiscordPresence
                {
                    Discord = this,
                    Activity = act,
                    Status = userStatus ?? UserStatus.Online,
                    InternalUser = new TransportUser { Id = this.CurrentUser.Id }
                };
            }
            else
            {
                var pr = this._presences[this.CurrentUser.Id];
                pr.Activity = act;
                pr.Status = userStatus ?? pr.Status;
            }
        }

        internal async Task SendHeartbeatAsync(long seq)
        {
            var more_than_5 = Volatile.Read(ref this._skippedHeartbeats) > 5;
            var guilds_comp = Volatile.Read(ref this._guildDownloadCompleted);

            if (guilds_comp && more_than_5)
            {
                this.Logger.LogCritical(LoggerEvents.HeartbeatFailure, "Server failed to acknowledge more than 5 heartbeats - connection is zombie");

                var args = new ZombiedEventArgs
                {
                    Failures = Volatile.Read(ref this._skippedHeartbeats),
                    GuildDownloadCompleted = true
                };
                await this._zombied.InvokeAsync(this, args);

                await this.InternalReconnectAsync(code: 4001, message: "Too many heartbeats missed");

                return;
            }

            if (!guilds_comp && more_than_5)
            {
                var args = new ZombiedEventArgs
                {
                    Failures = Volatile.Read(ref this._skippedHeartbeats),
                    GuildDownloadCompleted = false
                };
                await this._zombied.InvokeAsync(this, args);

                this.Logger.LogWarning(LoggerEvents.HeartbeatFailure, "Server failed to acknowledge more than 5 heartbeats, but the guild download is still running - check your connection speed");
            }

            Volatile.Write(ref this._lastSequence, seq);
            this.Logger.LogTrace(LoggerEvents.Heartbeat, "Sending heartbeat");
            var heartbeat = new GatewayPayload
            {
                OpCode = GatewayOpCode.Heartbeat,
                Data = seq
            };
            var heartbeat_str = JsonConvert.SerializeObject(heartbeat);
            await this.SendRawPayloadAsync(heartbeat_str);

            this._lastHeartbeat = DateTimeOffset.Now;

            Interlocked.Increment(ref this._skippedHeartbeats);
        }

        internal async Task SendIdentifyAsync(StatusUpdate status)
        {
            var identify = new GatewayIdentify
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
            var payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.Identify,
                Data = identify
            };
            var payloadstr = JsonConvert.SerializeObject(payload);
            await this.SendRawPayloadAsync(payloadstr);

            this.Logger.LogDebug(LoggerEvents.Intents, "Registered gateway intents ({Intents})", this.Configuration.Intents);
        }

        internal async Task SendResumeAsync()
        {
            var resume = new GatewayResume
            {
                Token = Utilities.GetFormattedToken(this),
                SessionId = this._sessionId,
                SequenceNumber = Volatile.Read(ref this._lastSequence)
            };
            var resume_payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.Resume,
                Data = resume
            };
            var resumestr = JsonConvert.SerializeObject(resume_payload);

            await this.SendRawPayloadAsync(resumestr);
        }

        internal async Task InternalUpdateGatewayAsync()
        {
            var info = await this.GetGatewayInfoAsync();
            this.GatewayInfo = info;
            this.GatewayUri = new Uri(info.Url);
        }

        internal async Task SendRawPayloadAsync(string jsonPayload)
        {
            this.Logger.LogTrace(LoggerEvents.GatewayWsTx, jsonPayload);
            await this._webSocketClient.SendMessageAsync(jsonPayload);
        }

        /// <summary>
        /// Sends a raw payload to the gateway. This method is not recommended for use unless you know what you're doing.
        /// </summary>
        /// <param name="opCode">The opcode to send to the Discord gateway.</param>
        /// <param name="data">The data to deserialize.</param>
        /// <typeparam name="T">The type of data that the object belongs to.</typeparam>
        /// <returns>A task representing the payload being sent.</returns>
        [Obsolete("This method should not be used unless you know what you're doing. Instead, look towards the other explicitly implemented methods which come with client-side validation.")]
        public Task SendPayloadAsync<T>(GatewayOpCode opCode, T data) => this.SendPayloadAsync(opCode, (object?)data);

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
            return this.SendRawPayloadAsync(payloadString);
        }
        #endregion

        #region Semaphore Methods

        private SocketLock GetSocketLock()
            => _socketLocks.GetOrAdd(this.CurrentApplication.Id, appId => new SocketLock(appId, this.GatewayInfo.SessionBucket.MaxConcurrency));

        #endregion
    }
}
