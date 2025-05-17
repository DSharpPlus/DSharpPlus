using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Net.Serialization;
using DSharpPlus.Net.Udp;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VoiceNext.Codec;
using DSharpPlus.VoiceNext.Entities;
using DSharpPlus.VoiceNext.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.VoiceNext;

internal delegate Task VoiceDisconnectedEventHandler(DiscordGuild guild);

/// <summary>
/// VoiceNext connection to a voice channel.
/// </summary>
public sealed class VoiceNextConnection : IDisposable
{
    /// <summary>
    /// Triggered whenever a user speaks in the connected voice channel.
    /// </summary>
    public event AsyncEventHandler<VoiceNextConnection, UserSpeakingEventArgs> UserSpeaking
    {
        add => this.userSpeaking.Register(value);
        remove => this.userSpeaking.Unregister(value);
    }
    private readonly AsyncEvent<VoiceNextConnection, UserSpeakingEventArgs> userSpeaking;

    /// <summary>
    /// Triggered whenever a user joins voice in the connected guild.
    /// </summary>
    public event AsyncEventHandler<VoiceNextConnection, VoiceUserJoinEventArgs> UserJoined
    {
        add => this.userJoined.Register(value);
        remove => this.userJoined.Unregister(value);
    }
    private readonly AsyncEvent<VoiceNextConnection, VoiceUserJoinEventArgs> userJoined;

    /// <summary>
    /// Triggered whenever a user leaves voice in the connected guild.
    /// </summary>
    public event AsyncEventHandler<VoiceNextConnection, VoiceUserLeaveEventArgs> UserLeft
    {
        add => this.userLeft.Register(value);
        remove => this.userLeft.Unregister(value);
    }
    private readonly AsyncEvent<VoiceNextConnection, VoiceUserLeaveEventArgs> userLeft;

    /// <summary>
    /// Triggered whenever voice data is received from the connected voice channel.
    /// </summary>
    public event AsyncEventHandler<VoiceNextConnection, VoiceReceiveEventArgs> VoiceReceived
    {
        add => this.voiceReceived.Register(value);
        remove => this.voiceReceived.Unregister(value);
    }
    private readonly AsyncEvent<VoiceNextConnection, VoiceReceiveEventArgs> voiceReceived;

    /// <summary>
    /// Triggered whenever voice WebSocket throws an exception.
    /// </summary>
    public event AsyncEventHandler<VoiceNextConnection, SocketErrorEventArgs> VoiceSocketErrored
    {
        add => this.voiceSocketError.Register(value);
        remove => this.voiceSocketError.Unregister(value);
    }
    private readonly AsyncEvent<VoiceNextConnection, SocketErrorEventArgs> voiceSocketError;

    internal event VoiceDisconnectedEventHandler VoiceDisconnected;

    private static DateTimeOffset UnixEpoch { get; } = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private DiscordClient Discord { get; }
    private DiscordGuild Guild { get; }
    private ConcurrentDictionary<uint, AudioSender> TransmittingSSRCs { get; }

    private BaseUdpClient UdpClient { get; }
    private IWebSocketClient VoiceWs { get; set; }
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
    private IpEndpoint DiscoveredEndpoint { get; set; }
    internal ConnectionEndpoint WebSocketEndpoint { get; set; }
    internal ConnectionEndpoint UdpEndpoint { get; set; }

    private TaskCompletionSource<bool> ReadyWait { get; set; }
    private bool IsInitialized { get; set; }
    private bool IsDisposed { get; set; }

    private TaskCompletionSource<bool> PlayingWait { get; set; }

    private AsyncManualResetEvent PauseEvent { get; }
    private VoiceTransmitSink TransmitStream { get; set; }
    private Channel<RawVoicePacket> TransmitChannel { get; }
    private ConcurrentDictionary<ulong, long> KeepaliveTimestamps { get; }
    private ulong lastKeepalive = 0;

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

    private volatile bool isSpeaking = false;

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
        => Volatile.Read(ref this.wsPing);
    private int wsPing = 0;

    /// <summary>
    /// Gets the UDP round-trip time in ms.
    /// </summary>
    public int UdpPing
        => Volatile.Read(ref this.udpPing);
    private int udpPing = 0;

    private int queueCount;

    /// <summary>
    /// Gets the channel this voice client is connected to.
    /// </summary>
    public DiscordChannel TargetChannel { get; internal set; }

    internal VoiceNextConnection(DiscordClient client, DiscordGuild guild, DiscordChannel channel, VoiceNextConfiguration config, VoiceServerUpdatePayload server, VoiceStateUpdatePayload state)
    {
        this.Discord = client;
        this.Guild = guild;
        this.TargetChannel = channel;
        this.TransmittingSSRCs = new ConcurrentDictionary<uint, AudioSender>();

        DefaultClientErrorHandler errorHandler = new(client.Logger);

        this.userSpeaking = new AsyncEvent<VoiceNextConnection, UserSpeakingEventArgs>(errorHandler);
        this.userJoined = new AsyncEvent<VoiceNextConnection, VoiceUserJoinEventArgs>(errorHandler);
        this.userLeft = new AsyncEvent<VoiceNextConnection, VoiceUserLeaveEventArgs>(errorHandler);
        this.voiceReceived = new AsyncEvent<VoiceNextConnection, VoiceReceiveEventArgs>(errorHandler);
        this.voiceSocketError = new AsyncEvent<VoiceNextConnection, SocketErrorEventArgs>(errorHandler);
        this.TokenSource = new CancellationTokenSource();

        this.Configuration = config;
        this.Opus = new Opus(this.AudioFormat);
        //this.Sodium = new Sodium();
        this.Rtp = new Rtp();

        this.ServerData = server;
        this.StateData = state;

        string eps = this.ServerData.Endpoint;
        int epi = eps.LastIndexOf(':');
        string eph = string.Empty;
        int epp = 443;
        if (epi != -1)
        {
            eph = eps[..epi];
            epp = int.Parse(eps[(epi + 1)..]);
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
        this.TransmitChannel = Channel.CreateBounded<RawVoicePacket>(new BoundedChannelOptions(this.Configuration.PacketQueueSize));
        this.KeepaliveTimestamps = new ConcurrentDictionary<ulong, long>();
        this.PauseEvent = new AsyncManualResetEvent(true);

        this.UdpClient = this.Discord.Configuration.UdpClientFactory();
        this.VoiceWs = new WebSocketClient(errorHandler);
        this.VoiceWs.Disconnected += VoiceWS_SocketClosedAsync;
        this.VoiceWs.MessageReceived += VoiceWS_SocketMessage;
        this.VoiceWs.Connected += VoiceWS_SocketOpened;
        this.VoiceWs.ExceptionThrown += VoiceWs_SocketException;
    }

    /// <summary>
    /// Connects to the specified voice channel.
    /// </summary>
    /// <returns>A task representing the connection operation.</returns>
    internal Task ConnectAsync()
    {
        UriBuilder gwuri = new()
        {
            Scheme = "wss",
            Host = this.WebSocketEndpoint.Hostname,
            Query = "encoding=json&v=4"
        };

        return this.VoiceWs.ConnectAsync(gwuri.Uri);
    }

    internal Task ReconnectAsync()
        => this.VoiceWs.DisconnectAsync();

    internal async Task StartAsync()
    {
        // Let's announce our intentions to the server
        VoiceDispatch vdp = new();

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
        string vdj = JsonConvert.SerializeObject(vdp, Formatting.None);
        await WsSendAsync(vdj);
    }

    internal Task WaitForReadyAsync()
        => this.ReadyWait.Task;

    internal async Task EnqueuePacketAsync(RawVoicePacket packet, CancellationToken token = default)
    {
        await this.TransmitChannel.Writer.WriteAsync(packet, token);
        this.queueCount++;
    }

    internal bool PreparePacket(ReadOnlySpan<byte> pcm, out byte[] target, out int length)
    {
        target = null;
        length = 0;

        if (this.IsDisposed)
        {
            return false;
        }

        AudioFormat audioFormat = this.AudioFormat;

        byte[] packetArray = ArrayPool<byte>.Shared.Rent(Rtp.CalculatePacketSize(audioFormat.SampleCountToSampleSize(audioFormat.CalculateMaximumFrameSize()), this.SelectedEncryptionMode));
        Span<byte> packet = packetArray.AsSpan();

        Rtp.EncodeHeader(this.Sequence, this.Timestamp, this.SSRC, packet);
        Span<byte> opus = packet.Slice(Rtp.HeaderSize, pcm.Length);
        this.Opus.Encode(pcm, ref opus);

        this.Sequence++;
        this.Timestamp += (uint)audioFormat.CalculateFrameSize(audioFormat.CalculateSampleDuration(pcm.Length));

        Span<byte> nonce = stackalloc byte[Sodium.NonceSize];
        switch (this.SelectedEncryptionMode)
        {
            case EncryptionMode.AeadAes256GcmRtpSize:
                Sodium.GenerateNonce(this.Nonce++, nonce);
                break;

            default:
                ArrayPool<byte>.Shared.Return(packetArray);
                throw new Exception("Unsupported encryption mode.");
        }

        Span<byte> encrypted = stackalloc byte[Sodium.CalculateTargetSize(opus)];
        this.Sodium.Encrypt(opus, encrypted, nonce);
        encrypted.CopyTo(packet[Rtp.HeaderSize..]);
        packet = packet[..Rtp.CalculatePacketSize(encrypted.Length, this.SelectedEncryptionMode)];
        Sodium.AppendNonce(nonce, packet, this.SelectedEncryptionMode);

        target = packetArray;
        length = packet.Length;
        return true;
    }

    private async Task VoiceSenderTaskAsync()
    {
        CancellationToken token = this.SenderToken;
        BaseUdpClient client = this.UdpClient;
        ChannelReader<RawVoicePacket> reader = this.TransmitChannel.Reader;

        byte[] data = null;
        int length = 0;

        double synchronizerTicks = Stopwatch.GetTimestamp();
        double synchronizerResolution = Stopwatch.Frequency * 0.005;
        double tickResolution = 10_000_000.0 / Stopwatch.Frequency;
        this.Discord.Logger.LogDebug(VoiceNextEvents.Misc, "Timer accuracy: {Frequency}/{Resolution} (high resolution? {IsHighRes})", Stopwatch.Frequency, synchronizerResolution, Stopwatch.IsHighResolution);

        while (!token.IsCancellationRequested)
        {
            await this.PauseEvent.WaitAsync();

            bool hasPacket = reader.TryRead(out RawVoicePacket rawPacket);
            if (hasPacket)
            {
                this.queueCount--;

                if (this.PlayingWait == null || this.PlayingWait.Task.IsCompleted)
                {
                    this.PlayingWait = new TaskCompletionSource<bool>();
                }
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

            if (hasPacket)
            {
                hasPacket = PreparePacket(rawPacket.Bytes.Span, out data, out length);
                if (rawPacket.RentedBuffer != null)
                {
                    ArrayPool<byte>.Shared.Return(rawPacket.RentedBuffer);
                }
            }

            int durationModifier = hasPacket ? rawPacket.Duration / 5 : 4;
            double cts = Math.Max(Stopwatch.GetTimestamp() - synchronizerTicks, 0);
            if (cts < synchronizerResolution * durationModifier)
            {
                await Task.Delay(TimeSpan.FromTicks((long)(((synchronizerResolution * durationModifier) - cts) * tickResolution)));
            }

            synchronizerTicks += synchronizerResolution * durationModifier;

            if (!hasPacket)
            {
                continue;
            }

            await SendSpeakingAsync(true);
            await client.SendAsync(data, length);
            ArrayPool<byte>.Shared.Return(data);

            if (!rawPacket.Silence && this.queueCount == 0)
            {
                byte[] nullpcm = new byte[this.AudioFormat.CalculateSampleSize(20)];
                for (int i = 0; i < 3; i++)
                {
                    byte[] nullpacket = new byte[nullpcm.Length];
                    Memory<byte> nullpacketmem = nullpacket.AsMemory();
                    await EnqueuePacketAsync(new RawVoicePacket(nullpacketmem, 20, true));
                }
            }
            else if (this.queueCount == 0)
            {
                await SendSpeakingAsync(false);
                this.PlayingWait?.SetResult(true);
            }
        }
    }

    private bool ProcessPacket(ReadOnlySpan<byte> data, ref Memory<byte> opus, ref Memory<byte> pcm, List<ReadOnlyMemory<byte>> pcmPackets, out AudioSender voiceSender, out AudioFormat outputFormat)
    {
        voiceSender = null;
        outputFormat = default;

        if (!Rtp.IsRtpHeader(data))
        {
            return false;
        }

        Rtp.DecodeHeader(data, out ushort shortSequence, out uint _, out uint ssrc, out bool hasExtension);

        if (!this.TransmittingSSRCs.TryGetValue(ssrc, out AudioSender? vtx))
        {
            OpusDecoder decoder = this.Opus.CreateDecoder();

            vtx = new AudioSender(ssrc, decoder)
            {
                // user isn't present as we haven't received a speaking event yet.
                User = null
            };
        }

        voiceSender = vtx;
        ulong sequence = vtx.GetTrueSequenceAfterWrapping(shortSequence);
        ushort gap = 0;
        if (vtx.LastTrueSequence is ulong lastTrueSequence)
        {
            if (sequence <= lastTrueSequence) // out-of-order packet; discard
            {
                return false;
            }

            gap = (ushort)(sequence - 1 - lastTrueSequence);
            if (gap >= 5)
            {
                this.Discord.Logger.LogWarning(VoiceNextEvents.VoiceReceiveFailure, "5 or more voice packets were dropped when receiving");
            }
        }

        Span<byte> nonce = stackalloc byte[Sodium.NonceSize];
        Sodium.GetNonce(data, nonce, this.SelectedEncryptionMode);
        Rtp.GetDataFromPacket(data, out ReadOnlySpan<byte> encryptedOpus, this.SelectedEncryptionMode);

        int opusSize = Sodium.CalculateSourceSize(encryptedOpus);
        opus = opus[..opusSize];
        Span<byte> opusSpan = opus.Span;
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
                    int headerLen = (opusSpan[2] << 8) | opusSpan[3];
                    int i = 4;
                    for (; i < headerLen + 4; i++)
                    {
                        byte @byte = opusSpan[i];

                        // ID is currently unused since we skip it anyway
                        //var id = (byte)(@byte >> 4);
                        int length = (byte)(@byte & 0x0F) + 1;

                        i += length;
                    }

                    // Strip extension padding too
                    while (opusSpan[i] == 0)
                    {
                        i++;
                    }

                    opusSpan = opusSpan[i..];
                }

                // TODO: consider implementing RFC 5285, 4.3. Two-Byte Header
            }

            if (opusSpan[0] == 0x90)
            {
                // I'm not 100% sure what this header is/does, however removing the data causes no
                // real issues, and has the added benefit of removing a lot of noise.
                opusSpan = opusSpan[2..];
            }

            if (gap == 1)
            {
                int lastSampleCount = Opus.GetLastPacketSampleCount(vtx.Decoder);
                byte[] fecpcm = new byte[this.AudioFormat.SampleCountToSampleSize(lastSampleCount)];
                Span<byte> fecpcmMem = fecpcm.AsSpan();
                this.Opus.Decode(vtx.Decoder, opusSpan, ref fecpcmMem, true, out _);
                pcmPackets.Add(fecpcm.AsMemory(0, fecpcmMem.Length));
            }
            else if (gap > 1)
            {
                int lastSampleCount = Opus.GetLastPacketSampleCount(vtx.Decoder);
                for (int i = 0; i < gap; i++)
                {
                    byte[] fecpcm = new byte[this.AudioFormat.SampleCountToSampleSize(lastSampleCount)];
                    Span<byte> fecpcmMem = fecpcm.AsSpan();
                    Opus.ProcessPacketLoss(vtx.Decoder, lastSampleCount, ref fecpcmMem);
                    pcmPackets.Add(fecpcm.AsMemory(0, fecpcmMem.Length));
                }
            }

            Span<byte> pcmSpan = pcm.Span;
            this.Opus.Decode(vtx.Decoder, opusSpan, ref pcmSpan, false, out outputFormat);
            pcm = pcm[..pcmSpan.Length];
        }
        finally
        {
            vtx.LastTrueSequence = sequence;
        }

        return true;
    }

    private async Task ProcessVoicePacketAsync(byte[] data)
    {
        if (data.Length < 13) // minimum packet length
        {
            return;
        }

        try
        {
            byte[] pcm = new byte[this.AudioFormat.CalculateMaximumFrameSize()];
            Memory<byte> pcmMem = pcm.AsMemory();
            byte[] opus = new byte[pcm.Length];
            Memory<byte> opusMem = opus.AsMemory();
            List<ReadOnlyMemory<byte>> pcmFillers = [];
            if (!ProcessPacket(data, ref opusMem, ref pcmMem, pcmFillers, out AudioSender? vtx, out AudioFormat audioFormat))
            {
                return;
            }

            foreach (ReadOnlyMemory<byte> pcmFiller in pcmFillers)
            {
                await this.voiceReceived.InvokeAsync(this, new VoiceReceiveEventArgs
                {
                    SSRC = vtx.SSRC,
                    User = vtx.User,
                    PcmData = pcmFiller,
                    OpusData = Array.Empty<byte>(),
                    AudioFormat = audioFormat,
                    AudioDuration = audioFormat.CalculateSampleDuration(pcmFiller.Length)
                });
            }

            await this.voiceReceived.InvokeAsync(this, new VoiceReceiveEventArgs
            {
                SSRC = vtx.SSRC,
                User = vtx.User,
                PcmData = pcmMem,
                OpusData = opusMem,
                AudioFormat = audioFormat,
                AudioDuration = audioFormat.CalculateSampleDuration(pcmMem.Length)
            });
        }
        catch (Exception ex)
        {
            this.Discord.Logger.LogError(VoiceNextEvents.VoiceReceiveFailure, ex, "Exception occurred when decoding incoming audio data");
        }
    }

    private void ProcessKeepalive(byte[] data)
    {
        try
        {
            ulong keepalive = BinaryPrimitives.ReadUInt64LittleEndian(data);

            if (!this.KeepaliveTimestamps.TryRemove(keepalive, out long timestamp))
            {
                return;
            }

            int tdelta = (int)((Stopwatch.GetTimestamp() - timestamp) / (double)Stopwatch.Frequency * 1000);
            this.Discord.Logger.LogDebug(VoiceNextEvents.VoiceKeepalive, "Received UDP keepalive {KeepAlive} (ping {Ping}ms)", keepalive, tdelta);
            Volatile.Write(ref this.udpPing, tdelta);
        }
        catch (Exception ex)
        {
            this.Discord.Logger.LogError(VoiceNextEvents.VoiceKeepalive, ex, "Exception occurred when handling keepalive");
        }
    }

    private async Task UdpReceiverTaskAsync()
    {
        CancellationToken token = this.ReceiverToken;
        BaseUdpClient client = this.UdpClient;

        while (!token.IsCancellationRequested)
        {
            byte[] data = await client.ReceiveAsync();
            if (data.Length == 8)
            {
                ProcessKeepalive(data);
            }
            else if (this.Configuration.EnableIncoming)
            {
                await ProcessVoicePacketAsync(data);
            }
        }
    }

    /// <summary>
    /// Sends a speaking status to the connected voice channel.
    /// </summary>
    /// <param name="speaking">Whether the current user is speaking or not.</param>
    /// <returns>A task representing the sending operation.</returns>
    public async Task SendSpeakingAsync(bool speaking = true)
    {
        if (!this.IsInitialized)
        {
            throw new InvalidOperationException("The connection is not initialized");
        }

        if (this.isSpeaking != speaking)
        {
            this.isSpeaking = speaking;
            VoiceDispatch pld = new()
            {
                OpCode = 5,
                Payload = new VoiceSpeakingPayload
                {
                    Speaking = speaking,
                    Delay = 0
                }
            };

            string plj = JsonConvert.SerializeObject(pld, Formatting.None);
            await WsSendAsync(plj);
        }
    }

    /// <summary>
    /// Gets a transmit stream for this connection, optionally specifying a packet size to use with the stream. If a stream is already configured, it will return the existing one.
    /// </summary>
    /// <param name="sampleDuration">Duration, in ms, to use for audio packets.</param>
    /// <returns>Transmit stream.</returns>
    public VoiceTransmitSink GetTransmitSink(int sampleDuration = 20)
    {
        if (!AudioFormat.AllowedSampleDurations.Contains(sampleDuration))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleDuration), "Invalid PCM sample duration specified.");
        }

        this.TransmitStream ??= new VoiceTransmitSink(this, sampleDuration);
        return this.TransmitStream;
    }

    /// <summary>
    /// Asynchronously waits for playback to be finished. Playback is finished when speaking = false is signalled.
    /// </summary>
    /// <returns>A task representing the waiting operation.</returns>
    public async Task WaitForPlaybackFinishAsync()
    {
        if (this.PlayingWait != null)
        {
            await this.PlayingWait.Task;
        }
    }

    /// <summary>
    /// Pauses playback.
    /// </summary>
    public void Pause()
        => this.PauseEvent.Reset();

    /// <summary>
    /// Asynchronously resumes playback.
    /// </summary>
    /// <returns></returns>
    public async Task ResumeAsync()
        => await this.PauseEvent.SetAsync();

    /// <summary>
    /// Disconnects and disposes this voice connection.
    /// </summary>
    public void Disconnect()
        => Dispose();

    /// <summary>
    /// Disconnects and disposes this voice connection.
    /// </summary>
    public void Dispose()
    {
        if (this.IsDisposed)
        {
            return;
        }

        this.IsDisposed = true;
        this.IsInitialized = false;
        this.TokenSource?.Cancel();
        this.SenderTokenSource?.Cancel();
        this.ReceiverTokenSource?.Cancel();
        this.KeepaliveTokenSource?.Cancel();

        this.TokenSource?.Dispose();
        this.SenderTokenSource?.Dispose();
        this.ReceiverTokenSource?.Dispose();
        this.KeepaliveTokenSource?.Dispose();

        try
        {
            this.VoiceWs.DisconnectAsync().GetAwaiter().GetResult();
            this.UdpClient.Close();
        }
        catch { }

        this.Opus?.Dispose();
        this.Opus = null!;
        this.Sodium?.Dispose();
        this.Sodium = null!;
        this.Rtp?.Dispose();
        this.Rtp = null!;

        this.VoiceDisconnected?.Invoke(this.Guild);
    }

    private async Task HeartbeatAsync()
    {
        await Task.Yield();

        CancellationToken token = this.Token;
        while (true)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                DateTime dt = DateTime.Now;
                this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceHeartbeat, "Sent heartbeat");

                VoiceDispatch hbd = new()
                {
                    OpCode = 3,
                    Payload = UnixTimestamp(dt)
                };
                string hbj = JsonConvert.SerializeObject(hbd);
                await WsSendAsync(hbj);

                this.LastHeartbeat = dt;
                await Task.Delay(this.HeartbeatInterval);
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

        CancellationToken token = this.KeepaliveToken;
        BaseUdpClient client = this.UdpClient;

        while (!token.IsCancellationRequested)
        {
            long timestamp = Stopwatch.GetTimestamp();
            ulong keepalive = Volatile.Read(ref this.lastKeepalive);
            Volatile.Write(ref this.lastKeepalive, keepalive + 1);
            this.KeepaliveTimestamps.TryAdd(keepalive, timestamp);

            byte[] packet = new byte[8];
            BinaryPrimitives.WriteUInt64LittleEndian(packet, keepalive);

            await client.SendAsync(packet, packet.Length);

            await Task.Delay(5000, token);
        }
    }

    private async Task Stage1Async(VoiceReadyPayload voiceReady)
    {
        // IP Discovery
        this.UdpClient.Setup(this.UdpEndpoint);

        byte[] pck = new byte[74];
        PreparePacket(pck);

        await this.UdpClient.SendAsync(pck, pck.Length);

        byte[] ipd = await this.UdpClient.ReceiveAsync();
        ReadPacket(ipd, out System.Net.IPAddress? ip, out ushort port);
        this.DiscoveredEndpoint = new IpEndpoint
        {
            Address = ip,
            Port = port
        };
        this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceHandshake, "Endpoint discovery finished - discovered endpoint is {Ip}:{Port}", ip, port);

        void PreparePacket(byte[] packet)
        {
            uint ssrc = this.SSRC;
            ushort type = 0x1; // type: request (isn't this one way anyway?)
            ushort length = 70; // length of everything after this. should for this step always be 70.

            Span<byte> packetSpan = packet.AsSpan();
            Helpers.ZeroFill(packetSpan); // fill with zeroes

            byte[] typeByte = BitConverter.GetBytes(type);
            byte[] lengthByte = BitConverter.GetBytes(length);
            byte[] ssrcByte = BitConverter.GetBytes(ssrc);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(typeByte);
                Array.Reverse(lengthByte);
                Array.Reverse(ssrcByte);
            }

            typeByte.CopyTo(packet, 0);
            lengthByte.CopyTo(packet, 2);
            ssrcByte.CopyTo(packet, 4);
            // https://discord.com/developers/docs/topics/voice-connections#ip-discovery
        }

        void ReadPacket(byte[] packet, out System.Net.IPAddress decodedIp, out ushort decodedPort)
        {
            Span<byte> packetSpan = packet.AsSpan();

            // the packet we received in this step should be the IP discovery response.

            // it has the same format as PreparePacket. All we really need is IP + port so we strip it from
            // the response here, which are the last 6 bytes (4 for ip, 2 for port (ushort))

            string ipString = Utilities.UTF8.GetString(packet, 8, 64 /* 74 - 6 */).TrimEnd('\0');
            decodedIp = System.Net.IPAddress.Parse(ipString);
            decodedPort = BinaryPrimitives.ReadUInt16LittleEndian(packetSpan[72 /* 74 - 2 */..]);
        }

        // Select voice encryption mode
        KeyValuePair<string, EncryptionMode> selectedEncryptionMode = Sodium.SelectMode(voiceReady.Modes);
        this.SelectedEncryptionMode = selectedEncryptionMode.Value;

        // Ready
        this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceHandshake, "Selected encryption mode is {EncryptionMode}", selectedEncryptionMode.Key);
        VoiceDispatch vsp = new()
        {
            OpCode = 1,
            Payload = new VoiceSelectProtocolPayload
            {
                Protocol = "udp",
                Data = new VoiceSelectProtocolPayloadData
                {
                    Address = this.DiscoveredEndpoint.Address.ToString(),
                    Port = (ushort)this.DiscoveredEndpoint.Port,
                    Mode = selectedEncryptionMode.Key
                }
            }
        };
        string vsj = JsonConvert.SerializeObject(vsp, Formatting.None);
        await WsSendAsync(vsj);

        this.SenderTokenSource = new CancellationTokenSource();
        this.SenderTask = Task.Run(VoiceSenderTaskAsync, this.SenderToken);

        this.ReceiverTokenSource = new CancellationTokenSource();
        this.ReceiverTask = Task.Run(UdpReceiverTaskAsync, this.ReceiverToken);
    }

    private async Task Stage2Async(VoiceSessionDescriptionPayload voiceSessionDescription)
    {
        this.SelectedEncryptionMode = Sodium.SupportedModes[voiceSessionDescription.Mode.ToLowerInvariant()];
        this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceHandshake, "Discord updated encryption mode - new mode is {EncryptionMode}", this.SelectedEncryptionMode);

        // start keepalive
        this.KeepaliveTokenSource = new CancellationTokenSource();
        this.KeepaliveTask = KeepaliveAsync();

        // send 3 packets of silence to get things going
        byte[] nullpcm = new byte[this.AudioFormat.CalculateSampleSize(20)];
        for (int i = 0; i < 3; i++)
        {
            byte[] nullPcm = new byte[nullpcm.Length];
            Memory<byte> nullpacketmem = nullPcm.AsMemory();
            await EnqueuePacketAsync(new RawVoicePacket(nullpacketmem, 20, true));
        }

        this.IsInitialized = true;
        this.ReadyWait.SetResult(true);
    }

    private async Task HandleDispatchAsync(JObject jo)
    {
        int opc = (int)jo["op"];
        JObject? opp = jo["d"] as JObject;

        switch (opc)
        {
            case 2: // READY
                this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received READY (OP2)");
                VoiceReadyPayload vrp = opp.ToDiscordObject<VoiceReadyPayload>();
                this.SSRC = vrp.SSRC;
                this.UdpEndpoint = new ConnectionEndpoint(vrp.Address, vrp.Port);
                // this is not the valid interval
                // oh, discord
                //this.HeartbeatInterval = vrp.HeartbeatInterval;
                this.HeartbeatTask = Task.Run(HeartbeatAsync);
                await Stage1Async(vrp);
                break;

            case 4: // SESSION_DESCRIPTION
                this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received SESSION_DESCRIPTION (OP4)");
                VoiceSessionDescriptionPayload vsd = opp.ToDiscordObject<VoiceSessionDescriptionPayload>();
                this.Key = vsd.SecretKey;
                this.Sodium = new Sodium(this.Key.AsMemory());
                await Stage2Async(vsd);
                break;

            case 5: // SPEAKING
                // Don't spam OP5
                // No longer spam, Discord supposedly doesn't send many of these
                this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received SPEAKING (OP5)");
                VoiceSpeakingPayload spd = opp.ToDiscordObject<VoiceSpeakingPayload>();
                bool foundUserInCache = this.Discord.TryGetCachedUserInternal(spd.UserId.Value, out DiscordUser? resolvedUser);
                UserSpeakingEventArgs spk = new()
                {
                    Speaking = spd.Speaking,
                    SSRC = spd.SSRC.Value,
                    User = resolvedUser,
                };

                if (foundUserInCache && this.TransmittingSSRCs.TryGetValue(spk.SSRC, out AudioSender? txssrc5) && txssrc5.Id == 0)
                {
                    txssrc5.User = spk.User;
                }
                else
                {
                    OpusDecoder opus = this.Opus.CreateDecoder();
                    AudioSender vtx = new(spk.SSRC, opus)
                    {
                        User = await this.Discord.GetUserAsync(spd.UserId.Value)
                    };

                    if (!this.TransmittingSSRCs.TryAdd(spk.SSRC, vtx))
                    {
                        this.Opus.DestroyDecoder(opus);
                    }
                }

                await this.userSpeaking.InvokeAsync(this, spk);
                break;

            case 6: // HEARTBEAT ACK
                DateTime dt = DateTime.Now;
                int ping = (int)(dt - this.LastHeartbeat).TotalMilliseconds;
                Volatile.Write(ref this.wsPing, ping);
                this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received HEARTBEAT_ACK (OP6, {Heartbeat}ms)", ping);
                this.LastHeartbeat = dt;
                break;

            case 8: // HELLO
                // this sends a heartbeat interval that we need to use for heartbeating
                this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received HELLO (OP8)");
                this.HeartbeatInterval = opp["heartbeat_interval"].ToDiscordObject<int>();
                break;

            case 9: // RESUMED
                this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received RESUMED (OP9)");
                this.HeartbeatTask = Task.Run(HeartbeatAsync);
                break;

            case 12: // CLIENT_CONNECTED
                this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received CLIENT_CONNECTED (OP12)");
                VoiceUserJoinPayload ujpd = opp.ToDiscordObject<VoiceUserJoinPayload>();
                DiscordUser usrj = await this.Discord.GetUserAsync(ujpd.UserId);
                {
                    OpusDecoder opus = this.Opus.CreateDecoder();
                    AudioSender vtx = new(ujpd.SSRC, opus)
                    {
                        User = usrj
                    };

                    if (!this.TransmittingSSRCs.TryAdd(vtx.SSRC, vtx))
                    {
                        this.Opus.DestroyDecoder(opus);
                    }
                }

                await this.userJoined.InvokeAsync(this, new VoiceUserJoinEventArgs { User = usrj, SSRC = ujpd.SSRC });
                break;

            case 13: // CLIENT_DISCONNECTED
                this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received CLIENT_DISCONNECTED (OP13)");
                VoiceUserLeavePayload ulpd = opp.ToDiscordObject<VoiceUserLeavePayload>();
                KeyValuePair<uint, AudioSender> txssrc = this.TransmittingSSRCs.FirstOrDefault(x => x.Value.Id == ulpd.UserId);
                if (this.TransmittingSSRCs.ContainsKey(txssrc.Key))
                {
                    this.TransmittingSSRCs.TryRemove(txssrc.Key, out AudioSender? txssrc13);
                    this.Opus.DestroyDecoder(txssrc13.Decoder);
                }

                DiscordUser usrl = await this.Discord.GetUserAsync(ulpd.UserId);
                await this.userLeft.InvokeAsync(this, new VoiceUserLeaveEventArgs
                {
                    User = usrl,
                    SSRC = txssrc.Key
                });
                break;

            default:
                this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received unknown voice opcode (OP{Op})", opc);
                break;
        }
    }

    private async Task VoiceWS_SocketClosedAsync(IWebSocketClient client, SocketClosedEventArgs e)
    {
        this.Discord.Logger.LogDebug(VoiceNextEvents.VoiceConnectionClose, "Voice WebSocket closed ({CloseCode}, '{CloseMessage}')", e.CloseCode, e.CloseMessage);

        // generally this should not be disposed on all disconnects, only on requested ones
        // or something
        // otherwise problems happen
        //this.Dispose();

        if (e.CloseCode is 4006 or 4009)
        {
            this.Resume = false;
        }

        if (!this.IsDisposed)
        {
            this.TokenSource.Cancel();
            this.TokenSource = new CancellationTokenSource();
            this.VoiceWs = new WebSocketClient(new DefaultClientErrorHandler(this.Discord.Logger));
            this.VoiceWs.Disconnected += VoiceWS_SocketClosedAsync;
            this.VoiceWs.MessageReceived += VoiceWS_SocketMessage;
            this.VoiceWs.Connected += VoiceWS_SocketOpened;

            if (this.Resume) // emzi you dipshit
            {
                await ConnectAsync();
            }
        }
    }

    private Task VoiceWS_SocketMessage(IWebSocketClient client, SocketMessageEventArgs e)
    {
        if (e is not SocketTextMessageEventArgs et)
        {
            this.Discord.Logger.LogCritical(VoiceNextEvents.VoiceGatewayError, "Discord Voice Gateway sent binary data - unable to process");
            return Task.CompletedTask;
        }

        this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceWsRx, "{WebsocketMessage}", et.Message);
        return HandleDispatchAsync(JObject.Parse(et.Message));
    }

    private Task VoiceWS_SocketOpened(IWebSocketClient client, SocketEventArgs e)
        => StartAsync();

    private Task VoiceWs_SocketException(IWebSocketClient client, SocketErrorEventArgs e)
        => this.voiceSocketError.InvokeAsync(this, new SocketErrorEventArgs { Exception = e.Exception });

    private async Task WsSendAsync(string payload)
    {
        this.Discord.Logger.LogTrace(VoiceNextEvents.VoiceWsTx, "{WebsocketPayload}", payload);
        await this.VoiceWs.SendMessageAsync(payload);
    }

    private static uint UnixTimestamp(DateTime dt)
    {
        TimeSpan ts = dt - UnixEpoch;
        double sd = ts.TotalSeconds;
        uint si = (uint)sd;
        return si;
    }
}

// Naam you still owe me those noodles :^)
// I remember
// Alexa, how much is shipping to emzi
// NL -> PL is 18.50€ for packages <=2kg it seems (https://www.postnl.nl/en/mail-and-parcels/parcels/international-parcel/)
