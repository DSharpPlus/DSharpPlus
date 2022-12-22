// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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

    private static ConcurrentDictionary<ulong, SocketLock> SocketLocks { get; } = new ConcurrentDictionary<ulong, SocketLock>();
    private ManualResetEventSlim SessionLock { get; } = new ManualResetEventSlim(true);

    #endregion

    #region Internal Connection Methods

    private Task InternalReconnectAsync(bool startNewSession = false, int code = 1000, string message = "")
    {
        if (startNewSession)
        {
            _sessionId = null;
        }

        _ = _webSocketClient.DisconnectAsync(code, message);
        return Task.CompletedTask;
    }

    /* GATEWAY VERSION IS IN THIS METHOD!! If you need to update the Gateway Version, look for gwuri ~Velvet */
    internal async Task InternalConnectAsync()
    {
        SocketLock socketLock = null;
        try
        {
            if (GatewayInfo == null)
            {
                await InternalUpdateGatewayAsync().ConfigureAwait(false);
            }

            await InitializeAsync().ConfigureAwait(false);

            socketLock = GetSocketLock();
            await socketLock.LockAsync().ConfigureAwait(false);
        }
        catch
        {
            socketLock?.UnlockAfter(TimeSpan.Zero);
            throw;
        }

        if (!Presences.ContainsKey(CurrentUser.Id))
        {
            _presences[CurrentUser.Id] = new DiscordPresence
            {
                Discord = this,
                RawActivity = new TransportActivity(),
                Activity = new DiscordActivity(),
                Status = UserStatus.Online,
                InternalUser = new TransportUser
                {
                    Id = CurrentUser.Id,
                    Username = CurrentUser.Username,
                    Discriminator = CurrentUser.Discriminator,
                    AvatarHash = CurrentUser.AvatarHash
                }
            };
        }
        else
        {
            DiscordPresence pr = _presences[CurrentUser.Id];
            pr.RawActivity = new TransportActivity();
            pr.Activity = new DiscordActivity();
            pr.Status = UserStatus.Online;
        }

        Volatile.Write(ref _skippedHeartbeats, 0);

        _webSocketClient = Configuration.WebSocketClientFactory(Configuration.Proxy);
        _payloadDecompressor = Configuration.GatewayCompressionLevel != GatewayCompressionLevel.None
            ? new PayloadDecompressor(Configuration.GatewayCompressionLevel)
            : null;

        _cancelTokenSource = new CancellationTokenSource();
        _cancelToken = _cancelTokenSource.Token;

        _webSocketClient.Connected += SocketOnConnect;
        _webSocketClient.Disconnected += SocketOnDisconnect;
        _webSocketClient.MessageReceived += SocketOnMessage;
        _webSocketClient.ExceptionThrown += SocketOnException;

        QueryUriBuilder gwuri = new QueryUriBuilder(GatewayUri)
            .AddParameter("v", "10")
            .AddParameter("encoding", "json");

        if (Configuration.GatewayCompressionLevel == GatewayCompressionLevel.Stream)
        {
            gwuri.AddParameter("compress", "zlib-stream");
        }

        await _webSocketClient.ConnectAsync(gwuri.Build()).ConfigureAwait(false);

        Task SocketOnConnect(IWebSocketClient sender, SocketEventArgs e)
            => _socketOpened.InvokeAsync(this, e);

        async Task SocketOnMessage(IWebSocketClient sender, SocketMessageEventArgs e)
        {
            string msg = null;
            if (e is SocketTextMessageEventArgs etext)
            {
                msg = etext.Message;
            }
            else if (e is SocketBinaryMessageEventArgs ebin) // :DDDD
            {
                using MemoryStream ms = new MemoryStream();
                if (!_payloadDecompressor.TryDecompress(new ArraySegment<byte>(ebin.Message), ms))
                {
                    Logger.LogError(LoggerEvents.WebSocketReceiveFailure, "Payload decompression failed");
                    return;
                }

                ms.Position = 0;
                using StreamReader sr = new StreamReader(ms, Utilities.UTF8);
                msg = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            try
            {
                Logger.LogTrace(LoggerEvents.GatewayWsRx, msg);
                await HandleSocketMessageAsync(msg).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(LoggerEvents.WebSocketReceiveFailure, ex, "Socket handler suppressed an exception");
            }
        }

        Task SocketOnException(IWebSocketClient sender, SocketErrorEventArgs e)
            => _socketErrored.InvokeAsync(this, e);

        async Task SocketOnDisconnect(IWebSocketClient sender, SocketCloseEventArgs e)
        {
            // release session and connection
            ConnectionLock.Set();
            SessionLock.Set();

            if (!_disposed)
            {
                _cancelTokenSource.Cancel();
            }

            Logger.LogDebug(LoggerEvents.ConnectionClose, "Connection closed ({CloseCode}, '{CloseMessage}')", e.CloseCode, e.CloseMessage);
            await _socketClosed.InvokeAsync(this, e).ConfigureAwait(false);

            if (Configuration.AutoReconnect && (e.CloseCode < 4001 || e.CloseCode >= 5000))
            {
                Logger.LogCritical(LoggerEvents.ConnectionClose, "Connection terminated ({CloseCode}, '{CloseMessage}'), reconnecting", e.CloseCode, e.CloseMessage);

                if (_status == null)
                {
                    await ConnectAsync().ConfigureAwait(false);
                }
                else
                    if (_status.IdleSince.HasValue)
                {
                    await ConnectAsync(_status._activity, _status.Status, Utilities.GetDateTimeOffsetFromMilliseconds(_status.IdleSince.Value)).ConfigureAwait(false);
                }
                else
                {
                    await ConnectAsync(_status._activity, _status.Status).ConfigureAwait(false);
                }
            }
            else
            {
                Logger.LogInformation(LoggerEvents.ConnectionClose, "Connection terminated ({CloseCode}, '{CloseMessage}')", e.CloseCode, e.CloseMessage);
            }
        }
    }

    #endregion

    #region WebSocket (Events)

    internal async Task HandleSocketMessageAsync(string data)
    {
        GatewayPayload? payload = JsonConvert.DeserializeObject<GatewayPayload>(data);
        _lastSequence = payload.Sequence ?? _lastSequence;
        switch (payload.OpCode)
        {
            case GatewayOpCode.Dispatch:
                await HandleDispatchAsync(payload).ConfigureAwait(false);
                break;

            case GatewayOpCode.Heartbeat:
                await OnHeartbeatAsync((long)payload.Data).ConfigureAwait(false);
                break;

            case GatewayOpCode.Reconnect:
                await OnReconnectAsync().ConfigureAwait(false);
                break;

            case GatewayOpCode.InvalidSession:
                await OnInvalidateSessionAsync((bool)payload.Data).ConfigureAwait(false);
                break;

            case GatewayOpCode.Hello:
                await OnHelloAsync((payload.Data as JObject).ToDiscordObject<GatewayHello>()).ConfigureAwait(false);
                break;

            case GatewayOpCode.HeartbeatAck:
                await OnHeartbeatAckAsync().ConfigureAwait(false);
                break;

            default:
                Logger.LogWarning(LoggerEvents.WebSocketReceive, "Unknown Discord opcode: {Op}\nPayload: {Payload}", payload.OpCode, payload.Data);
                break;
        }
    }

    internal async Task OnHeartbeatAsync(long seq)
    {
        Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HEARTBEAT (OP1)");
        await SendHeartbeatAsync(seq).ConfigureAwait(false);
    }

    internal async Task OnReconnectAsync()
    {
        Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received RECONNECT (OP7)");
        await InternalReconnectAsync(code: 4000, message: "OP7 acknowledged").ConfigureAwait(false);
    }

    internal async Task OnInvalidateSessionAsync(bool data)
    {
        // begin a session if one is not open already
        if (SessionLock.Wait(0))
        {
            SessionLock.Reset();
        }

        // we are sending a fresh resume/identify, so lock the socket
        SocketLock socketLock = GetSocketLock();
        await socketLock.LockAsync().ConfigureAwait(false);
        socketLock.UnlockAfter(TimeSpan.FromSeconds(5));

        if (data)
        {
            Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received INVALID_SESSION (OP9, true)");
            await Task.Delay(6000).ConfigureAwait(false);
            await SendResumeAsync().ConfigureAwait(false);
        }
        else
        {
            Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received INVALID_SESSION (OP9, false)");
            _sessionId = null;
            await SendIdentifyAsync(_status).ConfigureAwait(false);
        }
    }

    internal async Task OnHelloAsync(GatewayHello hello)
    {
        Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HELLO (OP10)");

        if (SessionLock.Wait(0))
        {
            SessionLock.Reset();
            GetSocketLock().UnlockAfter(TimeSpan.FromSeconds(5));
        }
        else
        {
            Logger.LogWarning(LoggerEvents.SessionUpdate, "Attempt to start a session while another session is active");
            return;
        }

        Interlocked.CompareExchange(ref _skippedHeartbeats, 0, 0);
        _heartbeatInterval = hello.HeartbeatInterval;
        _heartbeatTask = Task.Run(HeartbeatLoopAsync, _cancelToken);

        if (string.IsNullOrEmpty(_sessionId))
        {
            await SendIdentifyAsync(_status).ConfigureAwait(false);
        }
        else
        {
            await SendResumeAsync().ConfigureAwait(false);
        }
    }

    internal async Task OnHeartbeatAckAsync()
    {
        Interlocked.Decrement(ref _skippedHeartbeats);

        int ping = (int)(DateTime.Now - _lastHeartbeat).TotalMilliseconds;

        Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HEARTBEAT_ACK (OP11, {Heartbeat}ms)", ping);

        Volatile.Write(ref _ping, ping);

        HeartbeatEventArgs args = new HeartbeatEventArgs
        {
            Ping = Ping,
            Timestamp = DateTimeOffset.Now
        };

        await _heartbeated.InvokeAsync(this, args).ConfigureAwait(false);
    }

    internal async Task HeartbeatLoopAsync()
    {
        Logger.LogDebug(LoggerEvents.Heartbeat, "Heartbeat task started");
        CancellationToken token = _cancelToken;
        try
        {
            while (true)
            {
                await SendHeartbeatAsync(_lastSequence).ConfigureAwait(false);
                await Task.Delay(_heartbeatInterval, token).ConfigureAwait(false);
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
        {
            throw new Exception("Game name can't be longer than 128 characters!");
        }

        long? since_unix = idleSince != null ? (long?)Utilities.GetUnixTime(idleSince.Value) : null;
        DiscordActivity act = activity ?? new DiscordActivity();

        StatusUpdate status = new StatusUpdate
        {
            Activity = new TransportActivity(act),
            IdleSince = since_unix,
            IsAFK = idleSince != null,
            Status = userStatus ?? UserStatus.Online
        };

        // Solution to have status persist between sessions
        _status = status;
        GatewayPayload status_update = new GatewayPayload
        {
            OpCode = GatewayOpCode.StatusUpdate,
            Data = status
        };

        string statusstr = JsonConvert.SerializeObject(status_update);

        await WsSendAsync(statusstr).ConfigureAwait(false);

        if (!_presences.ContainsKey(CurrentUser.Id))
        {
            _presences[CurrentUser.Id] = new DiscordPresence
            {
                Discord = this,
                Activity = act,
                Status = userStatus ?? UserStatus.Online,
                InternalUser = new TransportUser { Id = CurrentUser.Id }
            };
        }
        else
        {
            DiscordPresence pr = _presences[CurrentUser.Id];
            pr.Activity = act;
            pr.Status = userStatus ?? pr.Status;
        }
    }

    internal async Task SendHeartbeatAsync(long seq)
    {
        bool more_than_5 = Volatile.Read(ref _skippedHeartbeats) > 5;
        bool guilds_comp = Volatile.Read(ref _guildDownloadCompleted);

        if (guilds_comp && more_than_5)
        {
            Logger.LogCritical(LoggerEvents.HeartbeatFailure, "Server failed to acknowledge more than 5 heartbeats - connection is zombie");

            ZombiedEventArgs args = new ZombiedEventArgs
            {
                Failures = Volatile.Read(ref _skippedHeartbeats),
                GuildDownloadCompleted = true
            };
            await _zombied.InvokeAsync(this, args).ConfigureAwait(false);

            await InternalReconnectAsync(code: 4001, message: "Too many heartbeats missed").ConfigureAwait(false);

            return;
        }

        if (!guilds_comp && more_than_5)
        {
            ZombiedEventArgs args = new ZombiedEventArgs
            {
                Failures = Volatile.Read(ref _skippedHeartbeats),
                GuildDownloadCompleted = false
            };
            await _zombied.InvokeAsync(this, args).ConfigureAwait(false);

            Logger.LogWarning(LoggerEvents.HeartbeatFailure, "Server failed to acknowledge more than 5 heartbeats, but the guild download is still running - check your connection speed");
        }

        Volatile.Write(ref _lastSequence, seq);
        Logger.LogTrace(LoggerEvents.Heartbeat, "Sending heartbeat");
        GatewayPayload heartbeat = new GatewayPayload
        {
            OpCode = GatewayOpCode.Heartbeat,
            Data = seq
        };
        string heartbeat_str = JsonConvert.SerializeObject(heartbeat);
        await WsSendAsync(heartbeat_str).ConfigureAwait(false);

        _lastHeartbeat = DateTimeOffset.Now;

        Interlocked.Increment(ref _skippedHeartbeats);
    }

    internal async Task SendIdentifyAsync(StatusUpdate status)
    {
        GatewayIdentify identify = new GatewayIdentify
        {
            Token = Utilities.GetFormattedToken(this),
            Compress = Configuration.GatewayCompressionLevel == GatewayCompressionLevel.Payload,
            LargeThreshold = Configuration.LargeThreshold,
            ShardInfo = new ShardInfo
            {
                ShardId = Configuration.ShardId,
                ShardCount = Configuration.ShardCount
            },
            Presence = status,
            Intents = Configuration.Intents
        };
        GatewayPayload payload = new GatewayPayload
        {
            OpCode = GatewayOpCode.Identify,
            Data = identify
        };
        string payloadstr = JsonConvert.SerializeObject(payload);
        await WsSendAsync(payloadstr).ConfigureAwait(false);

        Logger.LogDebug(LoggerEvents.Intents, "Registered gateway intents ({Intents})", Configuration.Intents);
    }

    internal async Task SendResumeAsync()
    {
        GatewayResume resume = new GatewayResume
        {
            Token = Utilities.GetFormattedToken(this),
            SessionId = _sessionId,
            SequenceNumber = Volatile.Read(ref _lastSequence)
        };
        GatewayPayload resume_payload = new GatewayPayload
        {
            OpCode = GatewayOpCode.Resume,
            Data = resume
        };
        string resumestr = JsonConvert.SerializeObject(resume_payload);

        await WsSendAsync(resumestr).ConfigureAwait(false);
    }
    internal async Task InternalUpdateGatewayAsync()
    {
        Net.GatewayInfo info = await GetGatewayInfoAsync().ConfigureAwait(false);
        GatewayInfo = info;
        GatewayUri = new Uri(info.Url);
    }

    internal async Task WsSendAsync(string payload)
    {
        Logger.LogTrace(LoggerEvents.GatewayWsTx, payload);
        await _webSocketClient.SendMessageAsync(payload).ConfigureAwait(false);
    }

    #endregion

    #region Semaphore Methods

    private SocketLock GetSocketLock()
        => SocketLocks.GetOrAdd(CurrentApplication.Id, appId => new SocketLock(appId, GatewayInfo.SessionBucket.MaxConcurrency));

    #endregion
}
