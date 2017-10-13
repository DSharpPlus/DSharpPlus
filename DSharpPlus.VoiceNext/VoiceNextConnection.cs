// ReSharper disable RedundantUsingDirective
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
using DSharpPlus.Enums;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Udp;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VoiceNext.Codec;
using DSharpPlus.VoiceNext.VoiceEntities;
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
            add => _userSpeaking.Register(value);
            remove => _userSpeaking.Unregister(value);
        }
        private readonly AsyncEvent<UserSpeakingEventArgs> _userSpeaking;

#if !NETSTANDARD1_1
        /// <summary>
        /// Triggered whenever voice data is received from the connected voice channel.
        /// </summary>
        public event AsyncEventHandler<VoiceReceiveEventArgs> VoiceReceived
        {
            add => _voiceReceived.Register(value);
            remove => _voiceReceived.Unregister(value);
        }
        private readonly AsyncEvent<VoiceReceiveEventArgs> _voiceReceived;
#endif

        /// <summary>
        /// Triggered whenever voice WebSocket throws an exception.
        /// </summary>
        public event AsyncEventHandler<SocketErrorEventArgs> VoiceSocketErrored
        {
            add => _voiceSocketError.Register(value);
            remove => _voiceSocketError.Unregister(value);
        }
        private readonly AsyncEvent<SocketErrorEventArgs> _voiceSocketError;

        internal event VoiceDisconnectedEventHandler VoiceDisconnected;

        private const string VoiceMode = "xsalsa20_poly1305";
        private static DateTime UnixEpoch => _unixEpoch.Value;
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<DateTime> _unixEpoch;

        private DiscordClient Discord { get; }
        private DiscordGuild Guild { get; }
        private ConcurrentDictionary<uint, ulong> SsrcMap { get; }

        private BaseUdpClient UdpClient { get; }
        private BaseWebSocketClient VoiceWs { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private Task HeartbeatTask { get; set; }
        private int HeartbeatInterval { get; set; }
        private DateTime LastHeartbeat { get; set; }

        private CancellationTokenSource TokenSource { get; }
        private CancellationToken Token => TokenSource.Token;

        private VoiceServerUpdatePayload ServerData { get; }
        private VoiceStateUpdatePayload StateData { get; }
        private bool Resume { get; set; }

        private VoiceNextConfiguration Configuration { get; }
        private OpusCodec Opus { get; set; }
        private SodiumCodec Sodium { get; set; }
        private RtpCodec Rtp { get; set; }
        private double SynchronizerTicks { get; set; }
        private double SynchronizerResolution { get; set; }
        private double TickResolution { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private TimeSpan UdpLatency { get; }

        private ushort Sequence { get; set; }
        private uint Timestamp { get; set; }
        private uint Ssrc { get; set; }
        private byte[] Key { get; set; }
#if !NETSTANDARD1_1
        private IpEndpoint DiscoveredEndpoint { get; set; }
#endif
        private ConnectionEndpoint ConnectionEndpoint { get; set; }

        private TaskCompletionSource<bool> ReadyWait { get; }
        private bool IsInitialized { get; set; }
        private bool IsDisposed { get; set; }

        private TaskCompletionSource<bool> PlayingWait { get; set; }
        private SemaphoreSlim PlaybackSemaphore { get; }

#if !NETSTANDARD1_1
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private Task ReceiverTask { get; set; }
#endif

        /// <summary>
        /// Gets whether this connection is still playing audio.
        /// </summary>
        public bool IsPlaying => PlaybackSemaphore.CurrentCount == 0 || PlayingWait != null && !PlayingWait.Task.IsCompleted;

        /// <summary>
        /// Gets the websocket round-trip time in ms.
        /// </summary>
        public int Ping => Volatile.Read(ref _ping);
        private int _ping;

        /// <summary>
        /// Gets the channel this voice client is connected to.
        /// </summary>
        public DiscordChannel Channel { get; }

        internal VoiceNextConnection(DiscordClient client, DiscordGuild guild, DiscordChannel channel, VoiceNextConfiguration config, VoiceServerUpdatePayload server, VoiceStateUpdatePayload state)
        {
            Discord = client;
            Guild = guild;
            Channel = channel;
            SsrcMap = new ConcurrentDictionary<uint, ulong>();

            _userSpeaking = new AsyncEvent<UserSpeakingEventArgs>(Discord.EventErrorHandler, "USER_SPEAKING");
#if !NETSTANDARD1_1
            _voiceReceived = new AsyncEvent<VoiceReceiveEventArgs>(Discord.EventErrorHandler, "VOICE_RECEIVED");
#endif
            _voiceSocketError = new AsyncEvent<SocketErrorEventArgs>(Discord.EventErrorHandler, "VOICE_WS_ERROR");
            TokenSource = new CancellationTokenSource();

            Configuration = config;
            Opus = new OpusCodec(48000, 2, Configuration.VoiceApplication);
            Sodium = new SodiumCodec();
            Rtp = new RtpCodec();
            UdpLatency = TimeSpan.FromMilliseconds(0.1);

            ServerData = server;
            StateData = state;

            var eps = ServerData.Endpoint;
            var epi = eps.LastIndexOf(':');
            string eph;
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
            ConnectionEndpoint = new ConnectionEndpoint { Hostname = eph, Port = epp };

            ReadyWait = new TaskCompletionSource<bool>();
            IsInitialized = false;
            IsDisposed = false;

            PlayingWait = null;
            PlaybackSemaphore = new SemaphoreSlim(1, 1);

            UdpClient = BaseUdpClient.Create();
            VoiceWs = BaseWebSocketClient.Create();
            VoiceWs.OnDisconnect += VoiceWS_SocketClosed;
            VoiceWs.OnMessage += VoiceWS_SocketMessage;
            VoiceWs.OnConnect += VoiceWS_SocketOpened;
            VoiceWs.OnError += VoiceWs_SocketErrored;
        }

        static VoiceNextConnection()
        {
            _unixEpoch = new Lazy<DateTime>(() => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        }

        ~VoiceNextConnection()
        {
            Dispose();
        }

        /// <summary>
        /// Connects to the specified voice channel.
        /// </summary>
        /// <returns>A task representing the connection operation.</returns>
        internal async Task ConnectAsync()
        {
            await Task.Run(() => VoiceWs.ConnectAsync($"wss://{ConnectionEndpoint.Hostname}/?encoding=json&v=3")).ConfigureAwait(false);
        }

        internal Task StartAsync()
        {
            // Let's announce our intentions to the server
            var vdp = new VoiceDispatch();

            if (!Resume)
            {
                vdp.OpCode = 0;
                if (StateData.UserId != null)
                {
                    vdp.Payload = new VoiceIdentifyPayload
                    {
                        ServerId = ServerData.GuildId,
                        UserId = StateData.UserId.Value,
                        SessionId = StateData.SessionId,
                        Token = ServerData.Token
                    };
                }
                Resume = true;
            }
            else
            {
                vdp.OpCode = 7;
                vdp.Payload = new VoiceIdentifyPayload
                {
                    ServerId = ServerData.GuildId,
                    SessionId = StateData.SessionId,
                    Token = ServerData.Token
                };
            }
            var vdj = JsonConvert.SerializeObject(vdp, Formatting.None);
            VoiceWs.SendMessage(vdj);

            return Task.Delay(0);
        }

        internal async Task WaitForReady()
        {
            await ReadyWait.Task.ConfigureAwait(false);
        }

        /// <summary>
        /// Encodes, encrypts, and sends the provided PCM data to the connected voice channel.
        /// </summary>
        /// <param name="pcm">PCM data to encode, encrypt, and send.</param>
        /// <param name="blocksize">Millisecond length of the PCM data.</param>
        /// <param name="bitrate">Bitrate of the PCM data.</param>
        /// <returns>Task representing the sending operation.</returns>
        public async Task SendAsync(byte[] pcm, int blocksize, int bitrate = 16)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("The connection is not initialized");
            }

            await PlaybackSemaphore.WaitAsync();

            var rtp = Rtp.Encode(Sequence, Timestamp, Ssrc);

            var dat = Opus.Encode(pcm, 0, pcm.Length, bitrate);
            dat = Sodium.Encode(dat, Rtp.MakeNonce(rtp), Key);
            dat = Rtp.Encode(rtp, dat);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (SynchronizerTicks == 0)
            {
                SynchronizerTicks = Stopwatch.GetTimestamp();
                SynchronizerResolution = Stopwatch.Frequency * 0.02;
                TickResolution = 10_000_000.0 / Stopwatch.Frequency;
                // ReSharper disable once ImpureMethodCallOnReadonlyValueField
                Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"Timer accuracy: {Stopwatch.Frequency.ToString("#,##0", CultureInfo.InvariantCulture)}/{SynchronizerResolution.ToString(CultureInfo.InvariantCulture)} (high resolution? {Stopwatch.IsHighResolution})", DateTime.Now);
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
                
                var cts = Math.Max(Stopwatch.GetTimestamp() - SynchronizerTicks, 0);
                if (cts < SynchronizerResolution)
                {
                    await Task.Delay(TimeSpan.FromTicks((long)((SynchronizerResolution - cts) * TickResolution)));
                }

                SynchronizerTicks += SynchronizerResolution;
            }

            await SendSpeakingAsync();
            await UdpClient.SendAsync(dat, dat.Length);

            Sequence++;
            Timestamp += 48 * (uint)blocksize;

            PlaybackSemaphore.Release();
        }

#if !NETSTANDARD1_1
        private async Task VoiceReceiverTask()
        {
            var token = Token;
            var client = UdpClient;
            while (!token.IsCancellationRequested)
            {
                if (client.DataAvailable <= 0)
                {
                    continue;
                }

                byte[] data;
                uint ssrc;
                try
                {
                    data = await client.ReceiveAsync();

                    var header = new byte[RtpCodec.SizeHeader];
                    data = Rtp.Decode(data, header);

                    var nonce = Rtp.MakeNonce(header);
                    data = Sodium.Decode(data, nonce, Key);

                    // following is thanks to code from Eris
                    // https://github.com/abalabahaha/eris/blob/master/lib/voice/VoiceConnection.js#L623
                    var doff = 0;
                    Rtp.Decode(header, out _, out _, out ssrc, out var hasExt);
                    if (hasExt)
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
                            {
                                i++;
                            }

                            doff = i;
                        }
                        // TODO: consider implementing RFC 5285, 4.3. Two-Byte Header
                    }

                    data = Opus.Decode(data, doff, data.Length - doff);
                }
                catch { continue; }

                // TODO: wait for ssrc map?
                DiscordUser user = null;
                if (SsrcMap.ContainsKey(ssrc))
                {
                    var id = SsrcMap[ssrc];
                    if (Guild != null)
                    {
                        user = Guild._members.FirstOrDefault(xm => xm.Id == id) ?? await Guild.GetMemberAsync(id);
                    }

                    if (user == null)
                    {
                        user = Discord.InternalGetCachedUser(id);
                    }

                    if (user == null)
                    {
                        user = new DiscordUser { Discord = Discord, Id = id };
                    }
                }

                await _voiceReceived.InvokeAsync(new VoiceReceiveEventArgs(Discord)
                {
                    Ssrc = ssrc,
                    Voice = new ReadOnlyCollection<byte>(data),
                    VoiceLength = 20,
                    User = user
                });
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
            if (!IsInitialized)
            {
                throw new InvalidOperationException("The connection is not initialized");
            }

            if (!speaking)
            {
                var nullpcm = new byte[3840];
                for (var i = 0; i < 5; i++)
                {
                    await SendAsync(nullpcm, 20);
                }

                SynchronizerTicks = 0;
                if (PlayingWait != null)
                {
                    PlayingWait.SetResult(true);
                }
            }
            else
            {
                if (PlayingWait == null || PlayingWait.Task.IsCompleted)
                {
                    PlayingWait = new TaskCompletionSource<bool>();
                }
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
            await Task.Run(() => VoiceWs.SendMessage(plj));
        }

        /// <summary>
        /// Asynchronously waits for playback to be finished. Playback is finished when speaking = false is signalled.
        /// </summary>
        /// <returns>A task representing the waiting operation.</returns>
        public async Task WaitForPlaybackFinishAsync()
        {
            if (PlayingWait != null)
            {
                await PlayingWait.Task;
            }
        }

        /// <summary>
        /// Disconnects and disposes this voice connection.
        /// </summary>
        public void Disconnect() =>
            Dispose();

        /// <summary>
        /// Disconnects and disposes this voice connection.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            TokenSource.Cancel();

            IsDisposed = true;
            IsInitialized = false;
            try
            {
                VoiceWs.InternalDisconnectAsync(null).GetAwaiter().GetResult();
                UdpClient.Close();
            }
            catch
            {
                // ignored
            }

            Opus?.Dispose();
            Opus = null;
            Sodium = null;
            Rtp = null;

            VoiceDisconnected?.Invoke(Guild);
        }

        private async Task Heartbeat()
        {
            await Task.Yield();

            while (true)
            {
                try
                {
                    Token.ThrowIfCancellationRequested();

                    var dt = DateTime.Now;
                    Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "Sent heartbeat", dt);

                    var hbd = new VoiceDispatch
                    {
                        OpCode = 3,
                        Payload = UnixTimestamp(dt)
                    };
                    var hbj = JsonConvert.SerializeObject(hbd);
                    VoiceWs.SendMessage(hbj);

                    LastHeartbeat = dt;
                    await Task.Delay(HeartbeatInterval);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

#pragma warning disable 1998
        private async Task Stage1()
#pragma warning restore 1998
        {
            // Begin heartbeating
            HeartbeatTask = Task.Run(Heartbeat);

#if !NETSTANDARD1_1
            // IP Discovery
            UdpClient.Setup(ConnectionEndpoint);
            var pck = new byte[70];
            Array.Copy(BitConverter.GetBytes(Ssrc), 0, pck, pck.Length - 4, 4);
            await UdpClient.SendAsync(pck, pck.Length);
            var ipd = await UdpClient.ReceiveAsync();
            var ipe = Array.IndexOf<byte>(ipd, 0, 4);
            var ip = new UTF8Encoding(false).GetString(ipd, 4, ipe - 4);
            var port = BitConverter.ToUInt16(ipd, ipd.Length - 2);
            DiscoveredEndpoint = new IpEndpoint { Address = System.Net.IPAddress.Parse(ip), Port = port };
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
                        Address = DiscoveredEndpoint.Address.ToString(),
                        Port = (ushort)DiscoveredEndpoint.Port,
#else
                        Address = "0.0.0.0",
                        Port = 0,
#endif
                        Mode = VoiceMode
                    }
                }
            };
            var vsj = JsonConvert.SerializeObject(vsp, Formatting.None);
            VoiceWs.SendMessage(vsj);

#if !NETSTANDARD1_1
            if (Configuration.EnableIncoming)
            {
                ReceiverTask = Task.Run(VoiceReceiverTask, Token);
            }
#endif
        }

        private Task Stage2()
        {
            IsInitialized = true;
            ReadyWait.SetResult(true);
            return Task.Delay(0);
        }

        private async Task HandleDispatch(JObject jo)
        {
            var opc = (int)jo["op"];
            var opp = jo["d"] as JObject;

            switch (opc)
            {
                case 2:
                    Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP2 received", DateTime.Now);
                    if (opp != null)
                    {
                        var vrp = opp.ToObject<VoiceReadyPayload>();
                        Ssrc = vrp.Ssrc;
                        ConnectionEndpoint = new ConnectionEndpoint { Hostname = ConnectionEndpoint.Hostname, Port = vrp.Port };
                        HeartbeatInterval = vrp.HeartbeatInterval;
                        await Stage1();
                    }
                    break;

                case 4:
                    Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP4 received", DateTime.Now);
                    if (opp != null)
                    {
                        var vsd = opp.ToObject<VoiceSessionDescriptionPayload>();
                        Key = vsd.SecretKey;
                        await Stage2();
                    }
                    break;

                case 5:
                    // Don't spam OP5
                    //this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP5 received", DateTime.Now);
                    var spd = opp?.ToObject<VoiceSpeakingPayload>();

                    if (spd?.Ssrc != null)
                    {
                        if (spd.UserId != null)
                        {
                            var spk = new UserSpeakingEventArgs(Discord)
                            {
                                Speaking = spd.Speaking,
                                Ssrc = spd.Ssrc.Value,
                                User = Discord.InternalGetCachedUser(spd.UserId.Value)
                            };

                            if (!SsrcMap.ContainsKey(spk.Ssrc))
                            {
                                SsrcMap.AddOrUpdate(spk.Ssrc, spk.User.Id, (k, v) => spk.User.Id);
                            }
                            await _userSpeaking.InvokeAsync(spk);
                        }
                    }

                    break;

                case 3:
                case 6:
                    Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP3 or OP6 received", DateTime.Now);
                    var dt = DateTime.Now;
                    var ping = (int)(dt - LastHeartbeat).TotalMilliseconds;
                    Volatile.Write(ref _ping, ping);
                    Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"Received voice heartbeat ACK, ping {ping.ToString("#,###", CultureInfo.InvariantCulture)}ms", dt);
                    LastHeartbeat = dt;
                    break;

                case 8:
                    // this sends a heartbeat interval that appears to be consistent with regular GW hello
                    // however opcodes don't match (8 != 10)
                    // so we suppress it so that users are not alerted
                    // HELLO
                    break;

                case 9:
                    Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP9 received, starting new session", DateTime.Now);
                    Resume = false;
                    await StartAsync();
                    break;

                default:
                    Discord.DebugLogger.LogMessage(LogLevel.Warning, "VoiceNext", $"Unknown opcode received: {opc.ToString(CultureInfo.InvariantCulture)}", DateTime.Now);
                    break;
            }
        }

        private async Task VoiceWS_SocketClosed(SocketCloseEventArgs e)
        {
            Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"Voice socket closed ({e.CloseCode.ToString(CultureInfo.InvariantCulture)}, '{e.CloseMessage}')", DateTime.Now);
            Dispose();

            if (!IsDisposed)
            {
                VoiceWs = BaseWebSocketClient.Create();
                VoiceWs.OnDisconnect += VoiceWS_SocketClosed;
                VoiceWs.OnMessage += VoiceWS_SocketMessage;
                VoiceWs.OnConnect += VoiceWS_SocketOpened;
                await StartAsync();
            }
        }

        private async Task VoiceWS_SocketMessage(SocketMessageEventArgs e)
        {
            await HandleDispatch(JObject.Parse(e.Message));
        }

        private async Task VoiceWS_SocketOpened()
        {
            await StartAsync();
        }

        private Task VoiceWs_SocketErrored(SocketErrorEventArgs e) =>
            _voiceSocketError.InvokeAsync(new SocketErrorEventArgs(Discord) { Exception = e.Exception });

        private static uint UnixTimestamp(DateTime dt)
        {
            var ts = dt - UnixEpoch;
            var sd = ts.TotalSeconds;
            var si = (uint)sd;
            return si;
        }
    }
}
