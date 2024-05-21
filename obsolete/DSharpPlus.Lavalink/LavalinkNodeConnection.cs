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
        add => this.lavalinkSocketError.Register(value);
        remove => this.lavalinkSocketError.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkNodeConnection, SocketErrorEventArgs> lavalinkSocketError;

    /// <summary>
    /// Triggered when this node disconnects.
    /// </summary>
    public event AsyncEventHandler<LavalinkNodeConnection, NodeDisconnectedEventArgs> Disconnected
    {
        add => this.disconnected.Register(value);
        remove => this.disconnected.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs> disconnected;

    /// <summary>
    /// Triggered when this node receives a statistics update.
    /// </summary>
    public event AsyncEventHandler<LavalinkNodeConnection, StatisticsReceivedEventArgs> StatisticsReceived
    {
        add => this.statsReceived.Register(value);
        remove => this.statsReceived.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkNodeConnection, StatisticsReceivedEventArgs> statsReceived;

    /// <summary>
    /// Triggered whenever any of the players on this node is updated.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, PlayerUpdateEventArgs> PlayerUpdated
    {
        add => this.playerUpdated.Register(value);
        remove => this.playerUpdated.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs> playerUpdated;

    /// <summary>
    /// Triggered whenever playback of a track starts.
    /// <para>This is only available for version 3.3.1 and greater.</para>
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, TrackStartEventArgs> PlaybackStarted
    {
        add => this.playbackStarted.Register(value);
        remove => this.playbackStarted.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs> playbackStarted;

    /// <summary>
    /// Triggered whenever playback of a track finishes.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, TrackFinishEventArgs> PlaybackFinished
    {
        add => this.playbackFinished.Register(value);
        remove => this.playbackFinished.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs> playbackFinished;

    /// <summary>
    /// Triggered whenever playback of a track gets stuck.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, TrackStuckEventArgs> TrackStuck
    {
        add => this.trackStuck.Register(value);
        remove => this.trackStuck.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs> trackStuck;

    /// <summary>
    /// Triggered whenever playback of a track encounters an error.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, TrackExceptionEventArgs> TrackException
    {
        add => this.trackException.Register(value);
        remove => this.trackException.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs> trackException;

    /// <summary>
    /// Triggered whenever a new guild connection is created.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, GuildConnectionCreatedEventArgs> GuildConnectionCreated
    {
        add => this.guildConnectionCreated.Register(value);
        remove => this.guildConnectionCreated.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, GuildConnectionCreatedEventArgs> guildConnectionCreated;

    /// <summary>
    /// Triggered whenever a guild connection is removed.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, GuildConnectionRemovedEventArgs> GuildConnectionRemoved
    {
        add => this.guildConnectionRemoved.Register(value);
        remove => this.guildConnectionRemoved.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, GuildConnectionRemovedEventArgs> guildConnectionRemoved;

    /// <summary>
    /// Gets the remote endpoint of this Lavalink node connection.
    /// </summary>
    public ConnectionEndpoint NodeEndpoint => this.Configuration.SocketEndpoint;

    /// <summary>
    /// Gets whether the client is connected to Lavalink.
    /// </summary>
    public bool IsConnected => !Volatile.Read(ref this.isDisposed);
    private bool isDisposed = false;
    private int backoff = 0;
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
    internal ConcurrentDictionary<ulong, LavalinkGuildConnection> connectedGuilds = new();

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

    private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdatedEventArgs>> VoiceStateUpdates { get; }
    private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdatedEventArgs>> VoiceServerUpdates { get; }

    internal LavalinkNodeConnection(DiscordClient client, LavalinkExtension extension, LavalinkConfiguration config)
    {
        this.Discord = client;
        this.Parent = extension;
        this.Configuration = new LavalinkConfiguration(config);

        if (config.Region != null && this.Discord.VoiceRegions.Values.Contains(config.Region))
        {
            this.Region = config.Region;
        }

        this.ConnectedGuilds = new ReadOnlyConcurrentDictionary<ulong, LavalinkGuildConnection>(this.connectedGuilds);
        this.Statistics = new LavalinkStatistics();

        this.lavalinkSocketError = new AsyncEvent<LavalinkNodeConnection, SocketErrorEventArgs>("LAVALINK_SOCKET_ERROR", this.Discord.EventErrorHandler);
        this.disconnected = new AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs>("LAVALINK_NODE_DISCONNECTED", this.Discord.EventErrorHandler);
        this.statsReceived = new AsyncEvent<LavalinkNodeConnection, StatisticsReceivedEventArgs>("LAVALINK_STATS_RECEIVED", this.Discord.EventErrorHandler);
        this.playerUpdated = new AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs>("LAVALINK_PLAYER_UPDATED", this.Discord.EventErrorHandler);
        this.playbackStarted = new AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs>("LAVALINK_PLAYBACK_STARTED", this.Discord.EventErrorHandler);
        this.playbackFinished = new AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs>("LAVALINK_PLAYBACK_FINISHED", this.Discord.EventErrorHandler);
        this.trackStuck = new AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs>("LAVALINK_TRACK_STUCK", this.Discord.EventErrorHandler);
        this.trackException = new AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs>("LAVALINK_TRACK_EXCEPTION", this.Discord.EventErrorHandler);
        this.guildConnectionCreated = new AsyncEvent<LavalinkGuildConnection, GuildConnectionCreatedEventArgs>("LAVALINK_GUILD_CONNECTION_CREATED", this.Discord.EventErrorHandler);
        this.guildConnectionRemoved = new AsyncEvent<LavalinkGuildConnection, GuildConnectionRemovedEventArgs>("LAVALINK_GUILD_CONNECTION_REMOVED", this.Discord.EventErrorHandler);

        this.VoiceServerUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdatedEventArgs>>();
        this.VoiceStateUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdatedEventArgs>>();
        this.Discord.VoiceStateUpdated += Discord_VoiceStateUpdated;
        this.Discord.VoiceServerUpdated += Discord_VoiceServerUpdated;

        this.Rest = new LavalinkRestClient(this.Configuration, this.Discord);

        Volatile.Write(ref this.isDisposed, false);
    }

    /// <summary>
    /// Establishes a connection to the Lavalink node.
    /// </summary>
    /// <returns></returns>
    internal async Task StartAsync()
    {
        if (this.Discord?.CurrentUser?.Id == null || this.Discord?.ShardCount == null)
        {
            throw new InvalidOperationException("This operation requires the Discord client to be fully initialized.");
        }

        this.WebSocket = this.Discord.Configuration.WebSocketClientFactory(this.Discord.Configuration.Proxy);
        this.WebSocket.Connected += WebSocket_OnConnectAsync;
        this.WebSocket.Disconnected += WebSocket_OnDisconnectAsync;
        this.WebSocket.ExceptionThrown += WebSocket_OnException;
        this.WebSocket.MessageReceived += WebSocket_OnMessageAsync;

        this.WebSocket.AddDefaultHeader("Authorization", this.Configuration.Password);
        this.WebSocket.AddDefaultHeader("Num-Shards", this.Discord.ShardCount.ToString(CultureInfo.InvariantCulture));
        this.WebSocket.AddDefaultHeader("User-Id", this.Discord.CurrentUser.Id.ToString(CultureInfo.InvariantCulture));
        this.WebSocket.AddDefaultHeader("Client-Name", "DSharpPlus.Lavalink");
        if (this.Configuration.ResumeKey != null)
        {
            this.WebSocket.AddDefaultHeader("Resume-Key", this.Configuration.ResumeKey);
        }

        do
        {
            try
            {
                if (this.backoff != 0)
                {
                    await Task.Delay(this.backoff);
                    this.backoff = Math.Min(this.backoff * 2, MaximumBackoff);
                }
                else
                {
                    this.backoff = MinimumBackoff;
                }

                await this.WebSocket.ConnectAsync(new Uri(this.Configuration.SocketEndpoint.ToWebSocketString()));
                break;
            }
            catch (PlatformNotSupportedException)
            { throw; }
            catch (NotImplementedException)
            { throw; }
            catch (Exception ex)
            {
                if (!this.Configuration.SocketAutoReconnect || this.backoff == MaximumBackoff)
                {
                    this.Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, ex, "Failed to connect to Lavalink.");
                    throw;
                }
                else
                {
                    this.Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, ex, "Failed to connect to Lavalink, retrying in {BackOff} ms.", this.backoff);
                }
            }
        }
        while (this.Configuration.SocketAutoReconnect);

        Volatile.Write(ref this.isDisposed, false);
    }

    /// <summary>
    /// Stops this Lavalink node connection and frees resources.
    /// </summary>
    /// <returns></returns>
    public async Task StopAsync()
    {
        foreach (KeyValuePair<ulong, LavalinkGuildConnection> kvp in this.connectedGuilds)
        {
            await kvp.Value.DisconnectAsync();
        }

        NodeDisconnected?.Invoke(this);

        Volatile.Write(ref this.isDisposed, true);
        await this.WebSocket.DisconnectAsync();
        // this should not be here, no?
        //await this.disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this));
    }

    /// <summary>
    /// Connects this Lavalink node to specified Discord channel.
    /// </summary>
    /// <param name="channel">Voice channel to connect to.</param>
    /// <returns>Channel connection, which allows for playback control.</returns>
    public async Task<LavalinkGuildConnection> ConnectAsync(DiscordChannel channel)
    {
        if (this.connectedGuilds.TryGetValue(channel.Guild.Id, out LavalinkGuildConnection value))
        {
            return value;
        }

        if (channel.Guild == null || (channel.Type != DiscordChannelType.Voice && channel.Type != DiscordChannelType.Stage))
        {
            throw new ArgumentException("Invalid channel specified.", nameof(channel));
        }

        TaskCompletionSource<VoiceStateUpdatedEventArgs> vstut = new();
        TaskCompletionSource<VoiceServerUpdatedEventArgs> vsrut = new();
        this.VoiceStateUpdates[channel.Guild.Id] = vstut;
        this.VoiceServerUpdates[channel.Guild.Id] = vsrut;

        VoiceDispatch vsd = new()
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
        VoiceStateUpdatedEventArgs vstu = await vstut.Task;
        VoiceServerUpdatedEventArgs vsru = await vsrut.Task;
        await SendPayloadAsync(new LavalinkVoiceUpdate(vstu, vsru));

        LavalinkGuildConnection con = new(this, vstu);
        con.ChannelDisconnected += Con_ChannelDisconnectedAsync;
        con.PlayerUpdated += (s, e) => this.playerUpdated.InvokeAsync(s, e);
        con.PlaybackStarted += (s, e) => this.playbackStarted.InvokeAsync(s, e);
        con.PlaybackFinished += (s, e) => this.playbackFinished.InvokeAsync(s, e);
        con.TrackStuck += (s, e) => this.trackStuck.InvokeAsync(s, e);
        con.TrackException += (s, e) => this.trackException.InvokeAsync(s, e);
        this.connectedGuilds[channel.Guild.Id] = con;
        await this.guildConnectionCreated.InvokeAsync(con, new GuildConnectionCreatedEventArgs());

        return con;
    }

    /// <summary>
    /// Gets a Lavalink connection to specified Discord channel.
    /// </summary>
    /// <param name="guild">Guild to get connection for.</param>
    /// <returns>Channel connection, which allows for playback control.</returns>
    public LavalinkGuildConnection GetGuildConnection(DiscordGuild guild)
        => this.connectedGuilds.TryGetValue(guild.Id, out LavalinkGuildConnection? lgc) && lgc.IsConnected ? lgc : null;

    internal async Task SendPayloadAsync(LavalinkPayload payload)
        => await WsSendAsync(JsonConvert.SerializeObject(payload, Formatting.None));

    private async Task WebSocket_OnMessageAsync(IWebSocketClient client, SocketMessageEventArgs e)
    {
        if (e is not SocketTextMessageEventArgs et)
        {
            this.Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, "Lavalink sent binary data - unable to process");
            return;
        }

        this.Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsRx, "{WebsocketMessage}", et.Message);

        string json = et.Message;
        JObject jsonData = JObject.Parse(json);
        switch (jsonData["op"].ToString())
        {
            case "playerUpdate":
                ulong gid = (ulong)jsonData["guildId"];
                LavalinkState state = jsonData["state"].ToDiscordObject<LavalinkState>();
                if (this.connectedGuilds.TryGetValue(gid, out LavalinkGuildConnection? lvl))
                {
                    await lvl.InternalUpdatePlayerStateAsync(state);
                }

                break;

            case "stats":
                LavalinkStats statsRaw = jsonData.ToDiscordObject<LavalinkStats>();
                this.Statistics.Update(statsRaw);
                await this.statsReceived.InvokeAsync(this, new StatisticsReceivedEventArgs(this.Statistics));
                break;

            case "event":
                EventType evtype = jsonData["type"].ToDiscordObject<EventType>();
                ulong guildId = (ulong)jsonData["guildId"];
                switch (evtype)
                {
                    case EventType.TrackStartEvent:
                        if (this.connectedGuilds.TryGetValue(guildId, out LavalinkGuildConnection? lvl_evtst))
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
                        if (this.connectedGuilds.TryGetValue(guildId, out LavalinkGuildConnection? lvl_evtf))
                        {
                            await lvl_evtf.InternalPlaybackFinishedAsync(new TrackFinishData { Track = jsonData["track"].ToString(), Reason = reason });
                        }

                        break;

                    case EventType.TrackStuckEvent:
                        if (this.connectedGuilds.TryGetValue(guildId, out LavalinkGuildConnection? lvl_evts))
                        {
                            await lvl_evts.InternalTrackStuckAsync(new TrackStuckData { Track = jsonData["track"].ToString(), Threshold = (long)jsonData["thresholdMs"] });
                        }

                        break;

                    case EventType.TrackExceptionEvent:
                        if (this.connectedGuilds.TryGetValue(guildId, out LavalinkGuildConnection? lvl_evte))
                        {
                            await lvl_evte.InternalTrackExceptionAsync(new TrackExceptionData { Track = jsonData["track"].ToString(), Error = jsonData["error"].ToString() });
                        }

                        break;

                    case EventType.WebSocketClosedEvent:
                        if (this.connectedGuilds.TryGetValue(guildId, out LavalinkGuildConnection? lvl_ewsce))
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
        => this.lavalinkSocketError.InvokeAsync(this, new SocketErrorEventArgs { Exception = e.Exception });

    private async Task WebSocket_OnDisconnectAsync(IWebSocketClient client, SocketCloseEventArgs e)
    {
        if (this.IsConnected && e.CloseCode != 1001 && e.CloseCode != -1)
        {
            this.Discord.Logger.LogWarning(LavalinkEvents.LavalinkConnectionClosed, "Connection broken ({CloseCode}, '{CloseMessage}'), reconnecting", e.CloseCode, e.CloseMessage);
            await this.disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this, false));

            if (this.Configuration.SocketAutoReconnect)
            {
                await StartAsync();
            }
        }
        else if (e.CloseCode is not 1001 and not (-1))
        {
            this.Discord.Logger.LogInformation(LavalinkEvents.LavalinkConnectionClosed, "Connection closed ({CloseCode}, '{CloseMessage}')", e.CloseCode, e.CloseMessage);
            NodeDisconnected?.Invoke(this);
            await this.disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this, true));
        }
        else
        {
            Volatile.Write(ref this.isDisposed, true);
            this.Discord.Logger.LogWarning(LavalinkEvents.LavalinkConnectionClosed, "Lavalink died");
            foreach (KeyValuePair<ulong, LavalinkGuildConnection> kvp in this.connectedGuilds)
            {
                await kvp.Value.SendVoiceUpdateAsync();
                _ = this.connectedGuilds.TryRemove(kvp.Key, out LavalinkGuildConnection? con);
                await this.guildConnectionRemoved.InvokeAsync(con, new GuildConnectionRemovedEventArgs());
            }
            NodeDisconnected?.Invoke(this);
            await this.disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this, false));

            if (this.Configuration.SocketAutoReconnect)
            {
                await StartAsync();
            }
        }
    }

    private async Task WebSocket_OnConnectAsync(IWebSocketClient client, SocketEventArgs ea)
    {
        this.Discord.Logger.LogDebug(LavalinkEvents.LavalinkConnected, "Connection to Lavalink node established");
        this.backoff = 0;

        if (this.Configuration.ResumeKey != null)
        {
            await SendPayloadAsync(new LavalinkConfigureResume(this.Configuration.ResumeKey, this.Configuration.ResumeTimeout));
        }
    }

    private async void Con_ChannelDisconnectedAsync(LavalinkGuildConnection con)
    {
        this.connectedGuilds.TryRemove(con.GuildId, out con);
        await this.guildConnectionRemoved.InvokeAsync(con, new GuildConnectionRemovedEventArgs());
    }

    private Task Discord_VoiceStateUpdated(DiscordClient client, VoiceStateUpdatedEventArgs e)
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

        if (e.User.Id == this.Discord.CurrentUser.Id)
        {
            if (this.connectedGuilds.TryGetValue(e.Guild.Id, out LavalinkGuildConnection? lvlgc))
            {
                lvlgc.VoiceStateUpdate = e;
            }

            if (e.After.Channel == null && this.IsConnected && this.connectedGuilds.ContainsKey(gld.Id))
            {
                _ = Task.Run(async () =>
                {
                    Task delayTask = Task.Delay(this.Configuration.WebSocketCloseTimeout);
                    Task<bool> tcs = lvlgc.VoiceWsDisconnectTcs.Task;
                    _ = await Task.WhenAny(delayTask, tcs);

                    await lvlgc.DisconnectInternalAsync(false, true);
                    _ = this.connectedGuilds.TryRemove(gld.Id, out LavalinkGuildConnection? con);
                    await this.guildConnectionRemoved.InvokeAsync(con, new GuildConnectionRemovedEventArgs());
                });
            }

            if (!string.IsNullOrWhiteSpace(e.SessionId) && e.Channel != null && this.VoiceStateUpdates.TryRemove(gld.Id, out TaskCompletionSource<VoiceStateUpdatedEventArgs>? xe))
            {
                xe.SetResult(e);
            }
        }

        return Task.CompletedTask;
    }

    private Task Discord_VoiceServerUpdated(DiscordClient client, VoiceServerUpdatedEventArgs e)
    {
        DiscordGuild gld = e.Guild;
        if (gld == null)
        {
            return Task.CompletedTask;
        }

        if (this.connectedGuilds.TryGetValue(e.Guild.Id, out LavalinkGuildConnection? lvlgc))
        {
            LavalinkVoiceUpdate lvlp = new(lvlgc.VoiceStateUpdate, e);
            _ = Task.Run(() => WsSendAsync(JsonConvert.SerializeObject(lvlp)));
        }

        if (this.VoiceServerUpdates.TryRemove(gld.Id, out TaskCompletionSource<VoiceServerUpdatedEventArgs>? xe))
        {
            xe.SetResult(e);
        }

        return Task.CompletedTask;
    }

    private async Task WsSendAsync(string payload)
    {
        this.Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsTx, "{WebsocketPayload}", payload);
        await this.WebSocket.SendMessageAsync(payload);
    }

    internal event NodeDisconnectedEventHandler NodeDisconnected;
}

// Kinda think this deserves another pack of instant noodles :^) -Emzi
// No I did it before in sai- alright then.
// Coroned noodles
