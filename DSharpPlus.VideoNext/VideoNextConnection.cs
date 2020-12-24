using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Net.Udp;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VideoNext.Codec;
using DSharpPlus.VideoNext.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.Codec;
using DSharpPlus.VoiceNext.EventArgs;
using Emzi0767.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sodium;

namespace DSharpPlus.VideoNext
{
    internal delegate Task VideoDisconnectedEventHandler(DiscordGuild guild);

    /// <summary>
    /// VoiceNext connection to a voice channel.
    /// </summary>
    public sealed class VideoNextConnection : IDisposable
    {

        /// <summary>
        /// Triggered whenever voice data is received from the connected voice channel.
        /// </summary>
        public event AsyncEventHandler<VideoNextConnection, VideoReceiveEventArgs> VideoReceived
        {
            add { this._videoReceived.Register(value); }
            remove { this._videoReceived.Unregister(value); }
        }

        private AsyncEvent<VideoNextConnection, VideoReceiveEventArgs> _videoReceived;

        /// <summary>
        /// Triggered whenever voice WebSocket throws an exception.
        /// </summary>
        public event AsyncEventHandler<VideoNextConnection, SocketErrorEventArgs> VideoSocketErrored
        {
            add { this._videoSocketError.Register(value); }
            remove { this._videoSocketError.Unregister(value); }
        }
        

        private AsyncEvent<VideoNextConnection, SocketErrorEventArgs> _videoSocketError;

        internal event VoiceDisconnectedEventHandler VideoDisconnected;

        private static DateTimeOffset UnixEpoch { get; } = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private DiscordClient Discord { get; }
        private DiscordGuild Guild { get; }
        private ConcurrentDictionary<uint, VideoSender> TransmittingSSRCs { get; }

        private BaseUdpClient UdpClient { get; }

        private IWebSocketClient VideoWs { get; set; }
        private Task HeartbeatTask { get; set; }
        private int HeartbeatInterval { get; set; }
        private DateTimeOffset LastHeartbeat { get; set; }

        private CancellationTokenSource TokenSource { get; set; }

        private CancellationToken Token
            => this.TokenSource.Token;

        internal VideoStateData StateData { get; set; }
        internal bool Resume { get; set; }

        private VideoNextConfiguration Configuration { get; }
        private ulong Nonce { get; set; } = 0;

        private ushort Sequence { get; set; }
        private uint Timestamp { get; set; }
        private uint SSRC { get; set; }
        private byte[] Key { get; set; }
        private IpEndpoint DiscoveredEndpoint { get; set; }
        internal ConnectionEndpoint WebSocketEndpoint { get; set; }
        internal ConnectionEndpoint UdpEndpoint { get; set; }

        private TaskCompletionSource<bool> ReadyWait { get; set; }
        private bool IsInitialized { get; set; }
        private bool IsDisposed { get; set; }

        private TaskCompletionSource<bool> PlayingWait { get; set; }
        
        private VideoTransmitSink TransmitStream { get; set; }
        private Channel<RawVideoPacket> TransmitChannel { get; }
        private ConcurrentDictionary<ulong, long> KeepaliveTimestamps { get; }
        private ulong _lastKeepalive = 0;

        private Task SenderTask { get; set; }
        private CancellationTokenSource SenderTokenSource { get; set; }

        private CancellationToken SenderToken
            => this.SenderTokenSource.Token;

        private Task ReceiverTask { get; set; }
        private CancellationTokenSource ReceiverTokenSource { get; set; }

        private CancellationToken ReceiverToken
            => this.ReceiverTokenSource.Token;

        private Task KeepaliveTask { get; set; }
        private CancellationTokenSource KeepaliveTokenSource { get; set; }

        private CancellationToken KeepaliveToken
            => this.KeepaliveTokenSource.Token;
        

        /// <summary>
        /// Gets whether this connection is still playing audio.
        /// </summary>
        public bool IsPlaying
            => this.PlayingWait != null && !this.PlayingWait.Task.IsCompleted;

        /// <summary>
        /// Gets the websocket round-trip time in ms.
        /// </summary>
        public int WebSocketPing
            => Volatile.Read(ref this._wsPing);

        private int _wsPing = 0;

        /// <summary>
        /// Gets the UDP round-trip time in ms.
        /// </summary>
        public int UdpPing
            => Volatile.Read(ref this._udpPing);

        private int _udpPing = 0;

        private int _queueCount;

        /// <summary>
        /// Gets the channel this voice client is connected to.
        /// </summary>
        public DiscordChannel TargetChannel { get; internal set; }

        internal VideoNextConnection(DiscordClient client, DiscordChannel chn, VideoNextConfiguration config, VideoStateData state)
        {
            this.Discord = client;
            this.Guild = chn.Guild;
            this.TargetChannel = chn;
            this.TransmittingSSRCs = new ConcurrentDictionary<uint, VideoSender>();
            
            this._videoReceived = new AsyncEvent<VideoNextConnection, VideoReceiveEventArgs>("VNEXT_VIDEO_RECEIVE", TimeSpan.Zero, this.Discord.EventErrorHandler);
            this._videoSocketError = new AsyncEvent<VideoNextConnection, SocketErrorEventArgs>("VNEXT_WS_ERROR", TimeSpan.Zero, this.Discord.EventErrorHandler);

            this.TokenSource = new CancellationTokenSource();
            this.KeepaliveTokenSource = new CancellationTokenSource();
            this.ReceiverTokenSource = new CancellationTokenSource();
            this.SenderTokenSource = new CancellationTokenSource();
            
            this.StateData = state;
            
            this.Configuration = config;
            
            var eps = this.StateData.Endpoint;
            var epi = eps.LastIndexOf(':');
            var eph = string.Empty;
            var epp = 443;
            if (epi != -1)
            {
                eph = eps.Substring(0, epi);
                epp = int.Parse(eps.Substring(epi + 1));
            }
            else
            {
                eph = eps;
            }
            this.WebSocketEndpoint = new ConnectionEndpoint { Hostname = eph, Port = epp };
            
            this.ReadyWait = new TaskCompletionSource<bool>();
            this.IsInitialized = false;
            this.IsDisposed = false;

            this.PlayingWait = null;
            this.TransmitChannel = Channel.CreateBounded<RawVideoPacket>(new BoundedChannelOptions(1000));
            
            this.KeepaliveTimestamps = new ConcurrentDictionary<ulong, long>();

            this.UdpClient = this.Discord.Configuration.UdpClientFactory();
            this.VideoWs = this.Discord.Configuration.WebSocketClientFactory(this.Discord.Configuration.Proxy);
            this.VideoWs.Connected += this.VideoWS_SocketOpen;
            this.VideoWs.Disconnected += this.VideoWS_SocketClosed;
            this.VideoWs.MessageReceived += this.VideoWS_SocketMessage;
            this.VideoWs.ExceptionThrown += this.VideoWS_SocketException;

        }
        
        /// <summary>
        /// Gets a transmit stream for this connection, optionally specifying a packet size to use with the stream. If a stream is already configured, it will return the existing one.
        /// </summary>
        /// <param name="sampleDuration">Duration, in ms, to use for audio packets.</param>
        /// <returns>Transmit stream.</returns>
        public VideoTransmitSink GetTransmitSink()
        {
            if (this.TransmitStream == null)
                this.TransmitStream = new VideoTransmitSink(this);

            return this.TransmitStream;
        }

        ~VideoNextConnection()
        {
            this.Dispose();
        }

        internal Task ConnectAsync()
        {
            var gwuri = new UriBuilder
            {
                Scheme = "wss",
                Host = this.WebSocketEndpoint.Hostname,
                Query = "encoding=json&v=4"
            };

            return this.VideoWs.ConnectAsync(gwuri.Uri);
        }

        internal Task ReconnectAsync()
            => this.VideoWs.DisconnectAsync();

        internal async Task StreamUpdaterTask()
        {
            while (!Token.IsCancellationRequested)
            {
                var v = new JObject();
                v["op"] = 22;
                v["d"] = new JObject();
                v["d"]["stream_key"] = StateData.Key;
                v["d"]["paused"] = false;
                await this.Discord.WsSendAsync(v.ToString(Formatting.None));
                await Task.Delay(7000);
            }
        }

        internal async Task UdpReceiverTask()
        {
            var token = this.ReceiverToken;
            var client = this.UdpClient;

            while (!token.IsCancellationRequested)
            {
                var data = await client.ReceiveAsync().ConfigureAwait(false);
                if (data.Length == 8)
                    this.ProcessKeepalive(data);
                else if (this.Configuration.EnableIncoming)
                    await this.ProcessVideoPacket(data).ConfigureAwait(false);
            }
        }
        
        private async Task ProcessVideoPacket(byte[] data)
        {
            if (data.Length < 13) // minimum packet length
                return;
            try
            {
                var h264Mem = new ReadOnlyMemory<byte>();
                
                if (!this.ProcessPacket(data,  out RtpHeader header, out byte[] payload, out VideoSender sender))
                    return;
                
                await this._videoReceived.InvokeAsync(this, new VideoReceiveEventArgs()
                {
                    SSRC = sender.SSRC,
                    User = sender.User,
                    H264Data = h264Mem,
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Discord.Logger.LogError(VideoNextEvents.VideoReceiveFailure, ex, "Exception occurred when decoding incoming audio data");
            }
        }

        private void ProcessKeepalive(byte[] data)
        {
            try
            {
                var keepalive = BinaryPrimitives.ReadUInt64LittleEndian(data);

                if (!this.KeepaliveTimestamps.TryRemove(keepalive, out var timestamp))
                    return;

                var tdelta = (int)(((Stopwatch.GetTimestamp() - timestamp) / (double)Stopwatch.Frequency) * 1000);
                this.Discord.Logger.LogDebug(VideoNextEvents.VideoKeepalive, "Received UDP keepalive {0} (ping {1}ms)", keepalive, tdelta);
                Volatile.Write(ref this._udpPing, tdelta);
            }
            catch (Exception ex)
            {
                this.Discord.Logger.LogError(VideoNextEvents.VideoKeepalive, ex, "Exception occurred when handling keepalive");
            }
        }

        public async Task KeepaliveVideoAsync()
        {
            await Task.Yield();
            var token = this.KeepaliveToken;
            var client = this.UdpClient;

            while (!token.IsCancellationRequested)
            {
                var timestamp = Stopwatch.GetTimestamp();
                var keepalive = Volatile.Read(ref this._lastKeepalive);
                Volatile.Write(ref this._lastKeepalive, keepalive + 1);
                this.KeepaliveTimestamps.TryAdd(keepalive, timestamp);

                var packet = new byte[8];
                BinaryPrimitives.WriteUInt64LittleEndian(packet, keepalive);

                await client.SendAsync(packet, packet.Length).ConfigureAwait(false);

                await Task.Delay(5000, token);
            }
        }
        
        
        internal bool PreparePacket(ReadOnlySpan<byte> h264, out byte[] target, out int length)
        {
            target = null;
            length = 0;

            if (this.IsDisposed)
                return false;
            
            
            RtpHeader h = new RtpHeader();
            
            h.Sequence = this.Sequence;
            h.Sequence++;
            
            //Broken, needs to be investigated.
            h.Timestamp = this.Timestamp;
            
            //Broken, needs to be investigated.
            h.SSRC = this.SSRC;
            
            //Even though the official client's video packets include an extension header, try not to?
            h.HasExtension = false;
            h.Version = 0x2;
            h.Padding = false;
            h.HasExtension = false;
            h.CSRC = null;
            h.Marker = false;
            h.PayloadType = 102;
            


            var packetArray = ArrayPool<byte>.Shared.Rent(h264.Length + h.Bytes.Length + 24);
            var packet = packetArray.AsSpan();
            

            //Sets the beginning of the packet to the resultant product of the header.
            for (int i = 0; i < h.Bytes.Length; i++)
            {
                packet[i] = h.Bytes[i];
            }
            
            this.Sequence++;
            //this.Timestamp += (uint)audioFormat.CalculateFrameSize(audioFormat.CalculateSampleDuration(pcm.Length));

            byte[] nonce = SodiumCore.GetRandomBytes(12);

            var h26 = packet.Slice(h.Bytes.Length, h264.Length);

            Span<byte> encrypted = Sodium.SecretAeadAes.Encrypt(h26.ToArray(), nonce, this.Key);

            encrypted.CopyTo(packet.Slice(h.Bytes.Length));
            
            nonce.CopyTo(packet.Slice(h.Size + encrypted.Length));
            target = packetArray;
            length = packet.Length;
            return true;
        }
        
        internal async Task VideoSenderTask()
        {
            var token = this.SenderToken;
            var client = this.UdpClient;
            var reader = this.TransmitChannel.Reader;

            byte[] data = null;
            int length = 0;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var hasPacket = reader.TryRead(out var rawPacket);
                    if (hasPacket)
                    {
                        this._queueCount--;

                        if (this.PlayingWait == null || this.PlayingWait.Task.IsCompleted)
                            this.PlayingWait = new TaskCompletionSource<bool>();
                
                        hasPacket = PreparePacket(rawPacket.Bytes.Span, out data, out length);
                        if (rawPacket.RentedBuffer != null)
                            ArrayPool<byte>.Shared.Return(rawPacket.RentedBuffer);
                
                        await client.SendAsync(data, length).ConfigureAwait(false);
                        ArrayPool<byte>.Shared.Return(data);
                    }
                }
                catch (Exception ex)
                {
                    this.Discord.Logger.LogCritical(VideoNextEvents.Misc, "An exception has occured during transmission!", ex);
                }
            }
        }
        
        

        internal async Task EnqueuePacketAsync(RawVideoPacket packet)
        {
            await this.TransmitChannel.Writer.WriteAsync(packet);
            this._queueCount++;
        }

        private bool ProcessPacket(ReadOnlySpan<byte> data, out RtpHeader header, out byte[] payload, out VideoSender sender)
        {
            header = new RtpHeader(data.ToArray());
            payload = data.Slice(header.Size).ToArray();
            sender = new VideoSender(header.SSRC);
            return true;
        }
        
        internal async Task HeartbeatVideoAsync()
        {

            var token = this.Token;
            while (true)
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    var dt = DateTime.Now;
                    this.Discord.Logger.LogTrace(VideoNextEvents.VideoHeartbeat, "Sent heartbeat");

                    var hbd = new VideoDispatch
                    {
                        OpCode = 3,
                        Payload = UnixTimestamp(dt)
                    };
                    var hbj = JsonConvert.SerializeObject(hbd);
                    await this.WsSendAsync(hbj).ConfigureAwait(false);

                    this.LastHeartbeat = dt;
                    await Task.Delay(this.HeartbeatInterval, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            } 
        }
        
        private static uint UnixTimestamp(DateTime dt)
        {
            var ts = dt - UnixEpoch;
            var sd = ts.TotalSeconds;
            var si = (uint)sd;
            return si;
        }

        internal async Task StartAsync()
        {
            _ = Task.Run(async () => await StreamUpdaterTask());
            var state = Discord.GetVoiceNext().GetConnection(TargetChannel.Guild).StateData;
            var vdp = new VideoDispatch();
            
            if (!this.Resume)
            {
                vdp.OpCode = 0;
                vdp.Payload = new VideoIdentifyPayload
                {
                    ServerId = this.StateData.RtcServerId,
                    UserId = state.UserId.ToString(),
                    SessionId = state.SessionId,
                    Token = this.StateData.Token,
                    Video = true,
                };
                this.Resume = true;
            }
            else
            {
                vdp.OpCode = 7;
                vdp.Payload = new VideoIdentifyPayload
                {
                    ServerId = state.GuildId.ToString(),
                    SessionId = state.SessionId,
                    Token = this.StateData.Token,
                    Video = true
                };
            }

            await this.WsSendAsync(JsonConvert.SerializeObject(vdp, Formatting.None)).ConfigureAwait(false);
        }

        internal Task WaitForReadyAsync()
            => this.ReadyWait.Task;

        private async Task Stage1(VideoReadyPayload videoReady)
        {
            //IP Discover
            this.UdpClient.Setup(this.UdpEndpoint);
            
            var pck = new byte[70];
            PreparePacket(pck);
            await this.UdpClient.SendAsync(pck, pck.Length).ConfigureAwait(false);

            var ipd = await this.UdpClient.ReceiveAsync().ConfigureAwait(false);
            ReadPacket(ipd, out var ip, out var port);
            this.DiscoveredEndpoint = new IpEndpoint
            {
                Address = ip,
                Port = port
            };
            this.Discord.Logger.LogTrace(VideoNextEvents.VideoHandshake, "Endpoint dicovery finished - discovered endpoint is {0}:{1}", ip, port);

            void PreparePacket(byte[] packet)
            {
                var ssrc = this.SSRC;
                var packetSpan = packet.AsSpan();
                MemoryMarshal.Write(packetSpan, ref ssrc);
                Helpers.ZeroFill(packetSpan);
            }

            void ReadPacket(byte[] packet, out System.Net.IPAddress decodedIp, out ushort decodedPort)
            {
                var packetSpan = packet.AsSpan();

                var ipString = Utilities.UTF8.GetString(packet, 4, 64 /* 70 - 6 */).TrimEnd('\0');
                decodedIp = System.Net.IPAddress.Parse(ipString);

                decodedPort = BinaryPrimitives.ReadUInt16LittleEndian(packetSpan.Slice(68 /* 70 - 2 */));
            }
            
            this.Discord.Logger.LogTrace(VideoNextEvents.VideoHandshake, "Selected encryption mode is aes256_gcm");
            
            //TODO: add new POCO object to make this cleaner.
            JArray codecs = new JArray();
            JObject h264Codec = new JObject();
            h264Codec["name"] = "H264";
            h264Codec["type"] = "video";
            h264Codec["priority"] = 2000;
            h264Codec["payload_type"] = 101;
            h264Codec["rtx_payload_type"] = 102;
            
            JObject opusCodec = new JObject();
            opusCodec["name"] = "opus";
            opusCodec["type"] = "audio";
            opusCodec["priority"] = 1000;
            opusCodec["payload_type"] = 120;
            codecs.Add(h264Codec);
            codecs.Add(opusCodec);

            var vsp = new VideoDispatch
            {
                OpCode = 1,
                Payload = new VideoSelectProtocolPayload
                {
                    Protocol = "udp",
                    Data = new VideoSelectProtocolPayloadData()
                    {
                        Address = this.DiscoveredEndpoint.Address.ToString(),
                        Port = (ushort) this.DiscoveredEndpoint.Port,
                        Mode = "aead_aes256_gcm"
                    },
                    Codecs = codecs,
                    Address = this.DiscoveredEndpoint.Address.ToString(),
                    Port = (ushort) this.DiscoveredEndpoint.Port,
                    Mode = "aead_aes256_gcm"
                }
            };
            await this.WsSendAsync(JsonConvert.SerializeObject(vsp, Formatting.None)).ConfigureAwait(false);
            
            this.SenderTokenSource = new CancellationTokenSource();
            this.SenderTask = Task.Run(async () => await this.VideoSenderTask(), this.SenderToken);
            this.ReceiverTokenSource = new CancellationTokenSource();
            this.ReceiverTask = Task.Run(async() => await this.UdpReceiverTask(), this.ReceiverToken);
        }

        private async Task Stage2(VideoSessionDescriptionPayload sessionDescription)
        {
            //TODO: send video data!

            this.KeepaliveTask = this.KeepaliveVideoAsync();
            this.ReadyWait.SetResult(true);
            var nullData = new byte[40];
            for (var i = 0; i < 3; i++)
            {
                var nullH264 = new byte[nullData.Length];
                var nullPack = nullH264.AsMemory();
                await this.EnqueuePacketAsync(new RawVideoPacket(nullPack, 20, true));
            }
        }

        private async Task HandleDispatch(JObject jo)
        {
            var opc = (int) jo["op"];
            var opp = jo["d"] as JObject;

            switch (opc)
            {
           case 2: // READY
                    this.Discord.Logger.LogTrace(VideoNextEvents.VideoDispatch, "Received READY (OP2)");
                    //TODO: introduce a local field, VideoReadyPayload
                    var vrp = opp.ToObject<VideoReadyPayload>();
                    this.UdpEndpoint = new ConnectionEndpoint(vrp.Address, vrp.Port);
                    this.HeartbeatTask = Task.Run(async () => await this.HeartbeatVideoAsync());
                    await this.Stage1(vrp).ConfigureAwait(false);
                    break;

                case 4: // SESSION_DESCRIPTION
                    this.Discord.Logger.LogTrace(VideoNextEvents.VideoDispatch, "Received SESSION_DESCRIPTION (OP4)");
                    var vsd = opp.ToObject<VideoSessionDescriptionPayload>();
                    this.Key = vsd.SecretKey;
                    this.SSRC = (uint)vsd.Encodings[1]["rtx_ssrc"];
                    await this.Stage2(vsd).ConfigureAwait(false);
                    break;
                
                case 6: // HEARTBEAT ACK
                    var dt = DateTime.Now;
                    var ping = (int)(dt - this.LastHeartbeat).TotalMilliseconds;
                    Volatile.Write(ref this._wsPing, ping);
                    this.Discord.Logger.LogTrace(VideoNextEvents.VideoDispatch, "Received HEARTBEAT_ACK (OP6, {0}ms)", ping);
                    this.LastHeartbeat = dt;
                    break;

                case 8: // HELLO
                    // this sends a heartbeat interval that we need to use for heartbeating
                    this.Discord.Logger.LogTrace(VideoNextEvents.VideoDispatch, "Received HELLO (OP8)");
                    this.HeartbeatInterval = opp["heartbeat_interval"].ToObject<int>();
                    break;

                case 9: // RESUMED
                    this.Discord.Logger.LogTrace(VideoNextEvents.VideoDispatch, "Received RESUMED (OP9)");
                    this.HeartbeatTask = Task.Run(async () => await this.HeartbeatVideoAsync());
                    break;

                case 12: // CLIENT_CONNECTED
                    this.Discord.Logger.LogTrace(VideoNextEvents.VideoDispatch, "Received CLIENT_CONNECTED (OP12)");;
                    break;

                case 13: // CLIENT_DISCONNECTED
                    this.Discord.Logger.LogTrace(VideoNextEvents.VideoDispatch, "Received CLIENT_DISCONNECTED (OP13)");
                    break;

                default:
                    this.Discord.Logger.LogTrace(VideoNextEvents.VideoDispatch, "Received unknown voice opcode (OP{0})", opc);
                    break;
            }
        }

        public void Dispose()
        {
            VideoDisconnected.Invoke(this.TargetChannel.Guild);
        }
        

        internal Task WsSendAsync(string payload)
        {
            this.Discord.Logger.LogTrace(VideoNextEvents.VideoWsTx, payload);
            return this.VideoWs.SendMessageAsync(payload);
        }

        private Task VideoWS_SocketMessage(IWebSocketClient client, SocketMessageEventArgs e)
        {
            if (!(e is SocketTextMessageEventArgs et))
            {
                this.Discord.Logger.LogCritical(VideoNextEvents.VideoGatewayError, "Discord Voice Gateway sent binary data - unable to process");
                return Task.CompletedTask;
            }

            this.Discord.Logger.LogTrace(VideoNextEvents.VideoWsRx, et.Message);
            return this.HandleDispatch(JObject.Parse(et.Message));
        }

        private async Task VideoWS_SocketClosed(IWebSocketClient client, SocketCloseEventArgs e)
        {
            this.Discord.Logger.LogDebug(VideoNextEvents.VideoConnectionClose, "Video WebSocket closed ({0}, '{1}')", e.CloseCode, e.CloseMessage);
            if (e.CloseCode == 4006 || e.CloseCode == 4009)
                this.Resume = false;

            if (!this.IsDisposed)
            {
                this.TokenSource.Cancel();
                this.TokenSource = new CancellationTokenSource();
                this.VideoWs = this.Discord.Configuration.WebSocketClientFactory(this.Discord.Configuration.Proxy);
                this.VideoWs.Disconnected += this.VideoWS_SocketClosed;
                this.VideoWs.Connected += this.VideoWS_SocketOpen;
                this.VideoWs.MessageReceived += this.VideoWS_SocketMessage;
                this.VideoWs.ExceptionThrown += this.VideoWS_SocketException;

                if (this.Resume)
                    await this.ConnectAsync().ConfigureAwait(false);
            }

        }

        private Task VideoWS_SocketOpen(IWebSocketClient client, SocketEventArgs e)
            => this.StartAsync();

        private Task VideoWS_SocketException(IWebSocketClient client, SocketErrorEventArgs e)
            => this._videoSocketError.InvokeAsync(this, new SocketErrorEventArgs {Exception = e.Exception});
        
    }
}