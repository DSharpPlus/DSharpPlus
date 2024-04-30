using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Net.Serialization;
using DSharpPlus.Net.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Lavalink;

[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
internal delegate void NodeDisconnectedEventHandler(LavalinkNodeConnection node);

/// <summary>
/// Represents a connection to a Lavalink node.
/// </summary>
[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
public sealed class LavalinkNodeConnection
{
    /// <summary>
    /// Triggered whenever Lavalink WebSocket throws an exception.
    /// </summary>
    public event AsyncEventHandler<LavalinkNodeConnection, SocketErrorEventArgs> LavalinkSocketErrored
    {
        add => _lavalinkSocketError.Register(value);
        remove => _lavalinkSocketError.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkNodeConnection, SocketErrorEventArgs> _lavalinkSocketError;

    /// <summary>
    /// Triggered when this node disconnects.
    /// </summary>
    public event AsyncEventHandler<LavalinkNodeConnection, NodeDisconnectedEventArgs> Disconnected
    {
        add => _disconnected.Register(value);
        remove => _disconnected.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs> _disconnected;

    /// <summary>
    /// Triggered when this node receives a statistics update.
    /// </summary>
    public event AsyncEventHandler<LavalinkNodeConnection, StatisticsReceivedEventArgs> StatisticsReceived
    {
        add => _statsReceived.Register(value);
        remove => _statsReceived.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkNodeConnection, StatisticsReceivedEventArgs> _statsReceived;

    /// <summary>
    /// Triggered whenever any of the players on this node is updated.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, PlayerUpdateEventArgs> PlayerUpdated
    {
        add => _playerUpdated.Register(value);
        remove => _playerUpdated.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs> _playerUpdated;

    /// <summary>
    /// Triggered whenever playback of a track starts.
    /// <para>This is only available for version 3.3.1 and greater.</para>
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, TrackStartEventArgs> PlaybackStarted
    {
        add => _playbackStarted.Register(value);
        remove => _playbackStarted.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs> _playbackStarted;

    /// <summary>
    /// Triggered whenever playback of a track finishes.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, TrackFinishEventArgs> PlaybackFinished
    {
        add => _playbackFinished.Register(value);
        remove => _playbackFinished.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs> _playbackFinished;

    /// <summary>
    /// Triggered whenever playback of a track gets stuck.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, TrackStuckEventArgs> TrackStuck
    {
        add => _trackStuck.Register(value);
        remove => _trackStuck.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs> _trackStuck;

    /// <summary>
    /// Triggered whenever playback of a track encounters an error.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, TrackExceptionEventArgs> TrackException
    {
        add => _trackException.Register(value);
        remove => _trackException.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs> _trackException;

    /// <summary>
    /// Triggered whenever a new guild connection is created.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, GuildConnectionCreatedEventArgs> GuildConnectionCreated
    {
        add => _guildConnectionCreated.Register(value);
        remove => _guildConnectionCreated.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, GuildConnectionCreatedEventArgs> _guildConnectionCreated;

    /// <summary>
    /// Triggered whenever a guild connection is removed.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, GuildConnectionRemovedEventArgs> GuildConnectionRemoved
    {
        add => _guildConnectionRemoved.Register(value);
        remove => _guildConnectionRemoved.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, GuildConnectionRemovedEventArgs> _guildConnectionRemoved;

    /// <summary>
    /// Gets the remote endpoint of this Lavalink node connection.
    /// </summary>
    public ConnectionEndpoint NodeEndpoint => Configuration.SocketEndpoint;

    /// <summary>
    /// Gets whether the client is connected to Lavalink.
    /// </summary>
    public bool IsConnected => !Volatile.Read(ref _isDisposed);
    private bool _isDisposed = false;
    private int _backoff = 0;
    private const int MinimumBackoff = 7500;
    private const int MaximumBackoff = 120000;

    /// <summary>
    /// Gets the current resource usage statistics.
    /// </summary>
    public LavalinkStatistics Statistics { get; }

    /// <summary>
    /// Gets a dictionary of Lavalink guild connections for this node.
    /// </summary>
    public IReadOnlyDictionary<ulong, LavalinkGuildConnection> ConnectedGuilds { get; }
    internal ConcurrentDictionary<ulong, LavalinkGuildConnection> _connectedGuilds = new();

    /// <summary>
    /// Gets the REST client for this Lavalink connection.
    /// </summary>
    public LavalinkRestClient Rest { get; }

    /// <summary>
    /// Gets the parent extension which this node connection belongs to.
    /// </summary>
    public LavalinkExtension Parent { get; }

    /// <summary>
    /// Gets the Discord client this node connection belongs to.
    /// </summary>
    public DiscordClient Discord { get; }

    internal LavalinkConfiguration Configuration { get; }
    internal DiscordVoiceRegion Region { get; }

    private IWebSocketClient WebSocket { get; set; }

    private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>> VoiceStateUpdates { get; }
    private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>> VoiceServerUpdates { get; }

    internal LavalinkNodeConnection(DiscordClient client, LavalinkExtension extension, LavalinkConfiguration config)
    {
        Discord = client;
        Parent = extension;
        Configuration = new LavalinkConfiguration(config);

        if (config.Region != null && Discord.VoiceRegions.Values.Contains(config.Region))
        {
            Region = config.Region;
        }

        ConnectedGuilds = new ReadOnlyConcurrentDictionary<ulong, LavalinkGuildConnection>(_connectedGuilds);
        Statistics = new LavalinkStatistics();

        _lavalinkSocketError = new AsyncEvent<LavalinkNodeConnection, SocketErrorEventArgs>("LAVALINK_SOCKET_ERROR", Discord.EventErrorHandler);
        _disconnected = new AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs>("LAVALINK_NODE_DISCONNECTED", Discord.EventErrorHandler);
        _statsReceived = new AsyncEvent<LavalinkNodeConnection, StatisticsReceivedEventArgs>("LAVALINK_STATS_RECEIVED", Discord.EventErrorHandler);
        _playerUpdated = new AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs>("LAVALINK_PLAYER_UPDATED", Discord.EventErrorHandler);
        _playbackStarted = new AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs>("LAVALINK_PLAYBACK_STARTED", Discord.EventErrorHandler);
        _playbackFinished = new AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs>("LAVALINK_PLAYBACK_FINISHED", Discord.EventErrorHandler);
        _trackStuck = new AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs>("LAVALINK_TRACK_STUCK", Discord.EventErrorHandler);
        _trackException = new AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs>("LAVALINK_TRACK_EXCEPTION", Discord.EventErrorHandler);
        _guildConnectionCreated = new AsyncEvent<LavalinkGuildConnection, GuildConnectionCreatedEventArgs>("LAVALINK_GUILD_CONNECTION_CREATED", Discord.EventErrorHandler);
        _guildConnectionRemoved = new AsyncEvent<LavalinkGuildConnection, GuildConnectionRemovedEventArgs>("LAVALINK_GUILD_CONNECTION_REMOVED", Discord.EventErrorHandler);

        VoiceServerUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>>();
        VoiceStateUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>>();
        Discord.VoiceStateUpdated += Discord_VoiceStateUpdated;
        Discord.VoiceServerUpdated += Discord_VoiceServerUpdated;

        Rest = new LavalinkRestClient(Configuration, Discord);

        Volatile.Write(ref _isDisposed, false);
    }

    /// <summary>
    /// Establishes a connection to the Lavalink node.
    /// </summary>
    /// <returns></returns>
    internal async Task StartAsync()
    {
        if (Discord?.CurrentUser?.Id == null || Discord?.ShardCount == null)
        {
            throw new InvalidOperationException("This operation requires the Discord client to be fully initialized.");
        }

        WebSocket = Discord.Configuration.WebSocketClientFactory(Discord.Configuration.Proxy);
        WebSocket.Connected += WebSocket_OnConnect;
        WebSocket.Disconnected += WebSocket_OnDisconnect;
        WebSocket.ExceptionThrown += WebSocket_OnException;
        WebSocket.MessageReceived += WebSocket_OnMessage;

        WebSocket.AddDefaultHeader("Authorization", Configuration.Password);
        WebSocket.AddDefaultHeader("Num-Shards", Discord.ShardCount.ToString(CultureInfo.InvariantCulture));
        WebSocket.AddDefaultHeader("User-Id", Discord.CurrentUser.Id.ToString(CultureInfo.InvariantCulture));
        WebSocket.AddDefaultHeader("Client-Name", "DSharpPlus.Lavalink");
        if (Configuration.ResumeKey != null)
        {
            WebSocket.AddDefaultHeader("Resume-Key", Configuration.ResumeKey);
        }

        do
        {
            try
            {
                if (_backoff != 0)
                {
                    await Task.Delay(_backoff);
                    _backoff = Math.Min(_backoff * 2, MaximumBackoff);
                }
                else
                {
                    _backoff = MinimumBackoff;
                }

                await WebSocket.ConnectAsync(new Uri(Configuration.SocketEndpoint.ToWebSocketString()));
                break;
            }
            catch (PlatformNotSupportedException)
            { throw; }
            catch (NotImplementedException)
            { throw; }
            catch (Exception ex)
            {
                if (!Configuration.SocketAutoReconnect || _backoff == MaximumBackoff)
                {
                    Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, ex, "Failed to connect to Lavalink.");
                    throw;
                }
                else
                {
                    Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, ex, $"Failed to connect to Lavalink, retrying in {_backoff} ms.");
                }
            }
        }
        while (Configuration.SocketAutoReconnect);

        Volatile.Write(ref _isDisposed, false);
    }

    /// <summary>
    /// Stops this Lavalink node connection and frees resources.
    /// </summary>
    /// <returns></returns>
    public async Task StopAsync()
    {
        foreach (KeyValuePair<ulong, LavalinkGuildConnection> kvp in _connectedGuilds)
        {
            await kvp.Value.DisconnectAsync();
        }

        NodeDisconnected?.Invoke(this);

        Volatile.Write(ref _isDisposed, true);
        await WebSocket.DisconnectAsync();
        // this should not be here, no?
        //await this._disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this));
    }

    /// <summary>
    /// Connects this Lavalink node to specified Discord channel.
    /// </summary>
    /// <param name="channel">Voice channel to connect to.</param>
    /// <returns>Channel connection, which allows for playback control.</returns>
    public async Task<LavalinkGuildConnection> ConnectAsync(DiscordChannel channel)
    {
        if (_connectedGuilds.ContainsKey(channel.Guild.Id))
        {
            return _connectedGuilds[channel.Guild.Id];
        }

        if (channel.Guild == null || (channel.Type != DiscordChannelType.Voice && channel.Type != DiscordChannelType.Stage))
        {
            throw new ArgumentException("Invalid channel specified.", nameof(channel));
        }

        TaskCompletionSource<VoiceStateUpdateEventArgs> vstut = new TaskCompletionSource<VoiceStateUpdateEventArgs>();
        TaskCompletionSource<VoiceServerUpdateEventArgs> vsrut = new TaskCompletionSource<VoiceServerUpdateEventArgs>();
        VoiceStateUpdates[channel.Guild.Id] = vstut;
        VoiceServerUpdates[channel.Guild.Id] = vsrut;

        VoiceDispatch vsd = new VoiceDispatch
        {
            OpCode = 4,
            Payload = new VoiceStateUpdatePayload
            {
                GuildId = channel.Guild.Id,
                ChannelId = channel.Id,
                Deafened = false,
                Muted = false
            }
        };
        string vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
        await (channel.Discord as DiscordClient).SendRawPayloadAsync(vsj);
        VoiceStateUpdateEventArgs vstu = await vstut.Task;
        VoiceServerUpdateEventArgs vsru = await vsrut.Task;
        await SendPayloadAsync(new LavalinkVoiceUpdate(vstu, vsru));

        LavalinkGuildConnection con = new LavalinkGuildConnection(this, channel, vstu);
        con.ChannelDisconnected += Con_ChannelDisconnected;
        con.PlayerUpdated += (s, e) => _playerUpdated.InvokeAsync(s, e);
        con.PlaybackStarted += (s, e) => _playbackStarted.InvokeAsync(s, e);
        con.PlaybackFinished += (s, e) => _playbackFinished.InvokeAsync(s, e);
        con.TrackStuck += (s, e) => _trackStuck.InvokeAsync(s, e);
        con.TrackException += (s, e) => _trackException.InvokeAsync(s, e);
        _connectedGuilds[channel.Guild.Id] = con;
        await _guildConnectionCreated.InvokeAsync(con, new GuildConnectionCreatedEventArgs());

        return con;
    }

    /// <summary>
    /// Gets a Lavalink connection to specified Discord channel.
    /// </summary>
    /// <param name="guild">Guild to get connection for.</param>
    /// <returns>Channel connection, which allows for playback control.</returns>
    public LavalinkGuildConnection GetGuildConnection(DiscordGuild guild)
        => _connectedGuilds.TryGetValue(guild.Id, out LavalinkGuildConnection? lgc) && lgc.IsConnected ? lgc : null;

    internal async Task SendPayloadAsync(LavalinkPayload payload)
        => await WsSendAsync(JsonConvert.SerializeObject(payload, Formatting.None));

    private async Task WebSocket_OnMessage(IWebSocketClient client, SocketMessageEventArgs e)
    {
        if (e is not SocketTextMessageEventArgs et)
        {
            Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, "Lavalink sent binary data - unable to process");
            return;
        }

        Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsRx, et.Message);

        string json = et.Message;
        JObject jsonData = JObject.Parse(json);
        switch (jsonData["op"].ToString())
        {
            case "playerUpdate":
                ulong gid = (ulong)jsonData["guildId"];
                LavalinkState state = jsonData["state"].ToDiscordObject<LavalinkState>();
                if (_connectedGuilds.TryGetValue(gid, out LavalinkGuildConnection? lvl))
                {
                    await lvl.InternalUpdatePlayerStateAsync(state);
                }

                break;

            case "stats":
                LavalinkStats statsRaw = jsonData.ToDiscordObject<LavalinkStats>();
                Statistics.Update(statsRaw);
                await _statsReceived.InvokeAsync(this, new StatisticsReceivedEventArgs(Statistics));
                break;

            case "event":
                EventType evtype = jsonData["type"].ToDiscordObject<EventType>();
                ulong guildId = (ulong)jsonData["guildId"];
                switch (evtype)
                {
                    case EventType.TrackStartEvent:
                        if (_connectedGuilds.TryGetValue(guildId, out LavalinkGuildConnection? lvl_evtst))
                        {
                            await lvl_evtst.InternalPlaybackStartedAsync(jsonData["track"].ToString());
                        }

                        break;

                    case EventType.TrackEndEvent:
                        TrackEndReason reason = TrackEndReason.Cleanup;
                        switch (jsonData["reason"].ToString())
                        {
                            case "FINISHED":
                                reason = TrackEndReason.Finished;
                                break;
                            case "LOAD_FAILED":
                                reason = TrackEndReason.LoadFailed;
                                break;
                            case "STOPPED":
                                reason = TrackEndReason.Stopped;
                                break;
                            case "REPLACED":
                                reason = TrackEndReason.Replaced;
                                break;
                            case "CLEANUP":
                                reason = TrackEndReason.Cleanup;
                                break;
                        }
                        if (_connectedGuilds.TryGetValue(guildId, out LavalinkGuildConnection? lvl_evtf))
                        {
                            await lvl_evtf.InternalPlaybackFinishedAsync(new TrackFinishData { Track = jsonData["track"].ToString(), Reason = reason });
                        }

                        break;

                    case EventType.TrackStuckEvent:
                        if (_connectedGuilds.TryGetValue(guildId, out LavalinkGuildConnection? lvl_evts))
                        {
                            await lvl_evts.InternalTrackStuckAsync(new TrackStuckData { Track = jsonData["track"].ToString(), Threshold = (long)jsonData["thresholdMs"] });
                        }

                        break;

                    case EventType.TrackExceptionEvent:
                        if (_connectedGuilds.TryGetValue(guildId, out LavalinkGuildConnection? lvl_evte))
                        {
                            await lvl_evte.InternalTrackExceptionAsync(new TrackExceptionData { Track = jsonData["track"].ToString(), Error = jsonData["error"].ToString() });
                        }

                        break;

                    case EventType.WebSocketClosedEvent:
                        if (_connectedGuilds.TryGetValue(guildId, out LavalinkGuildConnection? lvl_ewsce))
                        {
                            lvl_ewsce.VoiceWsDisconnectTcs.SetResult(true);
                            await lvl_ewsce.InternalWebSocketClosedAsync(new WebSocketCloseEventArgs(jsonData["code"].ToDiscordObject<int>(), jsonData["reason"].ToString(), jsonData["byRemote"].ToDiscordObject<bool>()));
                        }
                        break;
                }
                break;
        }
    }

    private Task WebSocket_OnException(IWebSocketClient client, SocketErrorEventArgs e)
        => _lavalinkSocketError.InvokeAsync(this, new SocketErrorEventArgs { Exception = e.Exception });

    private async Task WebSocket_OnDisconnect(IWebSocketClient client, SocketCloseEventArgs e)
    {
        if (IsConnected && e.CloseCode != 1001 && e.CloseCode != -1)
        {
            Discord.Logger.LogWarning(LavalinkEvents.LavalinkConnectionClosed, "Connection broken ({CloseCode}, '{CloseMessage}'), reconnecting", e.CloseCode, e.CloseMessage);
            await _disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this, false));

            if (Configuration.SocketAutoReconnect)
            {
                await StartAsync();
            }
        }
        else if (e.CloseCode != 1001 && e.CloseCode != -1)
        {
            Discord.Logger.LogInformation(LavalinkEvents.LavalinkConnectionClosed, "Connection closed ({CloseCode}, '{CloseMessage}')", e.CloseCode, e.CloseMessage);
            NodeDisconnected?.Invoke(this);
            await _disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this, true));
        }
        else
        {
            Volatile.Write(ref _isDisposed, true);
            Discord.Logger.LogWarning(LavalinkEvents.LavalinkConnectionClosed, "Lavalink died");
            foreach (KeyValuePair<ulong, LavalinkGuildConnection> kvp in _connectedGuilds)
            {
                await kvp.Value.SendVoiceUpdateAsync();
                _ = _connectedGuilds.TryRemove(kvp.Key, out LavalinkGuildConnection? con);
                await _guildConnectionRemoved.InvokeAsync(con, new GuildConnectionRemovedEventArgs());
            }
            NodeDisconnected?.Invoke(this);
            await _disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this, false));

            if (Configuration.SocketAutoReconnect)
            {
                await StartAsync();
            }
        }
    }

    private async Task WebSocket_OnConnect(IWebSocketClient client, SocketEventArgs ea)
    {
        Discord.Logger.LogDebug(LavalinkEvents.LavalinkConnected, "Connection to Lavalink node established");
        _backoff = 0;

        if (Configuration.ResumeKey != null)
        {
            await SendPayloadAsync(new LavalinkConfigureResume(Configuration.ResumeKey, Configuration.ResumeTimeout));
        }
    }

    private async void Con_ChannelDisconnected(LavalinkGuildConnection con)
    {
        _connectedGuilds.TryRemove(con.GuildId, out con);
        await _guildConnectionRemoved.InvokeAsync(con, new GuildConnectionRemovedEventArgs());
    }

    private Task Discord_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
    {
        DiscordGuild gld = e.Guild;
        if (gld == null)
        {
            return Task.CompletedTask;
        }

        if (e.User == null)
        {
            return Task.CompletedTask;
        }

        if (e.User.Id == Discord.CurrentUser.Id)
        {
            if (_connectedGuilds.TryGetValue(e.Guild.Id, out LavalinkGuildConnection? lvlgc))
            {
                lvlgc.VoiceStateUpdate = e;
            }

            if (e.After.Channel == null && IsConnected && _connectedGuilds.ContainsKey(gld.Id))
            {
                _ = Task.Run(async () =>
                {
                    Task delayTask = Task.Delay(Configuration.WebSocketCloseTimeout);
                    Task<bool> tcs = lvlgc.VoiceWsDisconnectTcs.Task;
                    _ = await Task.WhenAny(delayTask, tcs);

                    await lvlgc.DisconnectInternalAsync(false, true);
                    _ = _connectedGuilds.TryRemove(gld.Id, out LavalinkGuildConnection? con);
                    await _guildConnectionRemoved.InvokeAsync(con, new GuildConnectionRemovedEventArgs());
                });
            }

            if (!string.IsNullOrWhiteSpace(e.SessionId) && e.Channel != null && VoiceStateUpdates.TryRemove(gld.Id, out TaskCompletionSource<VoiceStateUpdateEventArgs>? xe))
            {
                xe.SetResult(e);
            }
        }

        return Task.CompletedTask;
    }

    private Task Discord_VoiceServerUpdated(DiscordClient client, VoiceServerUpdateEventArgs e)
    {
        DiscordGuild gld = e.Guild;
        if (gld == null)
        {
            return Task.CompletedTask;
        }

        if (_connectedGuilds.TryGetValue(e.Guild.Id, out LavalinkGuildConnection? lvlgc))
        {
            LavalinkVoiceUpdate lvlp = new LavalinkVoiceUpdate(lvlgc.VoiceStateUpdate, e);
            _ = Task.Run(() => WsSendAsync(JsonConvert.SerializeObject(lvlp)));
        }

        if (VoiceServerUpdates.TryRemove(gld.Id, out TaskCompletionSource<VoiceServerUpdateEventArgs>? xe))
        {
            xe.SetResult(e);
        }

        return Task.CompletedTask;
    }

    private async Task WsSendAsync(string payload)
    {
        Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsTx, payload);
        await WebSocket.SendMessageAsync(payload);
    }

    internal event NodeDisconnectedEventHandler NodeDisconnected;
}

// Kinda think this deserves another pack of instant noodles :^) -Emzi
// No I did it before in sai- alright then.
// Coroned noodles
