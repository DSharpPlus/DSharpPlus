using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext.Codec;
using DSharpPlus.VoiceNext.VoiceEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

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
            add { this._user_speaking.Register(value); }
            remove { this._user_speaking.Unregister(value); }
        }
        private AsyncEvent<UserSpeakingEventArgs> _user_speaking = new AsyncEvent<UserSpeakingEventArgs>();

        /// <summary>
        /// Triggered whenever voice data is received from the connected voice channel.
        /// </summary>
        public event AsyncEventHandler<VoiceReceivedEventArgs> VoiceReceived
        {
            add { this._voice_received.Register(value); }
            remove { this._voice_received.Unregister(value); }
        }
        private AsyncEvent<VoiceReceivedEventArgs> _voice_received = new AsyncEvent<VoiceReceivedEventArgs>();

        internal event VoiceDisconnectedEventHandler VoiceDisconnected;

        private const string VOICE_MODE = "xsalsa20_poly1305";
        private static DateTime UnixEpoch { get { return _unix_epoch.Value; } }
        private static Lazy<DateTime> _unix_epoch;

        private DiscordClient Discord { get; set; }
        private DiscordGuild Guild { get; set; }
        private DiscordChannel Channel { get; set; }

        private UdpClient UdpClient { get; set; }
        private WebSocketWrapper VoiceWs { get; set; }
        private Thread HeartbeatThread { get; set; }
        private int HeartbeatInterval { get; set; }
        private DateTime LastHeartbeat { get; set; }

        private VoiceServerUpdatePayload ServerData { get; set; }
        private VoiceStateUpdatePayload StateData { get; set; }

        private VoiceNextConfiguration Configuration { get; set; }
        private OpusCodec Opus { get; set; }
        private SodiumCodec Sodium { get; set; }
        private RtpCodec RTP { get; set; }
        private Stopwatch Synchronizer { get; set; }
        private TimeSpan UdpLatency { get; set; }

        private ushort Sequence { get; set; }
        private uint Timestamp { get; set; }
        private uint SSRC { get; set; }
        private byte[] Key { get; set; }
        private IPEndPoint DiscoveredEndpoint { get; set; }
        private DnsEndPoint ConnectionEndpoint { get; set; }
        
        private TaskCompletionSource<bool> ReadyWait { get; set; }
        private bool IsInitialized { get; set; }
        private bool IsDisposed { get; set; }

        internal VoiceNextConnection(DiscordClient client, DiscordGuild guild, DiscordChannel channel, VoiceNextConfiguration config, VoiceServerUpdatePayload server, VoiceStateUpdatePayload state)
        {
            this.Discord = client;
            this.Guild = guild;
            this.Channel = channel;

            this.Configuration = config;
            this.Opus = new OpusCodec(48000, 2, this.Configuration.VoiceApplication);
            this.Sodium = new SodiumCodec();
            this.RTP = new RtpCodec();
            this.Synchronizer = new Stopwatch();
            this.UdpLatency = TimeSpan.FromMilliseconds(0.5);

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
            this.ConnectionEndpoint = new DnsEndPoint(eph, epp);

            this.ReadyWait = new TaskCompletionSource<bool>();
            this.IsInitialized = false;
            this.IsDisposed = false;

            this.UdpClient = new UdpClient();
            this.VoiceWs = WebSocketWrapper.Create($"wss://{this.ConnectionEndpoint.Host}");
            this.VoiceWs.OnDisconnect += this.VoiceWS_SocketClosed;
            this.VoiceWs.OnMessage += this.VoiceWS_SocketMessage;
            this.VoiceWs.OnConnect += this.VoiceWS_SocketOpened;
        }

        static VoiceNextConnection()
        {
            _unix_epoch = new Lazy<DateTime>(() => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        }

        ~VoiceNextConnection()
        {
            this.Dispose();
        }

        /// <summary>
        /// Connects to the specified voice channel.
        /// </summary>
        /// <returns>A task representing the connection operation.</returns>
        internal async Task ConnectAsync()
        {
            await Task.Run(() => this.VoiceWs.InternalConnectAsync()).ConfigureAwait(false);
        }

        internal Task StartAsync()
        {
            // Let's announce our intentions to the server
            var vdp = new VoiceDispatch
            {
                OpCode = 0,
                Payload = new VoiceIdentifyPayload
                {
                    ServerId = this.ServerData.GuildId,
                    UserId = this.StateData.UserId.Value,
                    SessionId = this.StateData.SessionId,
                    Token = this.ServerData.Token
                }
            };
            var vdj = JsonConvert.SerializeObject(vdp, Formatting.None);
            this.VoiceWs.SendMessage(vdj);

            return Task.Delay(0);
        }

        internal async Task WaitForReady()
        {
            await this.ReadyWait.Task.ConfigureAwait(false);
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
            if (!this.IsInitialized)
                throw new InvalidOperationException("The connection is not yet initialized");

            var rtp = this.RTP.Encode(this.Sequence, this.Timestamp, this.SSRC);

            var dat = this.Opus.Encode(pcm, 0, pcm.Length, bitrate);
            dat = this.Sodium.Encode(dat, this.RTP.MakeNonce(rtp), this.Key);
            dat = this.RTP.Encode(rtp, dat);
            
            await this.UdpClient.SendAsync(dat, dat.Length);

            this.Sequence++;
            this.Timestamp += 48 * (uint)blocksize;

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
            
            this.Synchronizer.Stop();
            var ts = TimeSpan.FromMilliseconds(blocksize) - this.Synchronizer.Elapsed - this.UdpLatency;
            if (ts.Ticks < 0)
                ts = TimeSpan.FromTicks(1);
            Thread.Sleep(ts);
            this.Synchronizer.Restart();
        }

        /// <summary>
        /// Sends a speaking status to the connected voice channel.
        /// </summary>
        /// <param name="speaking">Whether the current user is speaking or not.</param>
        /// <returns>A task representing the sending operation.</returns>
        public async Task SendSpeakingAsync(bool speaking = true)
        {
            if (!this.IsInitialized)
                throw new InvalidOperationException("The connection is not yet initialized");

            if (!speaking)
                this.Synchronizer.Reset();

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
            await Task.Run(() => this.VoiceWs.SendMessage(plj));
        }

        /// <summary>
        /// Disconnects and disposes this voice connection.
        /// </summary>
        public void Disconnect() =>
            this.Dispose();

        /// <summary>
        /// Disconnects and disposes this voice connection.
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed)
                return;

            this.HeartbeatThread.Abort();

            this.IsDisposed = true;
            this.IsInitialized = false;
            try
            {
                this.VoiceWs.InternalDisconnectAsync();
                this.UdpClient.Close();
            }
            catch (Exception)
            { }

            this.Opus?.Dispose();
            this.Opus = null;
            this.Sodium = null;
            this.RTP = null;

            if (this.VoiceDisconnected != null)
                this.VoiceDisconnected(this.Guild);
        }

        private void Heartbeat()
        {
            while (true)
            {
                try
                {
                    var dt = DateTime.Now;
                    this.Discord.DebugLogger.LogMessage(LogLevel.Unnecessary, "VoiceNext", "Sent heartbeat", dt);

                    var hbd = new VoiceDispatch
                    {
                        OpCode = 3,
                        Payload = UnixTimestamp(dt)
                    };
                    var hbj = JsonConvert.SerializeObject(hbd);
                    this.VoiceWs.SendMessage(hbj);

                    this.LastHeartbeat = dt;
                    Thread.Sleep(this.HeartbeatInterval);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
            }
        }

        private async Task Stage1()
        {
            // Begin heartbeating
            this.HeartbeatThread = new Thread(this.Heartbeat);
            this.HeartbeatThread.SetApartmentState(ApartmentState.STA);
            this.HeartbeatThread.Name = $"Heartbeat for Voice ({this.SSRC})";
            this.HeartbeatThread.Start();

            // IP Discovery
            this.UdpClient.Connect(this.ConnectionEndpoint.Host, this.ConnectionEndpoint.Port);
            this.UdpClient.AllowNatTraversal(true);
            var pck = new byte[70];
            Array.Copy(BitConverter.GetBytes(this.SSRC), 0, pck, pck.Length - 4, 4);
            await this.UdpClient.SendAsync(pck, pck.Length);
            var ipd = await this.UdpClient.ReceiveAsync();
            var ipe = Array.IndexOf<byte>(ipd.Buffer, 0, 4);
            var ip = new UTF8Encoding(false).GetString(ipd.Buffer, 4, ipe - 4);
            var port = BitConverter.ToUInt16(ipd.Buffer, ipd.Buffer.Length - 2);
            this.DiscoveredEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);

            // Ready
            var vsp = new VoiceDispatch
            {
                OpCode = 1,
                Payload = new VoiceSelectProtocolPayload
                {
                    Protocol = "udp",
                    Data = new VoiceSelectProtocolPayloadData
                    {
                        Address = this.DiscoveredEndpoint.Address.ToString(),
                        Port = (ushort)this.DiscoveredEndpoint.Port,
                        Mode = VOICE_MODE
                    }
                }
            };
            var vsj = JsonConvert.SerializeObject(vsp, Formatting.None);
            this.VoiceWs.SendMessage(vsj);
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
                    this.ConnectionEndpoint = new DnsEndPoint(this.ConnectionEndpoint.Host, vrp.Port);
                    this.HeartbeatInterval = vrp.HeartbeatInterval;
                    await this.Stage1();
                    break;

                case 3:
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP3 received", DateTime.Now);
                    var dt = DateTime.Now;
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"Received voice heartbeat ACK, ping {(dt - this.LastHeartbeat).TotalMilliseconds.ToString("#,###")}ms", dt);
                    this.LastHeartbeat = dt;
                    break;

                case 4:
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP4 received", DateTime.Now);
                    var vsd = opp.ToObject<VoiceSessionDescriptionPayload>();
                    this.Key = vsd.SecretKey;
                    await this.Stage2();
                    break;

                case 5:
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP5 received", DateTime.Now);
                    var spd = opp.ToObject<VoiceSpeakingPayload>();
                    var spk = new UserSpeakingEventArgs
                    {
                        Speaking = spd.Speaking,
                        SSRC = spd.SSRC.Value,
                        UserID = spd.UserId.Value
                    };
                    await this._user_speaking.InvokeAsync(spk);
                    break;

                default:
                    this.Discord.DebugLogger.LogMessage(LogLevel.Warning, "VoiceNext", $"Unknown opcode received: {opc}", DateTime.Now);
                    break;
            }
        }

        private Task VoiceWS_SocketClosed()
        {
            this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"Voice session closed", DateTime.Now);
            this.Dispose();
            return Task.Delay(0);
        }

        private Task VoiceWS_SocketError(ErrorEventArgs e)
        {
            return Task.Delay(0);
        }

        private async Task VoiceWS_SocketMessage(WebSocketMessageArgs e)
        {
            await this.HandleDispatch(JObject.Parse(e.message));
        }

        private async Task VoiceWS_SocketOpened()
        {
            await this.StartAsync();
        }

        private static uint UnixTimestamp(DateTime dt)
        {
            var ts = dt - UnixEpoch;
            var sd = ts.TotalSeconds;
            var si = (uint)sd;
            return si;
        }
    }
}
