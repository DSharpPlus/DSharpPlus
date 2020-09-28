﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Net.WebSocket;
using Emzi0767.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Lavalink
{
    internal delegate void NodeDisconnectedEventHandler(LavalinkNodeConnection node);

    /// <summary>
    /// Represents a connection to a Lavalink node.
    /// </summary>
    public sealed class LavalinkNodeConnection
    {
        /// <summary>
        /// Triggered whenever Lavalink WebSocket throws an exception.
        /// </summary>
        public event AsyncEventHandler<LavalinkNodeConnection, SocketErrorEventArgs> LavalinkSocketErrored
        {
            add { this._lavalinkSocketError.Register(value); }
            remove { this._lavalinkSocketError.Unregister(value); }
        }
        private AsyncEvent<LavalinkNodeConnection, SocketErrorEventArgs> _lavalinkSocketError;

        /// <summary>
        /// Triggered when this node disconnects.
        /// </summary>
        public event AsyncEventHandler<LavalinkNodeConnection, NodeDisconnectedEventArgs> Disconnected
        {
            add { this._disconnected.Register(value); }
            remove { this._disconnected.Unregister(value); }
        }
        private AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs> _disconnected;

        /// <summary>
        /// Triggered when this node receives a statistics update.
        /// </summary>
        public event AsyncEventHandler<LavalinkNodeConnection, StatisticsReceivedEventArgs> StatisticsReceived
        {
            add { this._statsReceived.Register(value); }
            remove { this._statsReceived.Unregister(value); }
        }
        private AsyncEvent<LavalinkNodeConnection, StatisticsReceivedEventArgs> _statsReceived;

        /// <summary>
        /// Triggered whenever any of the players on this node is updated.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, PlayerUpdateEventArgs> PlayerUpdated
        {
            add { this._playerUpdated.Register(value); }
            remove { this._playerUpdated.Unregister(value); }
        }
        private AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs> _playerUpdated;

        /// <summary>
        /// Triggered whenever playback of a track starts.
        /// <para>This is only available for version 3.3.1 and greater.</para>
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackStartEventArgs> PlaybackStarted
        {
            add { this._playbackStarted.Register(value); }
            remove { this._playbackStarted.Unregister(value); }
        }
        private AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs> _playbackStarted;

        /// <summary>
        /// Triggered whenever playback of a track finishes.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackFinishEventArgs> PlaybackFinished
        {
            add { this._playbackFinished.Register(value); }
            remove { this._playbackFinished.Unregister(value); }
        }
        private AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs> _playbackFinished;

        /// <summary>
        /// Triggered whenever playback of a track gets stuck.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackStuckEventArgs> TrackStuck
        {
            add { this._trackStuck.Register(value); }
            remove { this._trackStuck.Unregister(value); }
        }
        private AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs> _trackStuck;

        /// <summary>
        /// Triggered whenever playback of a track encounters an error.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackExceptionEventArgs> TrackException
        {
            add { this._trackException.Register(value); }
            remove { this._trackException.Unregister(value); }
        }
        private AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs> _trackException;

        /// <summary>
        /// Gets the remote endpoint of this Lavalink node connection.
        /// </summary>
        public ConnectionEndpoint NodeEndpoint => this.Configuration.SocketEndpoint;

        /// <summary>
        /// Gets whether the client is connected to Lavalink.
        /// </summary>
        public bool IsConnected => !Volatile.Read(ref this._isDisposed);
        private bool _isDisposed = false;

        /// <summary>
        /// Gets the current resource usage statistics.
        /// </summary>
        public LavalinkStatistics Statistics { get; }

        /// <summary>
        /// Gets a dictionary of Lavalink guild connections for this node.
        /// </summary>
        public IReadOnlyDictionary<ulong, LavalinkGuildConnection> ConnectedGuilds { get; }
        internal ConcurrentDictionary<ulong, LavalinkGuildConnection> _connectedGuilds = new ConcurrentDictionary<ulong, LavalinkGuildConnection>();

        /// <summary>
        /// Gets the REST client for this Lavalink connection.
        /// </summary>
        public LavalinkRestClient Rest { get; }

        internal DiscordClient Discord { get; }
        internal LavalinkConfiguration Configuration { get; }
        internal DiscordVoiceRegion Region { get; }

        private IWebSocketClient WebSocket { get; set; }

        private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>> VoiceStateUpdates { get; }
        private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>> VoiceServerUpdates { get; }

        internal LavalinkNodeConnection(DiscordClient client, LavalinkConfiguration config)
        {
            this.Discord = client;
            this.Configuration = new LavalinkConfiguration(config);

            if (config.Region != null && this.Discord.VoiceRegions.Values.Contains(config.Region))
                this.Region = config.Region;

            this.ConnectedGuilds = new ReadOnlyConcurrentDictionary<ulong, LavalinkGuildConnection>(this._connectedGuilds);
            this.Statistics = new LavalinkStatistics();

            this._lavalinkSocketError = new AsyncEvent<LavalinkNodeConnection, SocketErrorEventArgs>("LAVALINK_SOCKET_ERROR", TimeSpan.Zero, this.Discord.EventErrorHandler);
            this._disconnected = new AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs>("LAVALINK_NODE_DISCONNECTED", TimeSpan.Zero, this.Discord.EventErrorHandler);
            this._statsReceived = new AsyncEvent<LavalinkNodeConnection, StatisticsReceivedEventArgs>("LAVALINK_STATS_RECEIVED", TimeSpan.Zero, this.Discord.EventErrorHandler);
            this._playerUpdated = new AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs>("LAVALINK_PLAYER_UPDATED", TimeSpan.Zero, this.Discord.EventErrorHandler);
            this._playbackStarted = new AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs>("LAVALINK_PLAYBACK_STARTED", TimeSpan.Zero, this.Discord.EventErrorHandler);
            this._playbackFinished = new AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs>("LAVALINK_PLAYBACK_FINISHED", TimeSpan.Zero, this.Discord.EventErrorHandler);
            this._trackStuck = new AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs>("LAVALINK_TRACK_STUCK", TimeSpan.Zero, this.Discord.EventErrorHandler);
            this._trackException = new AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs>("LAVALINK_TRACK_EXCEPTION", TimeSpan.Zero, this.Discord.EventErrorHandler);

            this.VoiceServerUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>>();
            this.VoiceStateUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>>();
            this.Discord.VoiceStateUpdated += this.Discord_VoiceStateUpdated;
            this.Discord.VoiceServerUpdated += this.Discord_VoiceServerUpdated;

            this.Rest = new LavalinkRestClient(this.Configuration, this.Discord);

            this.WebSocket = client.Configuration.WebSocketClientFactory(client.Configuration.Proxy);
            this.WebSocket.Connected += this.WebSocket_OnConnect;
            this.WebSocket.Disconnected += this.WebSocket_OnDisconnect;
            this.WebSocket.ExceptionThrown += this.WebSocket_OnException;
            this.WebSocket.MessageReceived += this.WebSocket_OnMessage;

            Volatile.Write(ref this._isDisposed, false);
        }

        /// <summary>
        /// Establishes a connection to the Lavalink node.
        /// </summary>
        /// <returns></returns>
        internal Task StartAsync()
        {
            if (this.Discord?.CurrentUser?.Id == null || this.Discord?.ShardCount == null)
                throw new InvalidOperationException("This operation requires the Discord client to be fully initialized.");

            this.WebSocket.AddDefaultHeader("Authorization", this.Configuration.Password);
            this.WebSocket.AddDefaultHeader("Num-Shards", this.Discord.ShardCount.ToString(CultureInfo.InvariantCulture));
            this.WebSocket.AddDefaultHeader("User-Id", this.Discord.CurrentUser.Id.ToString(CultureInfo.InvariantCulture));
            if (this.Configuration.ResumeKey != null)
                this.WebSocket.AddDefaultHeader("Resume-Key", this.Configuration.ResumeKey);

            return this.WebSocket.ConnectAsync(new Uri(this.Configuration.SocketEndpoint.ToWebSocketString()));
        }

        /// <summary>
        /// Stops this Lavalink node connection and frees resources.
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            foreach (var kvp in this._connectedGuilds)
                await kvp.Value.DisconnectAsync().ConfigureAwait(false);

            this.NodeDisconnected?.Invoke(this);

            Volatile.Write(ref this._isDisposed, true);
            await this.WebSocket.DisconnectAsync().ConfigureAwait(false);
            await this._disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this)).ConfigureAwait(false);
        }

        /// <summary>
        /// Connects this Lavalink node to specified Discord channel.
        /// </summary>
        /// <param name="channel">Voice channel to connect to.</param>
        /// <returns>Channel connection, which allows for playback control.</returns>
        public async Task<LavalinkGuildConnection> ConnectAsync(DiscordChannel channel)
        {
            if (this._connectedGuilds.ContainsKey(channel.Guild.Id))
                return this._connectedGuilds[channel.Guild.Id];

            if (channel.Guild == null || channel.Type != ChannelType.Voice)
                throw new ArgumentException("Invalid channel specified.", nameof(channel));

            var vstut = new TaskCompletionSource<VoiceStateUpdateEventArgs>();
            var vsrut = new TaskCompletionSource<VoiceServerUpdateEventArgs>();
            this.VoiceStateUpdates[channel.Guild.Id] = vstut;
            this.VoiceServerUpdates[channel.Guild.Id] = vsrut;

            var vsd = new VoiceDispatch
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
            var vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
            await (channel.Discord as DiscordClient).WsSendAsync(vsj).ConfigureAwait(false);
            var vstu = await vstut.Task.ConfigureAwait(false);
            var vsru = await vsrut.Task.ConfigureAwait(false);
            await this.SendPayloadAsync(new LavalinkVoiceUpdate(vstu, vsru)).ConfigureAwait(false);

            var con = new LavalinkGuildConnection(this, channel, vstu);
            con.ChannelDisconnected += this.Con_ChannelDisconnected;
            con.PlayerUpdated += (s, e) => this._playerUpdated.InvokeAsync(s, e);
            con.PlaybackStarted += (s, e) => this._playbackStarted.InvokeAsync(s, e);
            con.PlaybackFinished += (s, e) => this._playbackFinished.InvokeAsync(s, e);
            con.TrackStuck += (s, e) => this._trackStuck.InvokeAsync(s, e);
            con.TrackException += (s, e) => this._trackException.InvokeAsync(s, e);
            this._connectedGuilds[channel.Guild.Id] = con;

            return con;
        }

        /// <summary>
        /// Gets a Lavalink connection to specified Discord channel.
        /// </summary>
        /// <param name="guild">Guild to get connection for.</param>
        /// <returns>Channel connection, which allows for playback control.</returns>
        public LavalinkGuildConnection GetGuildConnection(DiscordGuild guild)
            => this._connectedGuilds.TryGetValue(guild.Id, out LavalinkGuildConnection lgc) && lgc.IsConnected ? lgc : null;

        internal async Task SendPayloadAsync(LavalinkPayload payload)
            => await this.WsSendAsync(JsonConvert.SerializeObject(payload, Formatting.None)).ConfigureAwait(false);

        private async Task WebSocket_OnMessage(IWebSocketClient client, SocketMessageEventArgs e)
        {
            if (!(e is SocketTextMessageEventArgs et))
            {
                this.Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, "Lavalink sent binary data - unable to process");
                return;
            }

            this.Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsRx, et.Message);

            var json = et.Message;
            var jsonData = JObject.Parse(json);
            switch (jsonData["op"].ToString())
            {
                case "playerUpdate":
                    var gid = (ulong)jsonData["guildId"];
                    var state = jsonData["state"].ToObject<LavalinkState>();
                    if (this._connectedGuilds.TryGetValue(gid, out var lvl))
                        await lvl.InternalUpdatePlayerStateAsync(state).ConfigureAwait(false);
                    break;

                case "stats":
                    var statsRaw = jsonData.ToObject<LavalinkStats>();
                    this.Statistics.Update(statsRaw);
                    await this._statsReceived.InvokeAsync(this, new StatisticsReceivedEventArgs(this.Statistics)).ConfigureAwait(false);
                    break;

                case "event":
                    var evtype = jsonData["type"].ToObject<EventType>();
                    var guildId = (ulong)jsonData["guildId"];
                    switch (evtype)
                    {
                        case EventType.TrackStartEvent:
                            if (this._connectedGuilds.TryGetValue(guildId, out var lvl_evtst))
                                await lvl_evtst.InternalPlaybackStartedAsync(jsonData["track"].ToString()).ConfigureAwait(false);
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
                            if (this._connectedGuilds.TryGetValue(guildId, out var lvl_evtf))
                                await lvl_evtf.InternalPlaybackFinishedAsync(new TrackFinishData { Track = jsonData["track"].ToString(), Reason = reason }).ConfigureAwait(false);
                            break;

                        case EventType.TrackStuckEvent:
                            if (this._connectedGuilds.TryGetValue(guildId, out var lvl_evts))
                                await lvl_evts.InternalTrackStuckAsync(new TrackStuckData { Track = jsonData["track"].ToString(), Threshold = (long)jsonData["thresholdMs"] }).ConfigureAwait(false);
                            break;

                        case EventType.TrackExceptionEvent:
                            if (this._connectedGuilds.TryGetValue(guildId, out var lvl_evte))
                                await lvl_evte.InternalTrackExceptionAsync(new TrackExceptionData { Track = jsonData["track"].ToString(), Error = jsonData["error"].ToString() }).ConfigureAwait(false);
                            break;

                        case EventType.WebSocketClosedEvent:
                            if (this._connectedGuilds.TryGetValue(guildId, out var lvl_ewsce))
                            {
                                lvl_ewsce.VoiceWsDisconnectTcs.SetResult(true);
                                await lvl_ewsce.InternalWebSocketClosedAsync(new WebSocketCloseEventArgs(jsonData["code"].ToObject<int>(), jsonData["reason"].ToString(), jsonData["byRemote"].ToObject<bool>())).ConfigureAwait(false);
                            }
                            break;
                    }
                    break;
            }
        }

        private Task WebSocket_OnException(IWebSocketClient client, SocketErrorEventArgs e)
            => this._lavalinkSocketError.InvokeAsync(this, new SocketErrorEventArgs { Exception = e.Exception });

        private async Task WebSocket_OnDisconnect(IWebSocketClient client, SocketCloseEventArgs e)
        {
            if (this.IsConnected && e.CloseCode != 1001 && e.CloseCode != -1)
            {
                this.Discord.Logger.LogWarning(LavalinkEvents.LavalinkConnectionClosed, "Connection broken ({0}, '{1}'), reconnecting", e.CloseCode, e.CloseMessage);
                this.WebSocket = this.Discord.Configuration.WebSocketClientFactory(this.Discord.Configuration.Proxy);
                this.WebSocket.Connected += this.WebSocket_OnConnect;
                this.WebSocket.Disconnected += this.WebSocket_OnDisconnect;
                this.WebSocket.ExceptionThrown += this.WebSocket_OnException;
                this.WebSocket.MessageReceived += this.WebSocket_OnMessage;

                this.WebSocket.AddDefaultHeader("Authorization", this.Configuration.Password);
                this.WebSocket.AddDefaultHeader("Num-Shards", this.Discord.ShardCount.ToString(CultureInfo.InvariantCulture));
                this.WebSocket.AddDefaultHeader("User-Id", this.Discord.CurrentUser.Id.ToString(CultureInfo.InvariantCulture));
                if (this.Configuration.ResumeKey != null)
                    this.WebSocket.AddDefaultHeader("Resume-Key", this.Configuration.ResumeKey);

                await this.WebSocket.ConnectAsync(new Uri(this.Configuration.SocketEndpoint.ToWebSocketString()));
            }
            else if (e.CloseCode != 1001 && e.CloseCode != -1)
            {
                this.Discord.Logger.LogInformation(LavalinkEvents.LavalinkConnectionClosed, "Connection closed ({0}, '{1}')", e.CloseCode, e.CloseMessage);
                this.NodeDisconnected?.Invoke(this);
                await this._disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this)).ConfigureAwait(false);
            }
            else
            {
                Volatile.Write(ref this._isDisposed, true);
                this.Discord.Logger.LogWarning(LavalinkEvents.LavalinkConnectionClosed, "Lavalink died");
                foreach (var kvp in this._connectedGuilds)
                {
                    await kvp.Value.SendVoiceUpdateAsync().ConfigureAwait(false);
                    _ = this._connectedGuilds.TryRemove(kvp.Key, out _);
                }
                this.NodeDisconnected?.Invoke(this);
                await this._disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this)).ConfigureAwait(false);
            }
        }

        private async Task WebSocket_OnConnect(IWebSocketClient client, SocketEventArgs ea)
        {
            this.Discord.Logger.LogDebug(LavalinkEvents.LavalinkConnected, "Connection to Lavalink node established");

            if (this.Configuration.ResumeKey != null)
                await this.SendPayloadAsync(new LavalinkConfigureResume(this.Configuration.ResumeKey, this.Configuration.ResumeTimeout)).ConfigureAwait(false);
        }

        private void Con_ChannelDisconnected(LavalinkGuildConnection con)
            => this._connectedGuilds.TryRemove(con.GuildId, out _);

        private Task Discord_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            var gld = e.Guild;
            if (gld == null)
                return Task.CompletedTask;

            if (e.User == null)
                return Task.CompletedTask;

            if (e.User.Id == this.Discord.CurrentUser.Id)
            {
                if (this._connectedGuilds.TryGetValue(e.Guild.Id, out var lvlgc))
                    lvlgc.VoiceStateUpdate = e;

                if (e.After.Channel == null && this.IsConnected && this._connectedGuilds.ContainsKey(gld.Id))
                {
                    _ = Task.Run(async () =>
                    {
                        var delayTask = Task.Delay(this.Configuration.WebSocketCloseTimeout);
                        var tcs = lvlgc.VoiceWsDisconnectTcs.Task;
                        _ = await Task.WhenAny(delayTask, tcs).ConfigureAwait(false);

                        await lvlgc.DisconnectInternalAsync(false, true).ConfigureAwait(false);
                        _ = this._connectedGuilds.TryRemove(gld.Id, out _);
                    });
                }

                if (!string.IsNullOrWhiteSpace(e.SessionId) && e.Channel != null && this.VoiceStateUpdates.TryRemove(gld.Id, out var xe))
                    xe.SetResult(e);
            }

            return Task.CompletedTask;
        }

        private Task Discord_VoiceServerUpdated(DiscordClient client, VoiceServerUpdateEventArgs e)
        {
            var gld = e.Guild;
            if (gld == null)
                return Task.CompletedTask;

            if (this._connectedGuilds.TryGetValue(e.Guild.Id, out var lvlgc))
            {
                var lvlp = new LavalinkVoiceUpdate(lvlgc.VoiceStateUpdate, e);
                _ = Task.Run(() => this.WsSendAsync(JsonConvert.SerializeObject(lvlp)));
            }

            if (this.VoiceServerUpdates.TryRemove(gld.Id, out var xe))
                xe.SetResult(e);

            return Task.CompletedTask;
        }

        private async Task WsSendAsync(string payload)
        {
            this.Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsTx, payload);
            await this.WebSocket.SendMessageAsync(payload).ConfigureAwait(false);
        }

        internal event NodeDisconnectedEventHandler NodeDisconnected;
    }
}

// Kinda think this deserves another pack of instant noodles :^) -Emzi
// No I did it before in sai- alright then.
