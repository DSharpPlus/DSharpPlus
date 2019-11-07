using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Net.Udp;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VoiceNext.Codec;
using DSharpPlus.VoiceNext.Entities;
using DSharpPlus.VoiceNext.EventArgs;
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
        /// Triggered whenever a user joins voice in the connected guild.
        /// </summary>
        public event AsyncEventHandler<VoiceUserJoinEventArgs> UserJoined
        {
            add { this._userJoined.Register(value); }
            remove { this._userJoined.Unregister(value); }
        }
        private AsyncEvent<VoiceUserJoinEventArgs> _userJoined;

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

        private static DateTimeOffset UnixEpoch { get; } = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private DiscordClient Discord { get; }
        private DiscordGuild Guild { get; }
#if !NETSTANDARD1_1
        private ConcurrentDictionary<uint, AudioSender> TransmittingSSRCs { get; }
#endif

        private BaseUdpClient UdpClient { get; }
        private BaseWebSocketClient VoiceWs { get; set; }
        private Task HeartbeatTask { get; set; }
        private int HeartbeatInterval { get; set; }
        private DateTimeOffset LastHeartbeat { get; set; }

        private CancellationTokenSource TokenSource { get; set; }
        private CancellationToken Token
            => this.TokenSource.Token;

        internal VoiceServerUpdatePayload ServerData { get; set; }
        internal VoiceStateUpdatePayload StateData { get; set; }
        internal bool Resume { get; set; }

        private VoiceNextConfiguration Configuration { get; }
        private Opus Opus { get; set; }
        private Sodium Sodium { get; set; }
        private Rtp Rtp { get; set; }
        private EncryptionMode SelectedEncryptionMode { get; set; }
        private uint Nonce { get; set; } = 0;

        private ushort Sequence { get; set; }
        private uint Timestamp { get; set; }
        private uint SSRC { get; set; }
        private byte[] Key { get; set; }
#if !NETSTANDARD1_1
        private IpEndpoint DiscoveredEndpoint { get; set; }
#endif
        internal ConnectionEndpoint WebSocketEndpoint { get; set; }
        internal ConnectionEndpoint UdpEndpoint { get; set; }

        private TaskCompletionSource<bool> ReadyWait { get; set; }
        private bool IsInitialized { get; set; }
        private bool IsDisposed { get; set; }

        private TaskCompletionSource<bool> PlayingWait { get; set; }

        private ConcurrentQueue<VoicePacket> PacketQueue { get; }
        private VoiceTransmitStream TransmitStream { get; set; }
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
        /// Gets the audio format used by the Opus encoder.
        /// </summary>
        public AudioFormat AudioFormat => this.Configuration.AudioFormat;

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

        /// <summary>
        /// Gets the channel this voice client is connected to.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        internal VoiceNextConnection(DiscordClient client, DiscordGuild guild, DiscordChannel channel, VoiceNextConfiguration config, VoiceServerUpdatePayload server, VoiceStateUpdatePayload state)
        {
            this.Discord = client;
            this.Guild = guild;
            this.Channel = channel;
#if !NETSTANDARD1_1
            this.TransmittingSSRCs = new ConcurrentDictionary<uint, AudioSender>();
#endif

            this._userSpeaking = new AsyncEvent<UserSpeakingEventArgs>(this.Discord.EventErrorHandler, "VNEXT_USER_SPEAKING");
            this._userJoined = new AsyncEvent<VoiceUserJoinEventArgs>(this.Discord.EventErrorHandler, "VNEXT_USER_JOINED");
            this._userLeft = new AsyncEvent<VoiceUserLeaveEventArgs>(this.Discord.EventErrorHandler, "VNEXT_USER_LEFT");
#if !NETSTANDARD1_1
            this._voiceReceived = new AsyncEvent<VoiceReceiveEventArgs>(this.Discord.EventErrorHandler, "VNEXT_VOICE_RECEIVED");
#endif
            this._voiceSocketError = new AsyncEvent<SocketErrorEventArgs>(this.Discord.EventErrorHandler, "VNEXT_WS_ERROR");
            this.TokenSource = new CancellationTokenSource();

            this.Configuration = config;
            this.Opus = new Opus(this.AudioFormat);
            //this.Sodium = new Sodium();
            this.Rtp = new Rtp();

            this.ServerData = server;
            this.StateData = state;

            var eps = this.ServerData.Endpoint;
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
            this.PacketQueue = new ConcurrentQueue<VoicePacket>();
            this.KeepaliveTimestamps = new ConcurrentDictionary<ulong, long>();

            this.UdpClient = this.Discord.Configuration.UdpClientFactory();
            this.VoiceWs = this.Discord.Configuration.WebSocketClientFactory(this.Discord.Configuration.Proxy);
            this.VoiceWs.Disconnected += this.VoiceWS_SocketClosed;
            this.VoiceWs.MessageReceived += this.VoiceWS_SocketMessage;
            this.VoiceWs.Connected += this.VoiceWS_SocketOpened;
            this.VoiceWs.Errored += this.VoiceWs_SocketErrored;
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
                Host = this.WebSocketEndpoint.Hostname,
                Query = "encoding=json&v=4"
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

        internal void PreparePacket(ReadOnlySpan<byte> pcm, ref Memory<byte> target)
        {
            var audioFormat = this.AudioFormat;

            var packetArray = ArrayPool<byte>.Shared.Rent(this.Rtp.CalculatePacketSize(audioFormat.SampleCountToSampleSize(audioFormat.CalculateMaximumFrameSize()), this.SelectedEncryptionMode));
            var packet = packetArray.AsSpan();

            this.Rtp.EncodeHeader(this.Sequence, this.Timestamp, this.SSRC, packet);
            var opus = packet.Slice(Rtp.HeaderSize, pcm.Length);
            this.Opus.Encode(pcm, ref opus);

            this.Sequence++;
            this.Timestamp += (uint)audioFormat.CalculateFrameSize(audioFormat.CalculateSampleDuration(pcm.Length));

            Span<byte> nonce = stackalloc byte[Sodium.NonceSize];
            switch (this.SelectedEncryptionMode)
            {
                case EncryptionMode.XSalsa20_Poly1305:
                    this.Sodium.GenerateNonce(packet.Slice(0, Rtp.HeaderSize), nonce);
                    break;

#if !NETSTANDARD1_1
                case EncryptionMode.XSalsa20_Poly1305_Suffix:
                    this.Sodium.GenerateNonce(nonce);
                    break;
#endif

                case EncryptionMode.XSalsa20_Poly1305_Lite:
                    this.Sodium.GenerateNonce(this.Nonce++, nonce);
                    break;

                default:
                    ArrayPool<byte>.Shared.Return(packetArray);
                    throw new Exception("Unsupported encryption mode.");
            }

            Span<byte> encrypted = stackalloc byte[Sodium.CalculateTargetSize(opus)];
            this.Sodium.Encrypt(opus, encrypted, nonce);
            encrypted.CopyTo(packet.Slice(Rtp.HeaderSize));
            packet = packet.Slice(0, this.Rtp.CalculatePacketSize(encrypted.Length, this.SelectedEncryptionMode));
            this.Sodium.AppendNonce(nonce, packet, this.SelectedEncryptionMode);

            target = target.Slice(0, packet.Length);
            packet.CopyTo(target.Span);
            ArrayPool<byte>.Shared.Return(packetArray);
        }

        internal void EnqueuePacket(VoicePacket packet)
            => this.PacketQueue.Enqueue(packet);

        private async Task VoiceSenderTask()
        {
            var token = this.SenderToken;
            var client = this.UdpClient;
            var queue = this.PacketQueue;

            var synchronizerTicks = (double)Stopwatch.GetTimestamp();
            var synchronizerResolution = (Stopwatch.Frequency * 0.005);
            var tickResolution = 10_000_000.0 / Stopwatch.Frequency;
            this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"Timer accuracy: {Stopwatch.Frequency.ToString("#,##0", CultureInfo.InvariantCulture)}/{synchronizerResolution.ToString(CultureInfo.InvariantCulture)} (high resolution? {Stopwatch.IsHighResolution})", DateTime.Now);

            while (!token.IsCancellationRequested)
            {
                var hasPacket = queue.TryDequeue(out var packet);

                byte[] packetArray = null;
                if (hasPacket)
                {
                    if (this.PlayingWait == null || this.PlayingWait.Task.IsCompleted)
                        this.PlayingWait = new TaskCompletionSource<bool>();

                    packetArray = packet.Bytes.ToArray();
                }

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

                var durationModifier = hasPacket ? packet.MillisecondDuration / 5 : 4;
                var cts = Math.Max(Stopwatch.GetTimestamp() - synchronizerTicks, 0);
                if (cts < synchronizerResolution * durationModifier)
                    await Task.Delay(TimeSpan.FromTicks((long)(((synchronizerResolution * durationModifier) - cts) * tickResolution))).ConfigureAwait(false);

                synchronizerTicks += synchronizerResolution * durationModifier;

                if (!hasPacket)
                    continue;

                this.SendSpeaking(true);
                await this.UdpClient.SendAsync(packetArray, packetArray.Length).ConfigureAwait(false);

                if (!packet.IsSilence && queue.Count == 0)
                {
                    var nullpcm = new byte[this.AudioFormat.CalculateSampleSize(20)];
                    for (var i = 0; i < 3; i++)
                    {
                        var nullpacket = new byte[nullpcm.Length];
                        var nullpacketmem = nullpacket.AsMemory();

                        this.PreparePacket(nullpcm, ref nullpacketmem);
                        this.EnqueuePacket(new VoicePacket(nullpacketmem, 20, true));
                    }
                }
                else if (queue.Count == 0)
                {
                    this.SendSpeaking(false);
                    this.PlayingWait?.SetResult(true);
                }
            }
        }

#if !NETSTANDARD1_1
        private bool ProcessPacket(ReadOnlySpan<byte> data, ref Memory<byte> opus, ref Memory<byte> pcm, IList<ReadOnlyMemory<byte>> pcmPackets, out AudioSender voiceSender, out AudioFormat outputFormat)
        {
            voiceSender = null;
            outputFormat = default;
            if (!this.Rtp.IsRtpHeader(data))
                return false;

            this.Rtp.DecodeHeader(data, out var sequence, out var timestamp, out var ssrc, out var hasExtension);

            var vtx = this.TransmittingSSRCs[ssrc];
            voiceSender = vtx;
            if (sequence <= vtx.LastSequence) // out-of-order packet; discard
                return false;
            var gap = vtx.LastSequence != 0 ? sequence - 1 - vtx.LastSequence : 0;

            if (gap >= 5)
                this.Discord.DebugLogger.LogMessage(LogLevel.Warning, "VNext RX", "5 or more voice packets were dropped when receiving", DateTime.Now);

            Span<byte> nonce = stackalloc byte[Sodium.NonceSize];
            this.Sodium.GetNonce(data, nonce, this.SelectedEncryptionMode);
            this.Rtp.GetDataFromPacket(data, out var encryptedOpus, this.SelectedEncryptionMode);

            var opusSize = Sodium.CalculateSourceSize(encryptedOpus);
            opus = opus.Slice(0, opusSize);
            var opusSpan = opus.Span;
            try
            {
                this.Sodium.Decrypt(encryptedOpus, opusSpan, nonce);

                // Strip extensions, if any
                if (hasExtension)
                {
                    // RFC 5285, 4.2 One-Byte header
                    // http://www.rfcreader.com/#rfc5285_line186
                    if (opusSpan[0] == 0xBE && opusSpan[1] == 0xDE)
                    {
                        var headerLen = opusSpan[2] << 8 | opusSpan[3];
                        var i = 4;
                        for (; i < headerLen + 4; i++)
                        {
                            var @byte = opusSpan[i];

                            // ID is currently unused since we skip it anyway
                            //var id = (byte)(@byte >> 4);
                            var length = (byte)(@byte & 0x0F) + 1;

                            i += length;
                        }

                        // Strip extension padding too
                        while (opusSpan[i] == 0)
                            i++;

                        opusSpan = opusSpan.Slice(i);
                    }

                    // TODO: consider implementing RFC 5285, 4.3. Two-Byte Header
                }

                if (opusSpan[0] == 0x90)
                {
                    // I'm not 100% sure what this header is/does, however removing the data causes no
                    // real issues, and has the added benefit of removing a lot of noise. 
                    opusSpan = opusSpan.Slice(2); 
                }

                if (gap == 1)
                {
                    var lastSampleCount = this.Opus.GetLastPacketSampleCount(vtx.Decoder);
                    var fecpcm = new byte[this.AudioFormat.SampleCountToSampleSize(lastSampleCount)];
                    var fecpcmMem = fecpcm.AsSpan();
                    this.Opus.Decode(vtx.Decoder, opusSpan, ref fecpcmMem, true, out _);
                    pcmPackets.Add(fecpcm.AsMemory(0, fecpcmMem.Length));
                }
                else if (gap > 1)
                {
                    var lastSampleCount = this.Opus.GetLastPacketSampleCount(vtx.Decoder);
                    for (var i = 0; i < gap; i++)
                    {
                        var fecpcm = new byte[this.AudioFormat.SampleCountToSampleSize(lastSampleCount)];
                        var fecpcmMem = fecpcm.AsSpan();
                        this.Opus.ProcessPacketLoss(vtx.Decoder, lastSampleCount, ref fecpcmMem);
                        pcmPackets.Add(fecpcm.AsMemory(0, fecpcmMem.Length));
                    }
                }

                var pcmSpan = pcm.Span;
                this.Opus.Decode(vtx.Decoder, opusSpan, ref pcmSpan, false, out outputFormat);
                pcm = pcm.Slice(0, pcmSpan.Length);
            }
            finally
            {
                vtx.LastSequence = sequence;
            }

            return true;
        }

        private async Task ProcessVoicePacket(byte[] data)
        {
            if (data.Length < 13) // minimum packet length
                return;

            try
            {
                var pcm = new byte[this.AudioFormat.CalculateMaximumFrameSize()];
                var pcmMem = pcm.AsMemory();
                var opus = new byte[pcm.Length];
                var opusMem = opus.AsMemory();
                var pcmFillers = new List<ReadOnlyMemory<byte>>();
                if (!this.ProcessPacket(data, ref opusMem, ref pcmMem, pcmFillers, out var vtx, out var audioFormat))
                    return;

                foreach (var pcmFiller in pcmFillers)
                    await this._voiceReceived.InvokeAsync(new VoiceReceiveEventArgs(this.Discord)
                    {
                        SSRC = vtx.SSRC,
                        User = vtx.User,
                        PcmData = pcmFiller,
                        OpusData = new byte[0].AsMemory(),
                        AudioFormat = audioFormat,
                        AudioDuration = audioFormat.CalculateSampleDuration(pcmFiller.Length)
                    }).ConfigureAwait(false);

                await this._voiceReceived.InvokeAsync(new VoiceReceiveEventArgs(this.Discord)
                {
                    SSRC = vtx.SSRC,
                    User = vtx.User,
                    PcmData = pcmMem,
                    OpusData = opusMem,
                    AudioFormat = audioFormat,
                    AudioDuration = audioFormat.CalculateSampleDuration(pcmMem.Length)
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Discord.DebugLogger.LogMessage(LogLevel.Error, "VNext RX", "Exception occured when decoding incoming audio data", DateTime.Now, ex);
            }
        }
#endif

        private void ProcessKeepalive(byte[] data)
        {
            try
            {
                var keepalive = BinaryPrimitives.ReadUInt64LittleEndian(data);

                if (!this.KeepaliveTimestamps.TryRemove(keepalive, out var timestamp))
                    return;

                var tdelta = (int)(((Stopwatch.GetTimestamp() - timestamp) / (double)Stopwatch.Frequency) * 1000);
                Volatile.Write(ref this._udpPing, tdelta);
                this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VNext UDP", $"Received UDP keepalive {keepalive}, ping {tdelta}ms", DateTime.Now);
            }
            catch (Exception ex)
            {
                this.Discord.DebugLogger.LogMessage(LogLevel.Error, "VNext UDP", "Exception occured when handling keepalive", DateTime.Now, ex);
            }
        }

        private async Task UdpReceiverTask()
        {
            var token = this.ReceiverToken;
            var client = this.UdpClient;

            while (!token.IsCancellationRequested)
            {
                var data = await client.ReceiveAsync().ConfigureAwait(false);
                if (data.Length == 8)
                    this.ProcessKeepalive(data);
#if !NETSTANDARD1_1
                else if (this.Configuration.EnableIncoming)
                    await this.ProcessVoicePacket(data).ConfigureAwait(false);
#endif
            }
        }

        /// <summary>
        /// Sends a speaking status to the connected voice channel.
        /// </summary>
        /// <param name="speaking">Whether the current user is speaking or not.</param>
        /// <returns>A task representing the sending operation.</returns>
        public void SendSpeaking(bool speaking = true)
        {
            if (!this.IsInitialized)
                throw new InvalidOperationException("The connection is not initialized");

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
        /// Gets a transmit stream for this connection, optionally specifying a packet size to use with the stream. If a stream is already configured, it will return the existing one.
        /// </summary>
        /// <param name="sampleDuration">Duration, in ms, to use for audio packets.</param>
        /// <returns>Transmit stream.</returns>
        public VoiceTransmitStream GetTransmitStream(int sampleDuration = 20)
        {
            if (!AudioFormat.AllowedSampleDurations.Contains(sampleDuration))
                throw new ArgumentOutOfRangeException(nameof(sampleDuration), "Invalid PCM sample duration specified.");

            if (this.TransmitStream == null)
                this.TransmitStream = new VoiceTransmitStream(this, sampleDuration);

            return this.TransmitStream;
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
            this.SenderTokenSource.Cancel();
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
            this.Sodium?.Dispose();
            this.Sodium = null;
            this.Rtp?.Dispose();
            this.Rtp = null;

            if (this.VoiceDisconnected != null)
                this.VoiceDisconnected(this.Guild);
        }

        private async Task HeartbeatAsync()
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

        private async Task KeepaliveAsync()
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

        private async Task Stage1(VoiceReadyPayload voiceReady)
        {
#if !NETSTANDARD1_1
            // IP Discovery
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
            this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VNext UDP", $"Endpoint discovery resulted in {ip}:{port}", DateTime.Now);

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

                var ipString = new UTF8Encoding(false).GetString(packet, 4, 64 /* 70 - 6 */).TrimEnd('\0');
                decodedIp = System.Net.IPAddress.Parse(ipString);

                decodedPort = BinaryPrimitives.ReadUInt16LittleEndian(packetSpan.Slice(68 /* 70 - 2 */));
            }
#else
            this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VNext UDP", $"Voice receive not supported - not performing endpoint discovery", DateTime.Now);
            await Task.Yield(); // just stop bothering me VS
#endif

            // Select voice encryption mode
            var selectedEncryptionMode = Sodium.SelectMode(voiceReady.Modes);
            this.SelectedEncryptionMode = selectedEncryptionMode.Value;

            // Ready
            this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"Selected encryption mode: {selectedEncryptionMode.Key}", DateTime.Now);
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
                        Mode = selectedEncryptionMode.Key
                    }
                }
            };
            var vsj = JsonConvert.SerializeObject(vsp, Formatting.None);
            this.VoiceWs.SendMessage(vsj);

            this.SenderTokenSource = new CancellationTokenSource();
            this.SenderTask = Task.Run(this.VoiceSenderTask, this.SenderToken);

            this.ReceiverTokenSource = new CancellationTokenSource();
            this.ReceiverTask = Task.Run(this.UdpReceiverTask, this.ReceiverToken);
        }

        private Task Stage2(VoiceSessionDescriptionPayload voiceSessionDescription)
        {
            this.SelectedEncryptionMode = Sodium.SupportedModes[voiceSessionDescription.Mode.ToLowerInvariant()];
            this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"Discord updated encryption mode: {this.SelectedEncryptionMode}", DateTime.Now);

            // start keepalive
            this.KeepaliveTokenSource = new CancellationTokenSource();
            this.KeepaliveTask = this.KeepaliveAsync();

            // send 3 packets of silence to get things going
            var nullpcm = new byte[this.AudioFormat.CalculateSampleSize(20)];
            for (var i = 0; i < 3; i++)
            {
                var nullopus = new byte[nullpcm.Length];
                var nullopusmem = nullopus.AsMemory();
                this.PreparePacket(nullpcm, ref nullopusmem);
                this.EnqueuePacket(new VoicePacket(nullopusmem, 20));
            }

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
                case 2: // READY
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP2 received", DateTime.Now);
                    var vrp = opp.ToObject<VoiceReadyPayload>();
                    this.SSRC = vrp.SSRC;
                    this.UdpEndpoint = new ConnectionEndpoint(vrp.Address, vrp.Port);
                    // this is not the valid interval
                    // oh, discord
                    //this.HeartbeatInterval = vrp.HeartbeatInterval;
                    this.HeartbeatTask = Task.Run(this.HeartbeatAsync);
                    await this.Stage1(vrp).ConfigureAwait(false);
                    break;

                case 4: // SESSION_DESCRIPTION
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP4 received", DateTime.Now);
                    var vsd = opp.ToObject<VoiceSessionDescriptionPayload>();
                    this.Key = vsd.SecretKey;
                    this.Sodium = new Sodium(this.Key.AsMemory());
                    await this.Stage2(vsd).ConfigureAwait(false);
                    break;

                case 5: // SPEAKING
                    // Don't spam OP5
                    //this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP5 received", DateTime.Now);
                    var spd = opp.ToObject<VoiceSpeakingPayload>();
                    var spk = new UserSpeakingEventArgs(this.Discord)
                    {
                        Speaking = spd.Speaking,
                        SSRC = spd.SSRC.Value,
                        User = this.Discord.InternalGetCachedUser(spd.UserId.Value)
                    };

#if !NETSTANDARD1_1
                    if (spk.User != null && this.TransmittingSSRCs.TryGetValue(spk.SSRC, out var txssrc5) && txssrc5.Id == 0)
                    {
                        txssrc5.User = spk.User;
                    }
                    else
                    {
                        var opus = this.Opus.CreateDecoder();
                        var vtx = new AudioSender(spk.SSRC, opus)
                        {
                            User = await this.Discord.GetUserAsync(spd.UserId.Value).ConfigureAwait(false)
                        };

                        if (!this.TransmittingSSRCs.TryAdd(spk.SSRC, vtx))
                            this.Opus.DestroyDecoder(opus);
                    }
#endif

                    await this._userSpeaking.InvokeAsync(spk).ConfigureAwait(false);
                    break;

                case 6: // HEARTBEAT ACK
                    var dt = DateTime.Now;
                    var ping = (int)(dt - this.LastHeartbeat).TotalMilliseconds;
                    Volatile.Write(ref this._wsPing, ping);
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", $"Received voice heartbeat ACK, ping {ping.ToString("#,##0", CultureInfo.InvariantCulture)}ms", dt);
                    this.LastHeartbeat = dt;
                    break;

                case 8: // HELLO
                    // this sends a heartbeat interval that we need to use for heartbeating
                    this.HeartbeatInterval = opp["heartbeat_interval"].ToObject<int>();
                    break;

                case 9: // RESUMED
                    this.Discord.DebugLogger.LogMessage(LogLevel.Debug, "VoiceNext", "OP9 received", DateTime.Now);
                    this.HeartbeatTask = Task.Run(this.HeartbeatAsync);
                    break;

                case 12: // CLIENT_CONNECTED
                    var ujpd = opp.ToObject<VoiceUserJoinPayload>();
                    var usrj = await this.Discord.GetUserAsync(ujpd.UserId).ConfigureAwait(false);

#if !NETSTANDARD1_1
                    {
                        var opus = this.Opus.CreateDecoder();
                        var vtx = new AudioSender(ujpd.SSRC, opus)
                        {
                            User = usrj
                        };

                        if (!this.TransmittingSSRCs.TryAdd(vtx.SSRC, vtx))
                            this.Opus.DestroyDecoder(opus);
                    }
#endif

                    await this._userJoined.InvokeAsync(new VoiceUserJoinEventArgs(this.Discord) { User = usrj, SSRC = ujpd.SSRC }).ConfigureAwait(false);
                    break;

                case 13: // CLIENT_DISCONNECTED
                    var ulpd = opp.ToObject<VoiceUserLeavePayload>();

#if !NETSTANDARD1_1
                    var txssrc = this.TransmittingSSRCs.FirstOrDefault(x => x.Value.Id == ulpd.UserId);
                    if (this.TransmittingSSRCs.ContainsKey(txssrc.Key))
                    {
                        this.TransmittingSSRCs.TryRemove(txssrc.Key, out var txssrc13);
                        this.Opus.DestroyDecoder(txssrc13.Decoder);
                    }
#endif

                    var usrl = await this.Discord.GetUserAsync(ulpd.UserId).ConfigureAwait(false);
                    await this._userLeft.InvokeAsync(new VoiceUserLeaveEventArgs(this.Discord)
                    {
                        User = usrl
#if !NETSTANDARD1_1
                        ,
                        SSRC = txssrc.Key
#endif
                    }).ConfigureAwait(false);
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
                this.VoiceWs.Disconnected += this.VoiceWS_SocketClosed;
                this.VoiceWs.MessageReceived += this.VoiceWS_SocketMessage;
                this.VoiceWs.Connected += this.VoiceWS_SocketOpened;

                if (this.Resume) // emzi you dipshit
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
// Alexa, how much is shipping to emzi
// NL -> PL is 18.50€ for packages <=2kg it seems (https://www.postnl.nl/en/mail-and-parcels/parcels/international-parcel/)