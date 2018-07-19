using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.Net.Udp;
using DSharpPlus.Net.WebSocket;
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
        public event AsyncEventHandler<SocketErrorEventArgs> LavalinkSocketErrored
        {
            add { this._lavalinkSocketError.Register(value); }
            remove { this._lavalinkSocketError.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _lavalinkSocketError;

        /// <summary>
        /// Gets the remote endpoint of this Lavalink node connection.
        /// </summary>
        public ConnectionEndpoint NodeEndpoint => this.Configuration.SocketEndpoint;

        /// <summary>
        /// Gets whether this channel is still connected.
        /// </summary>
        public bool IsConnected => !Volatile.Read(ref this._isDisposed);
        private bool _isDisposed = false;

        private DiscordClient Discord { get; }
        private LavalinkConfiguration Configuration { get; }
        private ConcurrentDictionary<ulong, LavalinkGuildConnection> ConnectedGuilds { get; }

        private BaseWebSocketClient WebSocket { get; set; }
        private HttpClient Rest { get; }

        private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>> VoiceStateUpdates { get; }
        private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>> VoiceServerUpdates { get; }

        private static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);

        internal LavalinkNodeConnection(DiscordClient client, LavalinkConfiguration config)
        {
            this.Discord = client;
            this.Configuration = new LavalinkConfiguration(config);
            this.ConnectedGuilds = new ConcurrentDictionary<ulong, LavalinkGuildConnection>();

            this._lavalinkSocketError = new AsyncEvent<SocketErrorEventArgs>(this.Discord.EventErrorHandler, "LAVALINK_SOCKET_ERROR");

            this.VoiceServerUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>>();
            this.VoiceStateUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>>();
            this.Discord.VoiceStateUpdated += this.Discord_VoiceStateUpdated;
            this.Discord.VoiceServerUpdated += this.Discord_VoiceServerUpdated;

            var httphandler = new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseProxy = client.Configuration.Proxy != null
            };
            if (httphandler.UseProxy) // because mono doesn't implement this properly
                httphandler.Proxy = client.Configuration.Proxy;

            this.Rest = new HttpClient(httphandler)
            {
                BaseAddress = new Uri($"http://{this.Configuration.RestEndpoint}/loadtracks")
            };
            this.Rest.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"DSharpPlus.LavaLink/{client.VersionString}");
            this.Rest.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", this.Configuration.Password);

            this.WebSocket = client.Configuration.WebSocketClientFactory(client.Configuration.Proxy);
            this.WebSocket.OnConnect += this.WebSocket_OnConnect;
            this.WebSocket.OnDisconnect += this.WebSocket_OnDisconnect;
            this.WebSocket.OnError += this.WebSocket_OnError;
            this.WebSocket.OnMessage += this.WebSocket_OnMessage;

            Volatile.Write(ref this._isDisposed, false);
        }

        /// <summary>
        /// Establishes a connection to the Lavalink node.
        /// </summary>
        /// <returns></returns>
        internal Task StartAsync()
        {
            // set up event passthrough
            
            return this.WebSocket.ConnectAsync(new Uri($"ws://{this.Configuration.SocketEndpoint}/"), new Dictionary<string, string>()
            {
                ["Authorization"] = this.Configuration.Password,
                ["Num-Shards"] = this.Discord.ShardCount.ToString(CultureInfo.InvariantCulture),
                ["User-Id"] = this.Discord.CurrentUser.Id.ToString(CultureInfo.InvariantCulture)
            });
        }

        /// <summary>
        /// Stops this Lavalink node connection and frees resources.
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            foreach (var kvp in this.ConnectedGuilds)
                kvp.Value.Disconnect();

            if (this.NodeDisconnected != null)
                this.NodeDisconnected(this);

            Volatile.Write(ref this._isDisposed, true);
            await this.WebSocket.DisconnectAsync(null).ConfigureAwait(false);
        }

        /// <summary>
        /// Connects this Lavalink node to specified Discord channel.
        /// </summary>
        /// <param name="channel">Voice channel to connect to.</param>
        /// <returns>Channel connection, which allows for playback control.</returns>
        public async Task<LavalinkGuildConnection> ConnectAsync(DiscordChannel channel)
        {
            if (this.ConnectedGuilds.ContainsKey(channel.Guild.Id))
                return this.ConnectedGuilds[channel.Guild.Id];

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
            (channel.Discord as DiscordClient)._webSocketClient.SendMessage(vsj);
            var vstu = await vstut.Task.ConfigureAwait(false);
            var vsru = await vsrut.Task.ConfigureAwait(false);
            this.SendPayload(new LavalinkVoiceUpdate(vstu, vsru));

            var con = new LavalinkGuildConnection(this, channel, vstu);
            con.ChannelDisconnected += this.Con_ChannelDisconnected;
            this.ConnectedGuilds[channel.Guild.Id] = con;

            return con;
        }

        /// <summary>
        /// Gets a Lavalink connection to specified Discord channel.
        /// </summary>
        /// <param name="channel">Channel to get connection for.</param>
        /// <returns>Channel connection, which allows for playback control.</returns>
        public LavalinkGuildConnection GetConnection(DiscordChannel channel)
            => this.ConnectedGuilds[channel.Guild.Id];

        public Task<IEnumerable<LavalinkTrack>> GetTracksAsync(Uri uri)
        {
            var str = WebUtility.UrlEncode(uri.ToString());
            var tracksUri = new Uri($"http://{this.Configuration.RestEndpoint}/loadtracks?identifier={str}");
            return this.InternalResolveTracksAsync(tracksUri);
        }

#if !NETSTANDARD1_1
        public Task<IEnumerable<LavalinkTrack>> GetTracksAsync(FileInfo file)
        {
            var str = WebUtility.UrlEncode(file.FullName);
            var tracksUri = new Uri($"http://{this.Configuration.RestEndpoint}/loadtracks?identifier={str}");
            return this.InternalResolveTracksAsync(tracksUri);
        }
#endif

        private async Task<IEnumerable<LavalinkTrack>> InternalResolveTracksAsync(Uri uri)
        {
            var json = "[]";
            using (var req = await this.Rest.GetAsync(uri).ConfigureAwait(false))
            using (var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(res, UTF8))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var jarr = JArray.Parse(json);
            var tracks = new List<LavalinkTrack>(jarr.Count);
            foreach (var jt in jarr)
            {
                var track = jt["info"].ToObject<LavalinkTrack>();
                track.Track = jt["track"].ToString();

                tracks.Add(track);
            }

            return tracks;
        }

        internal void SendPayload(LavalinkPayload payload)
            => this.WebSocket.SendMessage(JsonConvert.SerializeObject(payload, Formatting.None));

        private Task WebSocket_OnMessage(SocketMessageEventArgs e)
        {
            this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "Lavalink", e.Message, DateTime.Now);
            return Task.Delay(0);
        }

        private Task WebSocket_OnError(SocketErrorEventArgs e)
            => this._lavalinkSocketError.InvokeAsync(new SocketErrorEventArgs(this.Discord) { Exception = e.Exception });

        private Task WebSocket_OnDisconnect(SocketCloseEventArgs e)
        {
            if (this.IsConnected)
            {
                this.Discord.DebugLogger.LogMessage(LogLevel.Warning, "Lavalink", "Connection broken; re-establishing...", DateTime.Now);
                this.WebSocket = this.Discord.Configuration.WebSocketClientFactory(this.Discord.Configuration.Proxy);
                this.WebSocket.OnConnect += this.WebSocket_OnConnect;
                this.WebSocket.OnDisconnect += this.WebSocket_OnDisconnect;
                this.WebSocket.OnError += this.WebSocket_OnError;
                this.WebSocket.OnMessage += this.WebSocket_OnMessage;
                return this.WebSocket.ConnectAsync(new Uri($"ws://{this.Configuration.SocketEndpoint}/"), new Dictionary<string, string>()
                {
                    ["Authorization"] = this.Configuration.Password,
                    ["Num-Shards"] = this.Discord.ShardCount.ToString(CultureInfo.InvariantCulture),
                    ["User-Id"] = this.Discord.CurrentUser.Id.ToString(CultureInfo.InvariantCulture)
                });
            }
            else
            {
                this.Discord.DebugLogger.LogMessage(LogLevel.Info, "Lavalink", "Connection closed", DateTime.Now);
                return Task.Delay(0);
            }
        }

        private Task WebSocket_OnConnect()
        {
            this.Discord.DebugLogger.LogMessage(LogLevel.Info, "Lavalink", "Connection established", DateTime.Now);

            return Task.Delay(0);
        }

        private void Con_ChannelDisconnected(LavalinkGuildConnection con)
            => this.ConnectedGuilds.TryRemove(con.GuildId, out _);

        private Task Discord_VoiceStateUpdated(VoiceStateUpdateEventArgs e)
        {
            var gld = e.Guild;
            if (gld == null)
                return Task.Delay(0);

            if (e.User == null)
                return Task.Delay(0);

            if (e.User.Id == this.Discord.CurrentUser.Id && this.ConnectedGuilds.TryGetValue(e.Guild.Id, out var lvlgc))
                lvlgc.Channel = e.Channel;

            if (!string.IsNullOrWhiteSpace(e.SessionId) && e.User.Id == this.Discord.CurrentUser.Id && this.VoiceStateUpdates.ContainsKey(gld.Id))
            {
                this.VoiceStateUpdates.TryRemove(gld.Id, out var xe);
                xe.SetResult(e);
            }

            return Task.Delay(0);
        }

        private Task Discord_VoiceServerUpdated(VoiceServerUpdateEventArgs e)
        {
            var gld = e.Guild;
            if (gld == null)
                return Task.Delay(0);

            if (this.ConnectedGuilds.TryGetValue(e.Guild.Id, out var lvlgc))
            {
                var lvlp = new LavalinkVoiceUpdate(lvlgc.VoiceStateUpdate, e);
                this.WebSocket.SendMessage(JsonConvert.SerializeObject(lvlp));
            }

            if (this.VoiceServerUpdates.ContainsKey(gld.Id))
            {
                this.VoiceServerUpdates.TryRemove(gld.Id, out var xe);
                xe.SetResult(e);
            }

            return Task.Delay(0);
        }

        internal event NodeDisconnectedEventHandler NodeDisconnected;
    }
}

// Kinda think this deserves another pack of instant noodles :^) -Emzi