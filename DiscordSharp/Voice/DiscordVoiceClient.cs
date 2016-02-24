extern alias ws4n;
using Discord.Audio.Opus;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
#if NETFX4_5
using ws4n.WebSocket4Net;
using CONCURRENT = System.Collections.Concurrent;
#else
using ws4n.WebSocket4Net;
using CONCURRENT = System.Collections.Concurrent;
#endif

/**
    This file contains the methods necessary for creating a Discord voice connection.

    To any future library developers looking to implement voice, this is how the basic recipe for voice communication goes with Discord.

    Ingredients Needed:
        1. A decent UdpClient
        2. A websocket that doesn't suck. (different from your main one)
        3. An understanding of bitshifting.

    To begin the voice connection:
        1. On your main websocket, send json with opcode 4, the guild_id, and channel_id, and whether or not you want your client to muted/deaf.
        2. After that, you will get a websocket message with t = "VOICE_SERVER_UPDATE".
            2a. The json you receive will contain some IMPORTANT information including voice endpoint (for socket) and a token you will need.
        3. Now, you can initiate your voice client.
        4. Point your websocket client to the given endpoint and upon connection, send opcode 0 with the server ID, channel ID, your client ID, and that token you got prior.
        5. Opcode 2 gives you various parameters for your voice connection.
            5a. ssrc is basically your voice client ID. You will need it for your packet. It differentiates between users.
            5b. Guess what port is for. 
            5c. (I believe) modes is for voice encryption.
            5d. heartbeat_interval is very important. The voice connection is extremely finnicky with its keepalives so **DONT HARDCODE THIS VALUE**. It's usually 5500 ms.
            5e. After opcode 2, you can begin your websocket keepalive.
        6. Now, you should be safe to begin your UDP connection. Point it to the endpoint you used to connect to voice initially BUT do **NOT** use the default port! Use the port that is given to you in opcode 2.
            6a. Also, don't include the wss://
        7. Once connected, send a 70 byte packet containing your ssrc as bigendian for the first 4 bytes.
        8. Your response should be a packet. This packet contains your IP address and port you need. Please use the IP from this packet.
            8a. Quoth thy wise Danny: "the ip is ascii starting at the 4th byte and ending at the first null"
            8b. the port is a little endian unsigned short in the last two bytes
                yes, this is different endianness from everything else
        9. You will then send opcode 1 with your protocol (more than likely udp), your IP from the packet, the port from the packet, and a mode. More than likely "plain"
        10. Finally, once you receive opcode 4, you may send the "speaking" json (opcode 5, speaking = true/false, delay = 0). At this point, you are fully connected and you may begin any other threads/keepalives.
*/

namespace DiscordSharp
{
    internal class VoiceConnectionParameters
    {
        [JsonProperty("ssrc")]
        public int ssrc { get; internal set; }
        [JsonProperty("port")]
        public int port { get; internal set; }
        [JsonProperty("modes")]
        public string[] modes { get; internal set; }
        [JsonProperty("heartbeat_interval")]
        public int heartbeat_interval { get; internal set; }
    }
    
    internal struct DiscordIpPort
    {
        public IPAddress Address;
        public int port;
    }

    public class DiscordAudioPacketEventArgs : EventArgs
    {
        //public DiscordAudioPacket Packet { get; internal set; }
        public byte[] PCMPacket { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordMember FromUser { get; internal set; }
    }

    public class DiscordVoiceConfig
    {
        internal int SampleRate { get; set; } = 48000; //discord default
        /// <summary>
        /// The amount of channels you wish to send over the network.
        /// Use 2 for stereo. Stereo will require more bandwidth.
        /// </summary>
        public int Channels { get; set; } = 1;

        /// <summary>
        /// The frame length, in ms, of audio you'll be sending at one time.
        /// As a rule of thumb, you generally want to set it to something above your current Discord ping.
        /// </summary>
        public int FrameLengthMs { get; set; } = 60;

        /// <summary>
        /// The voice client now auto-configures this based on the channel.
        /// </summary>
        [Obsolete]
        public int? Bitrate { get; set; } = null;

        /// <summary>
        /// The mode the Opus encoder will use.
        /// MusicOrMixed is generally the best.
        /// </summary>
        public OpusApplication OpusMode { get; set; } = OpusApplication.MusicOrMixed;

        /// <summary>
        /// If true, the voice client will only send voice and will not receive it.
        /// </summary>
        public bool SendOnly { get; set; } = true;

        /// <summary>
        /// The blocksize of PCM data you should be reading and piping into DiscordSharp.
        /// </summary>
        public int PCMBlockSize
        {
            get
            {
                return (48 * 2 * Channels * FrameLengthMs);
            }
        }
    }
    
    public class DiscordVoiceClient : IDisposable
    {
        private DiscordClient _parent;

        public bool Connected { get; internal set; }
        public string SessionID { get; internal set; }
        public string VoiceEndpoint { get; internal set; }
        public string Token { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordServer Guild { get; internal set; }
        public DiscordMember Me { get; internal set; }
        private DiscordMember LastSpoken { get; set; }
        private UdpClient _udp = new UdpClient();
        private VoiceConnectionParameters Params { get; set; }
        private Logger VoiceDebugLogger = new Logger();
        private WebSocket VoiceWebSocket;
        private OpusEncoder mainOpusEncoder;
        private CancellationTokenSource globalTaskSource = new CancellationTokenSource();
        //private CONCURRENT.ConcurrentQueue<byte[]> voiceToSend = new CONCURRENT.ConcurrentConcurrentQueue<byte[]>();\
        private CONCURRENT.ConcurrentQueue<byte[]> voiceToSend = new CONCURRENT.ConcurrentQueue<byte[]>();
        private DiscordVoiceConfig VoiceConfig;
        private List<DiscordMember> MembersInChannel = new List<DiscordMember>();
        private Dictionary<DiscordMember, int> SsrcDictionary = new Dictionary<DiscordMember, int>();
        private List<OpusDecoder> OpusDecoders = new List<OpusDecoder>();

#if !V45
        private IPEndPoint udpEndpoint;
#endif

#region Events
        internal event EventHandler<LoggerMessageReceivedArgs> DebugMessageReceived;
        internal event EventHandler<EventArgs> Disposed;
        internal event EventHandler<DiscordVoiceUserSpeakingEventArgs> UserSpeaking;
        internal event EventHandler<DiscordAudioPacketEventArgs> PacketReceived;
        internal event EventHandler<EventArgs> ErrorReceived;
        internal event EventHandler<EventArgs> VoiceConnectionComplete;
        internal event EventHandler<EventArgs> QueueEmpty;
#endregion

#region voice sending stuff
        /// <summary>
        /// The length, in ms, of audio for DiscordSharp to send. By default, 20.
        /// </summary>
        static int msToSend = 20;

        /// <summary>
        /// An signed short designating the sequence of the audio being sent.
        /// </summary>
        internal ushort ___sequence = 0;

        /// <summary>
        /// Unsigned int designating the timestamp offset of the audio being sent.
        /// </summary>
        internal uint ___timestamp = 0;
#endregion

        public Logger GetDebugLogger => VoiceDebugLogger;

        public DiscordVoiceClient(DiscordClient parentClient)
        {
            _parent = parentClient;
            VoiceConfig = new DiscordVoiceConfig();
            InitializeOpusEncoder();
        }

        public DiscordVoiceClient(DiscordClient parentClient, DiscordVoiceConfig config)
        {
            _parent = parentClient;
            VoiceConfig = config;
            InitializeOpusEncoder();
        }

        public DiscordVoiceClient(DiscordClient parentClient, DiscordVoiceConfig config, DiscordChannel channel)
        {
            _parent = parentClient;
            VoiceConfig = config;
            Channel = channel;
            InitializeOpusEncoder();
        }

        private void InitializeOpusEncoder()
        {
            if (Channel != null && Channel.Type == ChannelType.Voice)
            {
                if (Channel.Bitrate > 0)
                {
                    mainOpusEncoder = new OpusEncoder(VoiceConfig.SampleRate,
                        VoiceConfig.Channels,
                        VoiceConfig.FrameLengthMs,
                        (Channel.Bitrate / 1000),
                        VoiceConfig.OpusMode);
                }
            }
            else
            {
                mainOpusEncoder = new OpusEncoder(VoiceConfig.SampleRate,
                    VoiceConfig.Channels,
                    VoiceConfig.FrameLengthMs,
                    null,
                    VoiceConfig.OpusMode);
            }
            mainOpusEncoder.SetForwardErrorCorrection(true);
            msToSend = VoiceConfig.FrameLengthMs;
        }

        private void InitializeOpusDecoder()
        {
            if(Channel != null && Channel.Type == ChannelType.Voice)
            {
                OpusDecoders.Add(new OpusDecoder(VoiceConfig.SampleRate, VoiceConfig.Channels, VoiceConfig.FrameLengthMs));
            }
            else
            {
                OpusDecoders.Add(new OpusDecoder(VoiceConfig.SampleRate, VoiceConfig.Channels, VoiceConfig.FrameLengthMs));
            }
        }

        /// <summary>
        /// Begins the voice client connection.
        /// </summary>
        public void Initiate()
        {
            if(Me == null)
            {
                if (_parent != null)
                {
                    Me = _parent.Me;
                }
                else
                    throw new NullReferenceException("VoiceClient's main client reference was null!");
            }

            MembersInChannel.Add(Me);

            VoiceDebugLogger.LogMessageReceived += (sender, e) =>
            {
                if (DebugMessageReceived != null)
                    DebugMessageReceived(this, e);
            };

            VoiceWebSocket = new WebSocket(VoiceEndpoint.StartsWith("wss://") ? VoiceEndpoint.Replace(":80", "") :
                "wss://" + VoiceEndpoint.Replace(":80", ""));
            VoiceWebSocket.EnableAutoSendPing = false;
            VoiceWebSocket.AllowUnstrustedCertificate = true;
            VoiceWebSocket.NoDelay = true;
            VoiceWebSocket.Closed += VoiceWebSocket_OnClose;
            VoiceWebSocket.Error += VoiceWebSocket_OnError;

            VoiceWebSocket.MessageReceived += async (s, e) =>
            {
                try
                {
                    await VoiceWebSocket_OnMessage(s, e).ConfigureAwait(false);
                }
                catch(Exception ex)
                {
                    VoiceDebugLogger.Log($"Exception while awaiting OnMessage?!\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}", MessageLevel.Critical);
                }
            };
            VoiceWebSocket.Opened += (sender, e) =>
            {
                string initMsg = JsonConvert.SerializeObject(new
                {
                    op = 0,
                    d = new
                    {
                        server_id = Guild.id,
                        user_id = Me.ID,
                        session_id = SessionID,
                        token = Token
                    }
                });

                VoiceDebugLogger.Log("VoiceWebSocket opened, sending initial json. ( " + initMsg + ") ");

                VoiceWebSocket.Send(initMsg);
            };

            VoiceWebSocket.Open();
        }

#pragma warning disable 4014 //stupid await warnings
        private async Task VoiceWebSocket_OnMessage(object sender, MessageReceivedEventArgs e)
        {
            JObject message = JObject.Parse(e.Message);
            switch (int.Parse(message["op"].ToString()))
            {
                case 2:
                    //VoiceDebugLogger.Log(e.Message);
                    await OpCode2(message).ConfigureAwait(false);
                    //ok, now that we have opcode 2 we have to send a packet and configure the UDP
                    await InitialUDPConnection().ConfigureAwait(false);
                    break;
                case 3:
                    VoiceDebugLogger.Log("KeepAlive echoed back successfully!", MessageLevel.Unecessary);
                    break;
                case 4:
                    //post initializing the UDP client, we will receive opcode 4 and will now do the final connection steps
                    await OpCode4(message).ConfigureAwait(false);
                    if (!VoiceConfig.SendOnly)
                        DoUDPKeepAlive(globalTaskSource.Token);
                    SendVoiceTask(globalTaskSource.Token);
                    if (!VoiceConfig.SendOnly)
                        ReceiveVoiceTask(globalTaskSource.Token);
                    SetSpeaking(true);
                    if (VoiceConnectionComplete != null)
                        VoiceConnectionComplete(this, new EventArgs());
                    break;
                case 5: //User speaking
                    //VoiceDebugLogger.Log(e.Message);
                    OpCode5(message);
                    break;
            }
        }
#pragma warning restore 4014 //stupid await warnings

#region Websocket Opcode Events/other misc events
        internal void MemberRemoved(DiscordMember removed)
        {
            if (MembersInChannel.Contains(removed))
            {
                MembersInChannel.Remove(removed);
                VoiceDebugLogger.Log($"User {removed.Username} ({removed.ID}) left the client's current connected voice channel.");
            }
        }
        internal void MemberAdded(DiscordMember added)
        {
            if(added.Parent == Guild)
            {
                if(added.CurrentVoiceChannel == Channel)
                {
                    if (MembersInChannel.Contains(added))
                        MembersInChannel.Remove(added);
                    MembersInChannel.Add(added);
                    VoiceDebugLogger.Log($"User {added.Username} ({added.ID}) joined the client's current connected voice channel.");
                }
            }
        }
        private void OpCode5(JObject message)
        {
            DiscordVoiceUserSpeakingEventArgs e = new DiscordVoiceUserSpeakingEventArgs();
            e.Channel = Channel;
            e.UserSpeaking = Guild.members.Find(x => x.ID == message["d"]["user_id"].ToString());
            e.Speaking = message["d"]["speaking"].ToObject<bool>();
            e.ssrc = message["d"]["ssrc"].ToObject<int>();

            if(e.UserSpeaking != null)
            {
                if (!SsrcDictionary.ContainsKey(e.UserSpeaking))
                    SsrcDictionary.Add(e.UserSpeaking, e.ssrc);
            }

            LastSpoken = e.UserSpeaking;

            if (UserSpeaking != null)
                UserSpeaking(this, e);
        }

        private async Task OpCode4(JObject message)
        {
            string speakingJson = JsonConvert.SerializeObject(new
            {
                op = 5,
                d = new
                {
                    speaking = true,
                    delay = 0
                }
            });
            VoiceDebugLogger.Log("Sending initial speaking json..( " + speakingJson + " )");
            VoiceWebSocket.Send(speakingJson);
            //we are officially connected!!!
            Connected = true;
        }

#pragma warning disable 4014
        private async Task OpCode2(JObject message)
        {
            Params = JsonConvert.DeserializeObject<VoiceConnectionParameters>(message["d"].ToString());
            SsrcDictionary.Add(Me, Params.ssrc);
            //SendWebSocketKeepalive();
            DoWebSocketKeepAlive(globalTaskSource.Token);
        }
#pragma warning restore 4014

        private void VoiceWebSocket_OnError(object sender, EventArgs e)
        {
            VoiceDebugLogger.Log("Error in VoiceWebSocket.");
            if (ErrorReceived != null)
                ErrorReceived(this, new EventArgs());
        }

        private void VoiceWebSocket_OnClose(object sender, EventArgs e)
        {
            VoiceDebugLogger.Log($"VoiceWebSocket was closed.", MessageLevel.Critical);
            if (ErrorReceived != null)
                ErrorReceived(this, new EventArgs());
        }
#endregion

        private bool QueueEmptyEventTriggered = false;
#region Internal Voice Methods
#pragma warning disable 4014
        private Task SendVoiceTask(CancellationToken token)
        {
#if V45
            return Task.Run(async () =>
#else
            return Task.Factory.StartNew(async ()=>
#endif
            {
                while (!token.IsCancellationRequested)
                {
                    if (!voiceToSend.IsEmpty)
                    {
                        QueueEmptyEventTriggered = false;
                        await SendVoiceAsync(token).ConfigureAwait(false);
                    }
                    if (voiceToSend.IsEmpty)
                    {
                        //reset these
                        ___sequence = 0;
                        ___timestamp = 0;
                        if (!QueueEmptyEventTriggered)
                        {
                            if (QueueEmpty != null)
                            {
                                QueueEmpty(this, new EventArgs());
                                QueueEmptyEventTriggered = true;
                            }
                        }
                    }
                }
            });
        }
#pragma warning disable 4014
        private Task ReceiveVoiceTask(CancellationToken token)
        {
            VoiceDebugLogger.Log("Setting up for voice receive.");
#if V45
            return Task.Run(async () =>
#else
            return Task.Factory.StartNew(() =>
#endif
            {
                while(!token.IsCancellationRequested)
                {
#if V45
                    UdpReceiveResult receivedResult = await _udp.ReceiveAsync().ConfigureAwait(false);
                    byte[] receivedBytes = receivedResult.Buffer;
#else
                    byte[] receivedBytes = _udp.Receive(ref udpEndpoint);
#endif
                    if (receivedBytes.Length > 0)
                    {
                        DiscordAudioPacket packet = new DiscordAudioPacket(receivedBytes);
                        if(PacketReceived != null)
                        {
                            DiscordMember memberSpeaking = Me;
                            foreach (var dictItem in SsrcDictionary)
                                if (dictItem.Value == packet.SSRC)
                                    memberSpeaking = dictItem.Key;
                            if (memberSpeaking.ID != Me.ID)
                            {
                                byte[] pcmPacket = new byte[48 * VoiceConfig.Channels * VoiceConfig.FrameLengthMs];
                                OpusDecoders[0].DecodeFrame(packet.AsRawPacket(), 0, packet.AsRawPacket().Length, pcmPacket);
                                PacketReceived(this, new DiscordAudioPacketEventArgs { Channel = this.Channel, FromUser = memberSpeaking, PCMPacket = pcmPacket });
                            }
                        }
                            
                    }
                }
            });
        }
#pragma warning restore 4014
        private async Task SendVoiceAsync(CancellationToken cancelToken)
        {
            byte[] voiceToEncode;
            voiceToSend.TryDequeue(out voiceToEncode);
            if (voiceToEncode != null)
            {
                Stopwatch timeToSend = Stopwatch.StartNew();
                byte[] rtpPacket = new byte[12 + voiceToEncode.Length];
                rtpPacket[0] = (byte)0x80;
                rtpPacket[1] = (byte)0x78;

                rtpPacket[8] = (byte)((Params.ssrc >> 24) & 0xFF);
                rtpPacket[9] = (byte)((Params.ssrc >> 16) & 0xFF);
                rtpPacket[10] = (byte)((Params.ssrc >> 8) & 0xFF);
                rtpPacket[11] = (byte)((Params.ssrc >> 0) & 0xFF);

                byte[] opusAudio = new byte[voiceToEncode.Length];
                int encodedLength = mainOpusEncoder.EncodeFrame(voiceToEncode, 0, opusAudio);
                

                int dataSent = 0;

                //actual sending
                {
                    //sequence big endian
                    rtpPacket[2] = (byte)((___sequence >> 8));
                    rtpPacket[3] = (byte)((___sequence >> 0) & 0xFF);

                    //timestamp big endian
                    rtpPacket[4] = (byte)((___timestamp >> 24) & 0xFF);
                    rtpPacket[5] = (byte)((___timestamp >> 16) & 0xFF);
                    rtpPacket[6] = (byte)((___timestamp >> 8));
                    rtpPacket[7] = (byte)((___timestamp >> 0) & 0xFF);

                    if (opusAudio == null)
                        throw new ArgumentNullException("opusAudio");

                    int maxSize = encodedLength;
                    Buffer.BlockCopy(opusAudio, 0, rtpPacket, 12, encodedLength);

#if V45
                    dataSent = _udp.SendAsync(rtpPacket, encodedLength + 12).Result;
#else
                    dataSent = _udp.Send(rtpPacket, encodedLength + 12);
#endif

                    ___sequence = unchecked(___sequence++);
                    ___timestamp = unchecked(___timestamp + (uint)(voiceToEncode.Length / 2));
                }

                timeToSend.Stop(); //stop after completely sending

                //Compensate for however long it took to sent.
                if (timeToSend.ElapsedMilliseconds > 0)
                {
                    long timeToWait = (msToSend * TimeSpan.TicksPerMillisecond) - (timeToSend.ElapsedMilliseconds * TimeSpan.TicksPerMillisecond);
                    if (timeToWait > 0) //if it's negative then don't bother waiting
                    {
#if V45
                        await Task.Delay(new TimeSpan(timeToWait)).ConfigureAwait(false);
#else
                        Thread.Sleep(new TimeSpan(timeToWait));
#endif
                    }
                }
                else
                {
#if V45
                    await Task.Delay(msToSend).ConfigureAwait(false);
#else
                    Thread.Sleep(msToSend);
#endif
                }

                VoiceDebugLogger.LogAsync("Sent " + dataSent + " bytes of Opus audio", MessageLevel.Unecessary);
            }
        }

        private Task DoWebSocketKeepAlive(CancellationToken token)
        {
#if V45
            return Task.Run(async () =>
#else
            return Task.Factory.StartNew(()=>
#endif
            {
                while(VoiceWebSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    if (VoiceWebSocket != null)
                    {
                        if (VoiceWebSocket.State == WebSocketState.Open)
                        {
                            string keepAliveJson = JsonConvert.SerializeObject(new
                            {
                                op = 3,
                                d = EpochTime.GetMilliseconds()
                            });
                            VoiceDebugLogger.Log("Sending voice keepalive ( " + keepAliveJson + " ) ", MessageLevel.Unecessary);
                            VoiceWebSocket.Send(keepAliveJson);
#if V45
                            await Task.Delay(Params.heartbeat_interval);
#else
                            Thread.Sleep(Params.heartbeat_interval);
#endif
                        }
                    }
                }
            });
        }

        private Task DoUDPKeepAlive(CancellationToken token)
        {
#if V45
            return Task.Run(async () =>
#else
            return Task.Factory.StartNew(() =>
#endif
            {
                byte[] keepAlive = new byte[5];
                keepAlive[0] = (byte)0xC9;
                try
                {
                    long seq = 0;
                    while (VoiceWebSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
                    {
                        if (token.IsCancellationRequested)
                            break;
                        keepAlive[1] = (byte)((___sequence >> 24) & 0xFF);
                        keepAlive[2] = (byte)((___sequence >> 16) & 0xFF);
                        keepAlive[3] = (byte)((___sequence >> 8) & 0xFF);
                        keepAlive[4] = (byte)((___sequence >> 0) & 0xFF);
#if V45
                        await _udp.SendAsync(keepAlive, keepAlive.Length).ConfigureAwait(false);
                        VoiceDebugLogger.Log("Sent UDP Keepalive.", MessageLevel.Unecessary);
                        await Task.Delay(5 * 1000); //5 seconds usually
#else
                        _udp.Send(keepAlive, keepAlive.Length);
                        VoiceDebugLogger.Log("Sent UDP keepalive.", MessageLevel.Unecessary);
                        Thread.Sleep(5 * 1000);
#endif
                    }
                }
                catch (ObjectDisposedException)
                {/*cancel token disposed*/}
                catch (NullReferenceException)
                {/*disposed*/}
                catch (Exception ex)
                {
                    VoiceDebugLogger.Log($"Error sending UDP keepalive\n\t{ex.Message}\n\t{ex.StackTrace}", MessageLevel.Error);
                }
            });
        }
        private async Task InitialUDPConnection()
        {
            try
            {
                _udp = new UdpClient(Params.port); //passes in proper port
                _udp.DontFragment = false;
                _udp.Connect(VoiceEndpoint.Replace(":80", ""), Params.port);

                VoiceDebugLogger.Log($"Initialized UDP Client at {VoiceEndpoint.Replace(":80", "")}:{Params.port}");

#if !V45
                udpEndpoint = new IPEndPoint(IPAddress.Parse(VoiceEndpoint.Replace(":80", "")), 80);
#endif

                byte[] packet = new byte[70]; //the initial packet
                packet[0] = (byte)((Params.ssrc >> 24) & 0xFF);
                packet[1] = (byte)((Params.ssrc >> 16) & 0xFF);
                packet[2] = (byte)((Params.ssrc >> 8) & 0xFF);
                packet[3] = (byte)((Params.ssrc >> 0) & 0xFF);

#if V45
                await _udp.SendAsync(packet, packet.Length).ConfigureAwait(false); //sends this initial packet.
                VoiceDebugLogger.Log("Sent ssrc packet.");

                UdpReceiveResult resultingMessage = await _udp.ReceiveAsync().ConfigureAwait(false); //receive a response packet
#else
                _udp.Send(packet, packet.Length);
                VoiceDebugLogger.Log("Sent ssrc packet.");

                byte[] resultingMessage = _udp.Receive(ref udpEndpoint);
#endif

#if V45
                if (resultingMessage != null && resultingMessage.Buffer.Length > 0)
#else
                if(resultingMessage != null && resultingMessage.Length > 0)
#endif
                {
                    VoiceDebugLogger.Log("Received IP packet, reading..");
#if V45
                    await SendIPOverUDP(GetIPAndPortFromPacket(resultingMessage.Buffer)).ConfigureAwait(false);
#else
                    var ipAndPort = GetIPAndPortFromPacket(resultingMessage);
                    udpEndpoint = new IPEndPoint(ipAndPort.Address, ipAndPort.port);
                    await SendIPOverUDP(ipAndPort).ConfigureAwait(false);
#endif
                }
                else
                    VoiceDebugLogger.Log("No IP packet received.", MessageLevel.Critical);
            }
            catch (Exception ex)
            {
                VoiceDebugLogger.Log("UDP Client Error: " + ex.Message, MessageLevel.Critical);
            }
        }
        /// <summary>
        /// Sends our IP over UDP for Discord's voice server to process. Also sends op 1
        /// </summary>
        /// <param name="buffer">The byte[] returned after sending your ssrc.</param>
        /// <returns></returns>
        private async Task SendIPOverUDP(DiscordIpPort ipPort)
        {
            string msg = JsonConvert.SerializeObject(new
            {
                op = 1,
                d = new
                {
                    protocol = "udp",
                    data = new
                    {
                        address = ipPort.Address.ToString(),
                        port = ipPort.port,
                        mode = "plain"
                    }
                }
            });
            VoiceDebugLogger.Log("Sending our IP over WebSocket ( " + msg.ToString() + " ) ");
            VoiceWebSocket.Send(msg);
            //await Task.Run(() => VoiceWebSocket.Send(msg)).ConfigureAwait(false); //idk lets try it
        }

        private DiscordIpPort GetIPAndPortFromPacket(byte[] packet)
        {
            DiscordIpPort returnVal = new DiscordIpPort();
            //quoth thy danny
            //#the ip is ascii starting at the 4th byte and ending at the first null
            int startingIPIndex = 4;
            int endingIPIndex = 4;
            for (int i = startingIPIndex; i < packet.Length; i++)
            {
                if (packet[i] != (byte)0)
                    endingIPIndex++;
                else
                    break;
            }

            byte[] ipArray = new byte[endingIPIndex - startingIPIndex];
            for (int i = 0; i < ipArray.Length; i++)
            {
                ipArray[i] = packet[i + startingIPIndex];
            }
            //quoth thy wise danny part two:
            //# the port is a little endian unsigned short in the last two bytes
            //# yes, this is different endianness from everything else
            int port = packet[packet.Length - 2] | packet[packet.Length - 1] << 8;

            returnVal.Address = IPAddress.Parse(System.Text.Encoding.ASCII.GetString(ipArray));
            returnVal.port = port;

            VoiceDebugLogger.Log($"Our IP is {returnVal.Address} and we're using port {returnVal.port}.");
            return returnVal;
        }
#endregion

#region Public Methods
        /// <summary>
        /// Enqueues audio to be sent through the UDP client.
        /// </summary>
        /// <param name="voice">PCM audio data.</param>
        public void SendVoice(byte[] voice)
        {
            voiceToSend.Enqueue((byte[])voice.Clone());
        }

        /// <summary>
        /// Clears the voice queue thus stopping the audio.
        /// </summary>
        public void ClearVoiceQueue()
        {
            byte[] ignored;
            while (voiceToSend.TryDequeue(out ignored)) ;
        }
        
        /// <summary>
        /// Echos a given DiscordAudioPacket.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public async Task EchoPacket(DiscordAudioPacket packet)
        {
            await SendPacket(DiscordAudioPacket.EchoPacket(packet.AsRawPacket(), Params.ssrc)).ConfigureAwait(false);
        }
        /// <summary>
        /// Sends a given DiscordAudioPacket over the UDP client..
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public async Task SendPacket(DiscordAudioPacket packet)
        {
            if (_udp != null && VoiceWebSocket.State == WebSocketState.Open)
            {
#if V45
                await _udp.SendAsync(packet.AsRawPacket(), packet.AsRawPacket().Length).ConfigureAwait(false);
#else
                _udp.Send(packet.AsRawPacket(), packet.AsRawPacket().Length);
#endif
                VoiceDebugLogger.Log("Sent packet through SendPacket task.", MessageLevel.Unecessary);
            }
        }
        /// <summary>
        /// Whether or not the current client is set to be speaking.
        /// </summary>
        /// <param name="speaking">If true, you will be set to speaking.</param>
        public void SetSpeaking(bool speaking)
        {
            if (VoiceWebSocket != null)
            {
                if (VoiceWebSocket.State == WebSocketState.Open)
                {
                    string speakingJson = JsonConvert.SerializeObject(new
                    {
                        op = 5,
                        d = new
                        {
                            speaking = speaking,
                            delay = 0
                        }
                    });
                    VoiceDebugLogger.Log("Sending voice speaking ( " + speakingJson + " ) ", MessageLevel.Unecessary);
                    VoiceWebSocket.Send(speakingJson);
                }
                else
                    VoiceDebugLogger.Log("VoiceWebSocket not alive?", MessageLevel.Critical);
            }
            else
                VoiceDebugLogger.Log("VoiceWebSocket null?", MessageLevel.Critical);
        }
#endregion

#region Cleanup
        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if(disposing)
            {
                handle.Dispose();
                Connected = false;
                if (VoiceWebSocket != null)
                {
                    VoiceWebSocket.Closed -= VoiceWebSocket_OnClose;
                    VoiceWebSocket.Error -= VoiceWebSocket_OnError;
                    VoiceWebSocket.Close();
                }
                VoiceWebSocket = null;
                globalTaskSource.Cancel(false);
                globalTaskSource.Dispose();
                if (_udp != null)
                    _udp.Close();
                _udp = null;
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); 
            
            if (Disposed != null)
                Disposed(this, new EventArgs());
        }
#endregion
    }
}
