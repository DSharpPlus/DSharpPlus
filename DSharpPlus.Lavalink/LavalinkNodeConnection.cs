// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Net.Serialization;
using DSharpPlus.Net.WebSocket;
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
        private readonly AsyncEvent<LavalinkNodeConnection, SocketErrorEventArgs> _lavalinkSocketError;

        /// <summary>
        /// Triggered when this node disconnects.
        /// </summary>
        public event AsyncEventHandler<LavalinkNodeConnection, NodeDisconnectedEventArgs> Disconnected
        {
            add { this._disconnected.Register(value); }
            remove { this._disconnected.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs> _disconnected;

        /// <summary>
        /// Triggered when this node receives a statistics update.
        /// </summary>
        public event AsyncEventHandler<LavalinkNodeConnection, StatisticsReceivedEventArgs> StatisticsReceived
        {
            add { this._statsReceived.Register(value); }
            remove { this._statsReceived.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkNodeConnection, StatisticsReceivedEventArgs> _statsReceived;

        /// <summary>
        /// Triggered whenever any of the players on this node is updated.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, PlayerUpdateEventArgs> PlayerUpdated
        {
            add { this._playerUpdated.Register(value); }
            remove { this._playerUpdated.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs> _playerUpdated;

        /// <summary>
        /// Triggered whenever playback of a track starts.
        /// <para>This is only available for version 3.3.1 and greater.</para>
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackStartEventArgs> PlaybackStarted
        {
            add { this._playbackStarted.Register(value); }
            remove { this._playbackStarted.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs> _playbackStarted;

        /// <summary>
        /// Triggered whenever playback of a track finishes.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackFinishEventArgs> PlaybackFinished
        {
            add { this._playbackFinished.Register(value); }
            remove { this._playbackFinished.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs> _playbackFinished;

        /// <summary>
        /// Triggered whenever playback of a track gets stuck.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackStuckEventArgs> TrackStuck
        {
            add { this._trackStuck.Register(value); }
            remove { this._trackStuck.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs> _trackStuck;

        /// <summary>
        /// Triggered whenever playback of a track encounters an error.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackExceptionEventArgs> TrackException
        {
            add { this._trackException.Register(value); }
            remove { this._trackException.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs> _trackException;

        /// <summary>
        /// Triggered whenever a new guild connection is created.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, GuildConnectionCreatedEventArgs> GuildConnectionCreated
        {
            add { this._guildConnectionCreated.Register(value); }
            remove { this._guildConnectionCreated.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, GuildConnectionCreatedEventArgs> _guildConnectionCreated;

        /// <summary>
        /// Triggered whenever a guild connection is removed.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, GuildConnectionRemovedEventArgs> GuildConnectionRemoved
        {
            add { this._guildConnectionRemoved.Register(value); }
            remove { this._guildConnectionRemoved.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, GuildConnectionRemovedEventArgs> _guildConnectionRemoved;

        /// <summary>
        /// Gets the remote endpoint of this Lavalink node connection.
        /// </summary>
        public ConnectionEndpoint NodeEndpoint => this.Configuration.SocketEndpoint;

        /// <summary>
        /// Gets whether the client is connected to Lavalink.
        /// </summary>
        public bool IsConnected => !Volatile.Read(ref this._isDisposed);
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
            this.Discord = client;
            this.Parent = extension;
            this.Configuration = new LavalinkConfiguration(config);

            if (config.Region != null && this.Discord.VoiceRegions.Values.Contains(config.Region))
                this.Region = config.Region;

            this.ConnectedGuilds = new ReadOnlyConcurrentDictionary<ulong, LavalinkGuildConnection>(this._connectedGuilds);
            this.Statistics = new LavalinkStatistics();

            this._lavalinkSocketError = new AsyncEvent<LavalinkNodeConnection, SocketErrorEventArgs>("LAVALINK_SOCKET_ERROR", this.Discord.EventErrorHandler);
            this._disconnected = new AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs>("LAVALINK_NODE_DISCONNECTED", this.Discord.EventErrorHandler);
            this._statsReceived = new AsyncEvent<LavalinkNodeConnection, StatisticsReceivedEventArgs>("LAVALINK_STATS_RECEIVED", this.Discord.EventErrorHandler);
            this._playerUpdated = new AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs>("LAVALINK_PLAYER_UPDATED", this.Discord.EventErrorHandler);
            this._playbackStarted = new AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs>("LAVALINK_PLAYBACK_STARTED", this.Discord.EventErrorHandler);
            this._playbackFinished = new AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs>("LAVALINK_PLAYBACK_FINISHED", this.Discord.EventErrorHandler);
            this._trackStuck = new AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs>("LAVALINK_TRACK_STUCK", this.Discord.EventErrorHandler);
            this._trackException = new AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs>("LAVALINK_TRACK_EXCEPTION", this.Discord.EventErrorHandler);
            this._guildConnectionCreated = new AsyncEvent<LavalinkGuildConnection, GuildConnectionCreatedEventArgs>("LAVALINK_GUILD_CONNECTION_CREATED", this.Discord.EventErrorHandler);
            this._guildConnectionRemoved = new AsyncEvent<LavalinkGuildConnection, GuildConnectionRemovedEventArgs>("LAVALINK_GUILD_CONNECTION_REMOVED", this.Discord.EventErrorHandler);

            this.VoiceServerUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>>();
            this.VoiceStateUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>>();
            this.Discord.VoiceStateUpdated += this.Discord_VoiceStateUpdated;
            this.Discord.VoiceServerUpdated += this.Discord_VoiceServerUpdated;

            this.Rest = new LavalinkRestClient(this.Configuration, this.Discord);

            Volatile.Write(ref this._isDisposed, false);
        }

        /// <summary>
        /// Establishes a connection to the Lavalink node.
        /// </summary>
        /// <returns></returns>
        internal async Task StartAsync()
        {
            if (this.Discord?.CurrentUser?.Id == null || this.Discord?.ShardCount == null)
                throw new InvalidOperationException("This operation requires the Discord client to be fully initialized.");

            this.WebSocket = this.Discord.Configuration.WebSocketClientFactory(this.Discord.Configuration.Proxy);
            this.WebSocket.Connected += this.WebSocket_OnConnect;
            this.WebSocket.Disconnected += this.WebSocket_OnDisconnect;
            this.WebSocket.ExceptionThrown += this.WebSocket_OnException;
            this.WebSocket.MessageReceived += this.WebSocket_OnMessage;

            this.WebSocket.AddDefaultHeader("Authorization", this.Configuration.Password);
            this.WebSocket.AddDefaultHeader("Num-Shards", this.Discord.ShardCount.ToString(CultureInfo.InvariantCulture));
            this.WebSocket.AddDefaultHeader("User-Id", this.Discord.CurrentUser.Id.ToString(CultureInfo.InvariantCulture));
            this.WebSocket.AddDefaultHeader("Client-Name", "DSharpPlus.Lavalink");
            if (this.Configuration.ResumeKey != null)
                this.WebSocket.AddDefaultHeader("Resume-Key", this.Configuration.ResumeKey);

            do
            {
                try
                {
                    if (this._backoff != 0)
                    {
                        await Task.Delay(this._backoff);
                        this._backoff = Math.Min(this._backoff * 2, MaximumBackoff);
                    }
                    else
                    {
                        this._backoff = MinimumBackoff;
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
                    if (!this.Configuration.SocketAutoReconnect || this._backoff == MaximumBackoff)
                    {
                        this.Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, ex, "Failed to connect to Lavalink.");
                        throw ex;
                    }
                    else
                    {
                        this.Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, ex, $"Failed to connect to Lavalink, retrying in {this._backoff} ms.");
                    }
                }
            }
            while (this.Configuration.SocketAutoReconnect);

            Volatile.Write(ref this._isDisposed, false);
        }

        /// <summary>
        /// Stops this Lavalink node connection and frees resources.
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            foreach (var kvp in this._connectedGuilds)
                await kvp.Value.DisconnectAsync();

            this.NodeDisconnected?.Invoke(this);

            Volatile.Write(ref this._isDisposed, true);
            await this.WebSocket.DisconnectAsync();
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
            if (this._connectedGuilds.ContainsKey(channel.Guild.Id))
                return this._connectedGuilds[channel.Guild.Id];

            if (channel.Guild == null || (channel.Type != ChannelType.Voice && channel.Type != ChannelType.Stage))
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
            await (channel.Discord as DiscordClient).SendRawPayloadAsync(vsj);
            var vstu = await vstut.Task;
            var vsru = await vsrut.Task;
            await this.SendPayloadAsync(new LavalinkVoiceUpdate(vstu, vsru));

            var con = new LavalinkGuildConnection(this, channel, vstu);
            con.ChannelDisconnected += this.Con_ChannelDisconnected;
            con.PlayerUpdated += (s, e) => this._playerUpdated.InvokeAsync(s, e);
            con.PlaybackStarted += (s, e) => this._playbackStarted.InvokeAsync(s, e);
            con.PlaybackFinished += (s, e) => this._playbackFinished.InvokeAsync(s, e);
            con.TrackStuck += (s, e) => this._trackStuck.InvokeAsync(s, e);
            con.TrackException += (s, e) => this._trackException.InvokeAsync(s, e);
            this._connectedGuilds[channel.Guild.Id] = con;
            await this._guildConnectionCreated.InvokeAsync(con, new GuildConnectionCreatedEventArgs());

            return con;
        }

        /// <summary>
        /// Gets a Lavalink connection to specified Discord channel.
        /// </summary>
        /// <param name="guild">Guild to get connection for.</param>
        /// <returns>Channel connection, which allows for playback control.</returns>
        public LavalinkGuildConnection GetGuildConnection(DiscordGuild guild)
            => this._connectedGuilds.TryGetValue(guild.Id, out var lgc) && lgc.IsConnected ? lgc : null;

        internal async Task SendPayloadAsync(LavalinkPayload payload)
            => await this.WsSendAsync(JsonConvert.SerializeObject(payload, Formatting.None));

        private async Task WebSocket_OnMessage(IWebSocketClient client, SocketMessageEventArgs e)
        {
            if (e is not SocketTextMessageEventArgs et)
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
                    var state = jsonData["state"].ToDiscordObject<LavalinkState>();
                    if (this._connectedGuilds.TryGetValue(gid, out var lvl))
                        await lvl.InternalUpdatePlayerStateAsync(state);
                    break;

                case "stats":
                    var statsRaw = jsonData.ToDiscordObject<LavalinkStats>();
                    this.Statistics.Update(statsRaw);
                    await this._statsReceived.InvokeAsync(this, new StatisticsReceivedEventArgs(this.Statistics));
                    break;

                case "event":
                    var evtype = jsonData["type"].ToDiscordObject<EventType>();
                    var guildId = (ulong)jsonData["guildId"];
                    switch (evtype)
                    {
                        case EventType.TrackStartEvent:
                            if (this._connectedGuilds.TryGetValue(guildId, out var lvl_evtst))
                                await lvl_evtst.InternalPlaybackStartedAsync(jsonData["track"].ToString());
                            break;

                        case EventType.TrackEndEvent:
                            var reason = TrackEndReason.Cleanup;
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
                                await lvl_evtf.InternalPlaybackFinishedAsync(new TrackFinishData { Track = jsonData["track"].ToString(), Reason = reason });
                            break;

                        case EventType.TrackStuckEvent:
                            if (this._connectedGuilds.TryGetValue(guildId, out var lvl_evts))
                                await lvl_evts.InternalTrackStuckAsync(new TrackStuckData { Track = jsonData["track"].ToString(), Threshold = (long)jsonData["thresholdMs"] });
                            break;

                        case EventType.TrackExceptionEvent:
                            if (this._connectedGuilds.TryGetValue(guildId, out var lvl_evte))
                                await lvl_evte.InternalTrackExceptionAsync(new TrackExceptionData { Track = jsonData["track"].ToString(), Error = jsonData["error"].ToString() });
                            break;

                        case EventType.WebSocketClosedEvent:
                            if (this._connectedGuilds.TryGetValue(guildId, out var lvl_ewsce))
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
            => this._lavalinkSocketError.InvokeAsync(this, new SocketErrorEventArgs { Exception = e.Exception });

        private async Task WebSocket_OnDisconnect(IWebSocketClient client, SocketCloseEventArgs e)
        {
            if (this.IsConnected && e.CloseCode != 1001 && e.CloseCode != -1)
            {
                this.Discord.Logger.LogWarning(LavalinkEvents.LavalinkConnectionClosed, "Connection broken ({CloseCode}, '{CloseMessage}'), reconnecting", e.CloseCode, e.CloseMessage);
                await this._disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this, false));

                if (this.Configuration.SocketAutoReconnect)
                    await this.StartAsync();
            }
            else if (e.CloseCode != 1001 && e.CloseCode != -1)
            {
                this.Discord.Logger.LogInformation(LavalinkEvents.LavalinkConnectionClosed, "Connection closed ({CloseCode}, '{CloseMessage}')", e.CloseCode, e.CloseMessage);
                this.NodeDisconnected?.Invoke(this);
                await this._disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this, true));
            }
            else
            {
                Volatile.Write(ref this._isDisposed, true);
                this.Discord.Logger.LogWarning(LavalinkEvents.LavalinkConnectionClosed, "Lavalink died");
                foreach (var kvp in this._connectedGuilds)
                {
                    await kvp.Value.SendVoiceUpdateAsync();
                    _ = this._connectedGuilds.TryRemove(kvp.Key, out var con);
                    await this._guildConnectionRemoved.InvokeAsync(con, new GuildConnectionRemovedEventArgs());
                }
                this.NodeDisconnected?.Invoke(this);
                await this._disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this, false));

                if (this.Configuration.SocketAutoReconnect)
                    await this.StartAsync();
            }
        }

        private async Task WebSocket_OnConnect(IWebSocketClient client, SocketEventArgs ea)
        {
            this.Discord.Logger.LogDebug(LavalinkEvents.LavalinkConnected, "Connection to Lavalink node established");
            this._backoff = 0;

            if (this.Configuration.ResumeKey != null)
                await this.SendPayloadAsync(new LavalinkConfigureResume(this.Configuration.ResumeKey, this.Configuration.ResumeTimeout));
        }

        private async void Con_ChannelDisconnected(LavalinkGuildConnection con)
        {
            this._connectedGuilds.TryRemove(con.GuildId, out con);
            await this._guildConnectionRemoved.InvokeAsync(con, new GuildConnectionRemovedEventArgs());
        }

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
                        _ = await Task.WhenAny(delayTask, tcs);

                        await lvlgc.DisconnectInternalAsync(false, true);
                        _ = this._connectedGuilds.TryRemove(gld.Id, out var con);
                        await this._guildConnectionRemoved.InvokeAsync(con, new GuildConnectionRemovedEventArgs());
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
            await this.WebSocket.SendMessageAsync(payload);
        }

        internal event NodeDisconnectedEventHandler NodeDisconnected;
    }
}

// Kinda think this deserves another pack of instant noodles :^) -Emzi
// No I did it before in sai- alright then.
// Coroned noodles
