using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Udp;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VoiceNext.Codec;
using DSharpPlus.VoiceNext.EventArgs;
using DSharpPlus.VoiceNext.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.VoiceNext
{
    internal delegate void VoiceDisconnectedEventHandler(DiscordGuild guild);

    /// <summary>
    /// VoiceNext connection to a voice channel.
    /// </summary>
    public sealed class VoiceNextConnection : IDisposable
    {
        /// <summary>
        /// Triggered whenever a user speaks in the connected voice channel.
        /// </summary>
        public event AsyncEventHandler<UserSpeakingEventArgs> UserSpeaking
        {
            add { this._userSpeaking.Register(value); }
            remove { this._userSpeaking.Unregister(value); }
        }
        private AsyncEvent<UserSpeakingEventArgs> _userSpeaking;

        /// <summary>
        /// Triggered whenever a user leaves voice in the connected guild.
        /// </summary>
        public event AsyncEventHandler<VoiceUserLeaveEventArgs> UserLeft
        {
            add { this._userLeft.Register(value); }
            remove { this._userLeft.Unregister(value); }
        }
        private AsyncEvent<VoiceUserLeaveEventArgs> _userLeft;

#if !NETSTANDARD1_1
        /// <summary>
        /// Triggered whenever voice data is received from the connected voice channel.
        /// </summary>
        public event AsyncEventHandler<VoiceReceiveEventArgs> VoiceReceived
        {
            add { this._voiceReceived.Register(value); }
            remove { this._voiceReceived.Unregister(value); }
        }
        private AsyncEvent<VoiceReceiveEventArgs> _voiceReceived;
#endif

        /// <summary>
        /// Triggered whenever voice WebSocket throws an exception.
        /// </summary>
        public event AsyncEventHandler<SocketErrorEventArgs> VoiceSocketErrored
        {
            add { this._voiceSocketError.Register(value); }
            remove { this._voiceSocketError.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _voiceSocketError;

        internal event VoiceDisconnectedEventHandler VoiceDisconnected;

        private const string VOICE_MODE = "xsalsa20_poly1305";
        private static DateTime UnixEpoch { get { return _unixEpoch.Value; } }
        private static Lazy<DateTime> _unixEpoch;

        private DiscordClient Discord { get; }
        private DiscordGuild Guild { get; }
        private ConcurrentDictionary<uint, ulong> SSRCMap { get; }

        private BaseUdpClient UdpClient { get; }
        private BaseWebSocketClient VoiceWs { get; set; }
        private Task HeartbeatTask { get; set; }
        private int HeartbeatInterval { get; set; }
        private DateTime LastHeartbeat { get; set; }

        private CancellationTokenSource TokenSource { get; set; }
        private CancellationToken Token 
            => this.TokenSource.Token;

        internal VoiceServerUpdatePayload ServerData { get; set; }
        internal VoiceStateUpdatePayload StateData { get; set; }
        internal bool Resume { get; set; }

        private VoiceNextConfiguration Configuration { get; }
        private OpusCodec Opus { get; set; }
        private SodiumCodec Sodium { get; set; }
        private RtpCodec Rtp { get; set; }
        private double SynchronizerTicks { get; set; }
        private double SynchronizerResolution { get; set; }
        private double TickResolution { get; set; }

        private ushort Sequence { get; set; }
        private uint Timestamp { get; set; }
        private uint SSRC { get; set; }
        private byte[] Key { get; set; }
#if !NETSTANDARD1_1
        private IpEndpoint DiscoveredEndpoint { get; set; }
#endif
        internal ConnectionEndpoint ConnectionEndpoint { get; set; }

        private TaskCompletionSource<bool> ReadyWait { get; set; }
        private bool IsInitialized { get; set; }
        private bool IsDisposed { get; set; }

        private TaskCompletionSource<bool> PlayingWait { get; set; }
        private SemaphoreSlim PlaybackSemaphore { get; set; }

#if !NETSTANDARD1_1
        private Task ReceiverTask { get; set; }
        private CancellationTokenSource ReceiverTokenSource { get; set; }
        private CancellationToken ReceiverToken
            => this.ReceiverTokenSource.Token;
#endif

        /// <summary>
        /// Gets whether this connection is still playing audio.
        /// </summary>
        public bool IsPlaying 
            => this.PlaybackSemaphore.CurrentCount == 0 || (this.PlayingWait != null && !this.PlayingWait.Task.IsCompleted);

        /// <summary>
        /// Gets the websocket round-trip time in ms.
        /// </summary>
        public int Ping 
            => Volatile.Read(ref this._ping);

        private int _ping = 0;

        /// <summary>
        /// Gets the channel this voice client is connected to.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        internal VoiceNextConnection(DiscordClient client, DiscordGuild guild, DiscordChannel channel, VoiceNextConfiguration config, VoiceServerUpdatePayload server, VoiceStateUpdatePayload state)
        {
            this.Discord = client;
            this.Guild = guild;
            this.Channel = channel;
            this.SSRCMap = new ConcurrentDictionary<uint, ulong>();

            this._userSpeaking = new AsyncEvent<UserSpeakingEventArgs>(this.Discord.EventErrorHandler, "USER_SPEAKING");
            this._userLeft = new AsyncEvent<VoiceUserLeaveEventArgs>(this.Discord.EventErrorHandler, "USER_LEFT");
#if !NETSTANDARD1_1
            this._voiceReceived = new AsyncEvent<VoiceReceiveEventArgs>(this.Discord.EventErrorHandler, "VOICE_RECEIVED");
#endif
            this._voiceSocketError = new AsyncEvent<SocketErrorEventArgs>(this.Discord.EventErrorHandler, "VOICE_WS_ERROR");
            this.TokenSource = new CancellationTokenSource();

            this.Configuration = config;
            this.Opus = new OpusCodec(48000, 2, this.Configuration.VoiceApplication);
            this.Sodium = new SodiumCodec();
            this.Rtp = new RtpCodec();

            this.ServerData = server;
            this.StateData = state;

            var eps = this.ServerData.Endpoint;
            var epi = eps.LastIndexOf(':');
            var eph = string.Empty;
            var epp = 80;
            if (epi != -1)
            {
                eph = eps.Substring(0, epi);
                epp = int.Parse(eps.Substring(epi + 1));
            }
            else
            {
                eph = eps;
            }
            this.ConnectionEndpoint = new ConnectionEndpoint { Hostname = eph, Port = epp };

            this.ReadyWait = new TaskCompletionSource<bool>();
            this.IsInitialized = false;
            this.IsDisposed = false;

            this.PlayingWait = null;
            this.PlaybackSemaphore = new SemaphoreSlim(1, 1);

            this.UdpClient = this.Discord.Configuration.UdpClientFactory();
            this.VoiceWs = this.Discord.Configuration.WebSocketClientFactory(this.Discord.Configuration.Proxy);
            this.VoiceWs.OnDisconnect += this.VoiceWS_SocketClosed;
            this.VoiceWs.OnMessage += this.VoiceWS_SocketMessage;
            this.VoiceWs.OnConnect += this.VoiceWS_SocketOpened;
            this.VoiceWs.OnError += this.VoiceWs_SocketErrored;
        }

        static VoiceNextConnection()
        {
            _unixEpoch = new Lazy<DateTime>(() => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        }

        ~VoiceNextConnection()
        {
            this.Dispose();
        }

        /// <summary>
        /// Connects to the specified voice channel.
        /// </summary>
        /// <returns>A task representing the connection operation.</returns>
        internal Task ConnectAsync()
        {
            var gwuri = new UriBuilder
            {
                Scheme = "wss",
                Host = this.ConnectionEndpoint.Hostname,
                Query = "encoding=json&v=3"
            };

            return this.VoiceWs.ConnectAsync(gwuri.Uri);
        }

        internal Task ReconnectAsync()
            => this.VoiceWs.DisconnectAsync(new SocketCloseEventArgs(this.Discord));

        internal Task StartAsync()
        {
            // Let's announce our intentions to the server
            var vdp = new VoiceDispatch();

            if (!this.Resume)
            {
                vdp.OpCode = 0;
                vdp.Payload = new VoiceIdentifyPayload
                {
                    ServerId = this.ServerData.GuildId,
                    UserId = this.StateData.UserId.Value,
                    SessionId = this.StateData.SessionId,
                    Token = this.ServerData.Token
                };
                this.Resume = true;
            }
            else
            {
                vdp.OpCode = 7;
                vdp.Payload = new VoiceIdentifyPayload
                {
                    ServerId = this.ServerData.GuildId,
                    SessionId = this.StateData.SessionId,
                    Token = this.ServerData.Token
                };
            }
            var vdj = JsonConvert.SerializeObject(vdp, Formatting.None);
            this.VoiceWs.SendMessage(vdj);

            return Task.Delay(0);
        }

        internal Task WaitForReadyAsync() 
            => this.ReadyWait.Task;

        /// <summary>
        /// Encodes, encrypts, and sends the provided PCM data to the connected voice channel.
        /// </summary>
        /// <param name="pcmData">PCM data to encode, encrypt, and send.</param>
        /// <param name="blockSize">Millisecond length of the PCM data.</param>
        /// <param name="bitRate">Bitrate of the PCM data.</param>
        /// <returns>Task representing the sending operation.</returns>
        public async Task SendAsync(byte[] pcmData, int blockSize, int bitRate = 16)
        {
            if (!this.IsInitialized)
                throw new InvalidOperationException("The connection is not initialized");

            await this.PlaybackSemaphore.WaitAsync().ConfigureAwait(false);

            var rtp = this.Rtp.Encode(this.Sequence, this.Timestamp, this.SSRC);

            var dat = this.Opus.Encode(pcmData, 0, pcmData.Length, bitRate);
            dat = this.Sodium.Encode(dat, this.Rtp.MakeNonce(rtp), this.Key);
            dat = this.Rtp.Encode(rtp, dat);

            if (this.SynchronizerTicks == 0)
            {
                this.SynchronizerTicks = Stopwatch.GetTimestamp();
                this.SynchronizerResolution = (Stopwatch.Frequency * 0.02);
                this.TickResolution = 10_000_000.0 / Stopwatch.Frequency;
                this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"Timer accuracy: {Stopwatch.Frequency.ToString("#,##0", CultureInfo.InvariantCulture)}/{this.SynchronizerResolution.ToString(CultureInfo.InvariantCulture)} (high resolution? {Stopwatch.IsHighResolution})", DateTime.Now);
            }
            else
            {
                // Provided by Laura#0090 (214796473689178133); this is Python, but adaptable:
                // 
                // delay = max(0, self.delay + ((start_time + self.delay * loops) + - time.time()))
                // 
                // self.delay
                //   sample size
                // start_time
                //   time since streaming started
                // loops
                //   number of samples sent
                // time.time()
                //   DateTime.Now
                
                var cts = Math.Max(Stopwatch.GetTimestamp() - this.SynchronizerTicks, 0);
                if (cts < this.SynchronizerResolution)
                    await Task.Delay(TimeSpan.FromTicks((long)((this.SynchronizerResolution - cts) * this.TickResolution))).ConfigureAwait(false);

                this.SynchronizerTicks += this.SynchronizerResolution;
            }

            await this.SendSpeakingAsync(true).ConfigureAwait(false);
            await this.UdpClient.SendAsync(dat, dat.Length).ConfigureAwait(false);

            this.Sequence++;
            this.Timestamp += 48 * (uint)blockSize;

            this.PlaybackSemaphore.Release();
        }

#if !NETSTANDARD1_1
        private async Task VoiceReceiverTask()
        {
            var token = this.ReceiverToken;
            var client = this.UdpClient;
            while (!token.IsCancellationRequested)
            {
                if (client.DataAvailable <= 0)
                    continue;

                byte[] data = null, header = null;
                ushort seq = 0;
                uint ts = 0, ssrc = 0;
                try
                {
                    data = await client.ReceiveAsync().ConfigureAwait(false);

                    header = new byte[RtpCodec.SIZE_HEADER];
                    data = this.Rtp.Decode(data, header);

                    var nonce = this.Rtp.MakeNonce(header);
                    data = this.Sodium.Decode(data, nonce, this.Key);

                    // following is thanks to code from Eris
                    // https://github.com/abalabahaha/eris/blob/master/lib/voice/VoiceConnection.js#L623
                    var doff = 0;
                    this.Rtp.Decode(header, out seq, out ts, out ssrc, out var has_ext);
                    if (has_ext)
                    {
                        if (data[0] == 0xBE && data[1] == 0xDE)
                        {
                            // RFC 5285, 4.2 One-Byte header
                            // http://www.rfcreader.com/#rfc5285_line186

                            var hlen = data[2] << 8 | data[3];
                            var i = 4;
                            for (; i < hlen + 4; i++)
                            {
                                var b = data[i];
                                // This is unused(?)
                                //var id = (b >> 4) & 0x0F;
                                var len = (b & 0x0F) + 1;
                                i += len;
                            }
                            while (data[i] == 0)
                                i++;
                            doff = i;
                        }
                        // TODO: consider implementing RFC 5285, 4.3. Two-Byte Header
                    }

                    data = this.Opus.Decode(data, doff, data.Length - doff);
                }
                catch { continue; }

                // TODO: wait for ssrc map?
                DiscordUser user = null;
                if (this.SSRCMap.ContainsKey(ssrc))
                {
                    var id = this.SSRCMap[ssrc];
                    if (this.Guild != null)
                        user = this.Guild._members.FirstOrDefault(xm => xm.Id == id) ?? await this.Guild.GetMemberAsync(id).ConfigureAwait(false);

                    if (user == null)
                        user = this.Discord.InternalGetCachedUser(id);

                    if (user == null)
                        user = new DiscordUser { Discord = this.Discord, Id = id };
                }

                await this._voiceReceived.InvokeAsync(new VoiceReceiveEventArgs(this.Discord)
                {
                    SSRC = ssrc,
                    Voice = new ReadOnlyCollection<byte>(data),
                    VoiceLength = 20,
                    User = user
                }).ConfigureAwait(false);
            }
        }
#endif

        /// <summary>
        /// Sends a speaking status to the connected voice channel.
        /// </summary>
        /// <param name="speaking">Whether the current user is speaking or not.</param>
        /// <returns>A task representing the sending operation.</returns>
        public async Task SendSpeakingAsync(bool speaking = true)
        {
            if (!this.IsInitialized)
                throw new InvalidOperationException("The connection is not initialized");

            if (!speaking)
            {
                var nullpcm = new byte[3840];
                for (var i = 0; i < 5; i++)
                    await this.SendAsync(nullpcm, 20).ConfigureAwait(false);

                this.SynchronizerTicks = 0;
                if (this.PlayingWait != null)
                    this.PlayingWait.SetResult(true);
            }
            else
            {
                if (this.PlayingWait == null || this.PlayingWait.Task.IsCompleted)
                    this.PlayingWait = new TaskCompletionSource<bool>();
            }

            var pld = new VoiceDispatch
            {
                OpCode = 5,
                Payload = new VoiceSpeakingPayload
                {
                    Speaking = speaking,
                    Delay = 0
                }
            };

            var plj = JsonConvert.SerializeObject(pld, Formatting.None);
            this.VoiceWs.SendMessage(plj);
        }

        /// <summary>
        /// Asynchronously waits for playback to be finished. Playback is finished when speaking = false is signalled.
        /// </summary>
        /// <returns>A task representing the waiting operation.</returns>
        public async Task WaitForPlaybackFinishAsync()
        {
            if (this.PlayingWait != null)
                await this.PlayingWait.Task.ConfigureAwait(false);
        }

        /// <summary>
        /// Disconnects and disposes this voice connection.
        /// </summary>
        public void Disconnect() 
            => this.Dispose();

        /// <summary>
        /// Disconnects and disposes this voice connection.
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed)
                return;

            this.IsDisposed = true;
            this.IsInitialized = false;
            this.TokenSource.Cancel();
#if !NETSTANDARD1_1
            if (this.Configuration.EnableIncoming)
                this.ReceiverTokenSource.Cancel();
#endif

            try
            {
                this.VoiceWs.DisconnectAsync(null).ConfigureAwait(false).GetAwaiter().GetResult();
                this.UdpClient.Close();
            }
            catch (Exception)
            { }

            this.Opus?.Dispose();
            this.Opus = null;
            this.Sodium = null;
            this.Rtp = null;

            if (this.VoiceDisconnected != null)
                this.VoiceDisconnected(this.Guild);
        }

        private async Task Heartbeat()
        {
            await Task.Yield();

            var token = this.Token;
            while (true)
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    var dt = DateTime.Now;
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "Sent heartbeat", dt);

                    var hbd = new VoiceDispatch
                    {
                        OpCode = 3,
                        Payload = UnixTimestamp(dt)
                    };
                    var hbj = JsonConvert.SerializeObject(hbd);
                    this.VoiceWs.SendMessage(hbj);

                    this.LastHeartbeat = dt;
                    await Task.Delay(this.HeartbeatInterval).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        private async Task Stage1()
        {
#if !NETSTANDARD1_1
            // IP Discovery
            this.UdpClient.Setup(this.ConnectionEndpoint);
            var pck = new byte[70];
            Array.Copy(BitConverter.GetBytes(this.SSRC), 0, pck, pck.Length - 4, 4);
            await this.UdpClient.SendAsync(pck, pck.Length).ConfigureAwait(false);
            var ipd = await this.UdpClient.ReceiveAsync().ConfigureAwait(false);
            var ipe = Array.IndexOf<byte>(ipd, 0, 4);
            var ip = new UTF8Encoding(false).GetString(ipd, 4, ipe - 4);
            var port = BitConverter.ToUInt16(ipd, ipd.Length - 2);
            this.DiscoveredEndpoint = new IpEndpoint { Address = System.Net.IPAddress.Parse(ip), Port = port };
#endif

            // Ready
            var vsp = new VoiceDispatch
            {
                OpCode = 1,
                Payload = new VoiceSelectProtocolPayload
                {
                    Protocol = "udp",
                    Data = new VoiceSelectProtocolPayloadData
                    {
#if !NETSTANDARD1_1
                        Address = this.DiscoveredEndpoint.Address.ToString(),
                        Port = (ushort)this.DiscoveredEndpoint.Port,
#else
                        Address = "0.0.0.0",
                        Port = 0,
#endif
                        Mode = VOICE_MODE
                    }
                }
            };
            var vsj = JsonConvert.SerializeObject(vsp, Formatting.None);
            this.VoiceWs.SendMessage(vsj);

#if !NETSTANDARD1_1
            if (this.Configuration.EnableIncoming)
            {
                this.ReceiverTokenSource = new CancellationTokenSource();
                this.ReceiverTask = Task.Run(this.VoiceReceiverTask, this.ReceiverToken);
            }
#endif
        }

        private Task Stage2()
        {
            this.IsInitialized = true;
            this.ReadyWait.SetResult(true);
            return Task.Delay(0);
        }

        private async Task HandleDispatch(JObject jo)
        {
            var opc = (int)jo["op"];
            var opp = jo["d"] as JObject;

            switch (opc)
            {
                case 2:
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP2 received", DateTime.Now);
                    var vrp = opp.ToObject<VoiceReadyPayload>();
                    this.SSRC = vrp.SSRC;
                    this.ConnectionEndpoint = new ConnectionEndpoint { Hostname = this.ConnectionEndpoint.Hostname, Port = vrp.Port };
                    this.HeartbeatInterval = vrp.HeartbeatInterval;
                    this.HeartbeatTask = Task.Run(this.Heartbeat);
                    await this.Stage1().ConfigureAwait(false);
                    break;

                case 4:
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP4 received", DateTime.Now);
                    var vsd = opp.ToObject<VoiceSessionDescriptionPayload>();
                    this.Key = vsd.SecretKey;
                    await this.Stage2().ConfigureAwait(false);
                    break;

                case 5:
                    // Don't spam OP5
                    //this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP5 received", DateTime.Now);
                    var spd = opp.ToObject<VoiceSpeakingPayload>();
                    var spk = new UserSpeakingEventArgs(this.Discord)
                    {
                        Speaking = spd.Speaking,
                        SSRC = spd.SSRC.Value,
                        User = this.Discord.InternalGetCachedUser(spd.UserId.Value)
                    };
                    if (!this.SSRCMap.ContainsKey(spk.SSRC))
                        this.SSRCMap.AddOrUpdate(spk.SSRC, spk.User.Id, (k, v) => spk.User.Id);
                    await this._userSpeaking.InvokeAsync(spk).ConfigureAwait(false);
                    break;
                    
                case 6:
                    var dt = DateTime.Now;
                    var ping = (int)(dt - this.LastHeartbeat).TotalMilliseconds;
                    Volatile.Write(ref this._ping, ping);
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"Received voice heartbeat ACK, ping {ping.ToString("#,##0", CultureInfo.InvariantCulture)}ms", dt);
                    this.LastHeartbeat = dt;
                    break;

                case 8:
                    // this sends a heartbeat interval that appears to be consistent with regular GW hello
                    // however opcodes don't match (8 != 10)
                    // so we suppress it so that users are not alerted
                    // HELLO
                    break;

                case 9:
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP9 received", DateTime.Now);
                    this.HeartbeatTask = Task.Run(this.Heartbeat);
                    break;

                case 13:
                    var ulpd = opp.ToObject<VoiceUserLeavePayload>();
                    var usr = await this.Discord.GetUserAsync(ulpd.UserId).ConfigureAwait(false);
                    var ssrc = this.SSRCMap.FirstOrDefault(x => x.Value == ulpd.UserId);
                    if (ssrc.Value != 0)
                        this.SSRCMap.TryRemove(ssrc.Key, out _);
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"User '{usr.Username}#{usr.Discriminator}' ({ulpd.UserId.ToString(CultureInfo.InvariantCulture)}) left voice chat in '{this.Channel.Guild.Name}' ({this.Channel.Guild.Id.ToString(CultureInfo.InvariantCulture)})", DateTime.Now);
                    await this._userLeft.InvokeAsync(new VoiceUserLeaveEventArgs(this.Discord) { User = usr }).ConfigureAwait(false);
                    break;

                default:
                    this.Discord.DebugLogger.LogMessage(LogLevel.Warning, "VoiceNext", $"Unknown opcode received: {opc.ToString(CultureInfo.InvariantCulture)}", DateTime.Now);
                    break;
            }
        }

        private async Task VoiceWS_SocketClosed(SocketCloseEventArgs e)
        {
            this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"Voice socket closed ({e.CloseCode.ToString(CultureInfo.InvariantCulture)}, '{e.CloseMessage}')", DateTime.Now);

            // generally this should not be disposed on all disconnects, only on requested ones
            // or something
            // otherwise problems happen
            //this.Dispose();

            if (e.CloseCode == 4006 || e.CloseCode == 4009)
                this.Resume = false;

            if (!this.IsDisposed)
            {
                this.TokenSource.Cancel();
                this.TokenSource = new CancellationTokenSource();
                this.VoiceWs = this.Discord.Configuration.WebSocketClientFactory(this.Discord.Configuration.Proxy);
                this.VoiceWs.OnDisconnect += this.VoiceWS_SocketClosed;
                this.VoiceWs.OnMessage += this.VoiceWS_SocketMessage;
                this.VoiceWs.OnConnect += this.VoiceWS_SocketOpened;
                await this.ConnectAsync().ConfigureAwait(false);
            }
        }

        private Task VoiceWS_SocketMessage(SocketMessageEventArgs e) 
            => this.HandleDispatch(JObject.Parse(e.Message));

        private Task VoiceWS_SocketOpened() 
            => this.StartAsync();

        private Task VoiceWs_SocketErrored(SocketErrorEventArgs e) 
            => this._voiceSocketError.InvokeAsync(new SocketErrorEventArgs(this.Discord) { Exception = e.Exception });

        private static uint UnixTimestamp(DateTime dt)
        {
            var ts = dt - UnixEpoch;
            var sd = ts.TotalSeconds;
            var si = (uint)sd;
            return si;
        }
    }
}

// Naam you still owe me those noodles :^)
// I remember