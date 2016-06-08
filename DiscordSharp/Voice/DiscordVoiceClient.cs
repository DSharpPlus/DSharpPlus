extern alias ws4n;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using DiscordSharp.Voice;
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
using System.Collections.Concurrent;
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
        public byte[] OpusAudio { get; internal set; }
        public int OpusAudioLength { get; internal set; }
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
                return ((SampleRate / 1000) * 2 * Channels * FrameLengthMs);
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
#if NETFX4_5
        private ConcurrentQueue<byte[]> voiceToSend = new ConcurrentQueue<byte[]>();
#else
            private CONCURRENT.ConcurrentQueue<byte[]> voiceToSend = new CONCURRENT.ConcurrentQueue<byte[]>();
#endif
        private List<DiscordMember> MembersInChannel = new List<DiscordMember>();
        private Dictionary<DiscordMember, int> SsrcDictionary = new Dictionary<DiscordMember, int>();
        private string encryptionMode = "xsalsa20_poly1305";
        private byte[] __secretKey;
        private Thread VoiceSendThread, VoiceReceiveThread, UDPKeepAliveThread, WebsocketKeepAliveThread;

        private IPEndPoint udpEndpoint;

        public OpusDecoder Decoder { get; private set; }
        public DiscordVoiceConfig VoiceConfig { get; internal set; }

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

        internal void InitializeOpusEncoder()
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
            if(!VoiceConfig.SendOnly)
                InitializeOpusDecoder();
        }

        private void InitializeOpusDecoder()
        {
            if(Channel != null && Channel.Type == ChannelType.Voice)
            {
                Decoder = new OpusDecoder(VoiceConfig.SampleRate, VoiceConfig.Channels, VoiceConfig.FrameLengthMs);
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

            VoiceWebSocket.MessageReceived += (s, e) =>
            {
                try
                {
                    VoiceWebSocket_OnMessage(s, e);
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
                        server_id = Guild.ID,
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
        private void VoiceWebSocket_OnMessage(object sender, MessageReceivedEventArgs e)
        {
            JObject message = JObject.Parse(e.Message);
            switch (int.Parse(message["op"].ToString()))
            {
                case 2:
                    //VoiceDebugLogger.Log(e.Message);
                    OpCode2(message);
                    //ok, now that we have opcode 2 we have to send a packet and configure the UDP
                    InitialUDPConnection();
                    break;
                case 3:
                    VoiceDebugLogger.Log("KeepAlive echoed back successfully!", MessageLevel.Unecessary);
                    break;
                case 4: //you get your secret key from here
                    //post initializing the UDP client, we will receive opcode 4 and will now do the final connection steps
                    OpCode4(message);
                    //if (!VoiceConfig.SendOnly)
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
            e.UserSpeaking = Guild.GetMemberByKey(message["d"]["user_id"].ToString());
            e.Speaking = message["d"]["speaking"].ToObject<bool>();
            e.Ssrc = message["d"]["ssrc"].ToObject<int>();

            if(e.UserSpeaking != null)
            {
                if (!SsrcDictionary.ContainsKey(e.UserSpeaking))
                    SsrcDictionary.Add(e.UserSpeaking, e.Ssrc);
            }

            LastSpoken = e.UserSpeaking;

            if (UserSpeaking != null)
                UserSpeaking(this, e);
        }

        private void OpCode4(JObject message)
        {
            __secretKey = message["d"]["secret_key"].ToObject<byte[]>();
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
        private void OpCode2(JObject message)
        {
            Params = JsonConvert.DeserializeObject<VoiceConnectionParameters>(message["d"].ToString());
            SsrcDictionary.Add(Me, Params.ssrc);
            for (int i = 0; i < Params.modes.Length; i++)
            {
                if (!Params.modes[i].ToLower().Contains("plain"))
                {
                    encryptionMode = Params.modes[i];
                    break;
                }
            }
            //SendWebSocketKeepalive();
            DoWebSocketKeepAlive(globalTaskSource.Token);
        }
#pragma warning restore 4014

        private void VoiceWebSocket_OnError(object sender, EventArgs e)
        {
            VoiceDebugLogger.Log("Error in VoiceWebSocket.", MessageLevel.Critical);
            ErrorReceived?.Invoke(this, new EventArgs());

            //Won't worror about on error for now
        }

        private void VoiceWebSocket_OnClose(object sender, EventArgs e)
        {
            VoiceDebugLogger.Log($"VoiceWebSocket was closed.", MessageLevel.Critical);
            ErrorReceived?.Invoke(this, new EventArgs());
            
            Dispose();
        }
#endregion

        private bool QueueEmptyEventTriggered = false;
#region Internal Voice Methods
#pragma warning disable 4014
        private void SendVoiceTask(CancellationToken token)
        {
            VoiceSendThread =  new Thread(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (!voiceToSend.IsEmpty)
                    {
                        QueueEmptyEventTriggered = false;
                        try
                        {
                            SendVoiceAsync(token);
                        }
                        catch (Exception ex)
                        {
                            VoiceDebugLogger.Log(ex.Message, MessageLevel.Error);
                        }
                    }
                    else
                    {
#if NETFX4_5
                        //await Task.Delay(1000).ConfigureAwait(false);
                        Thread.Sleep(1000);
#else
                        Thread.Sleep(1000);
#endif
                    }
                    if (___sequence > 0 || ___timestamp > 0)
                    {
                        if (voiceToSend.IsEmpty)
                        {
                            //reset these
                            ___sequence = 0;
                            ___timestamp = 0;
                            if (!QueueEmptyEventTriggered)
                            {
                                QueueEmpty?.Invoke(this, new EventArgs());
                                QueueEmptyEventTriggered = true;
                            }
                        }
                    }
                }
            });
            VoiceSendThread.Start();
        }
#pragma warning disable 4014
        private Task ReceiveVoiceTask(CancellationToken token)
        {
            VoiceDebugLogger.Log("Setting up for voice receive.");
#if NETFX4_5
            return Task.Run(async () =>
#else
            return Task.Factory.StartNew(async () =>
#endif
            {
                while(!token.IsCancellationRequested)
                {
                    if (_udp.Available > 0)
                    {
                        try
                        {
                            await DoReceiveVoice().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            VoiceDebugLogger.Log($"Exception in receive loop: {ex.Message}\n\t{ex.StackTrace}", MessageLevel.Error);
                        }
                    }
                }
            }, token);
        }

        private async Task DoReceiveVoice()
        {
            _udp.DontFragment = false;
            if (_udp.Available > 0)
            {
                //the packet received, the 4000 size buffer for decoding, the nonce header for encryption and the decrypted/decoded result :)
                byte[] packet, decodingBuffer = null, nonce = null, result;
                //UdpReceiveResult receivedResult = await _udp.ReceiveAsync().ConfigureAwait(false);
                packet = _udp.Receive(ref udpEndpoint);
                int packetLength, resultOffset, resultLength;
                decodingBuffer = new byte[4000];
                nonce = new byte[24];
				packetLength = packet.Length;

                if (packet.Length > 0)
                {
                    if (packetLength < 12) return; //irrelevant packet
                    if (packet[0] != 0x80) return; //flags
                    if (packet[1] != 0x78) return; //payload type. you know, from before.

                    ushort sequenceNumber = (ushort)((packet[2] << 8) | packet[3] << 0);
                    uint timDocuestamp = (uint)((packet[4] << 24) | packet[5] << 16 | packet[6] << 8 | packet[7] << 0);
                    uint ssrc = (uint)((packet[8] << 24) | (packet[9] << 16) | (packet[10] << 8) | (packet[11] << 0));

                    //encryption is enabled by default
                    if (packetLength < 28) return; //irrelevant packet

                    Buffer.BlockCopy(packet, 0, nonce, 0, 12); //copy nonce
                    var length = Convert.ToUInt64(packetLength - 12);
                    int returnValue = SecretBox.Decrypt(packet, 12, length, decodingBuffer, nonce, __secretKey);
                    if (returnValue != 0)
                        return;
                    result = decodingBuffer;
                    resultOffset = 0; resultLength = packetLength - 28;

                    if (SsrcDictionary.ContainsValue((int)ssrc))
                    {
                        if (PacketReceived != null)
                        {
                            PacketReceived(this, new DiscordAudioPacketEventArgs
                            {
                                Channel = this.Channel,
                                FromUser = GetUserBySsrc(ssrc),
                                OpusAudio = result,
                                OpusAudioLength = resultLength
                            });
                        }
                    }

                }
            }
        }

        private DiscordMember GetUserBySsrc(uint ssrc)
        {
            foreach (var user in SsrcDictionary)
                if (user.Value == ssrc)
                    return user.Key;

            return null;
        }

#pragma warning restore 4014
        private void SendVoiceAsync(CancellationToken cancelToken)
        {
            byte[] opusAudio; //pcm data
            voiceToSend.TryDequeue(out opusAudio);
            if (opusAudio != null)
            {
                Stopwatch timeToSend = Stopwatch.StartNew();

                byte[] fullVoicePacket = new byte[4000 + 12 + 16];
                byte[] nonce = new byte[24];

                fullVoicePacket[0] = (byte)0x80; //flags
                fullVoicePacket[1] = (byte)0x78; //flags

                fullVoicePacket[8] = (byte)((Params.ssrc >> 24) & 0xFF); //ssrc
                fullVoicePacket[9] = (byte)((Params.ssrc >> 16) & 0xFF); //ssrc
                fullVoicePacket[10] = (byte)((Params.ssrc >> 8) & 0xFF); //ssrc
                fullVoicePacket[11] = (byte)((Params.ssrc >> 0) & 0xFF); //ssrc

                //byte[] opusAudio = new byte[queuedOpus.Length - 4];
                //Buffer.BlockCopy(queuedOpus, 0, opusAudio, 0, opusAudio.Length);
                //int encodedLength = mainOpusEncoder.EncodeFrame(voiceToEncode, 0, opusAudio);
                
                int encodedLength = BitConverter.ToInt32(opusAudio, opusAudio.Length - 4);

                int dataSent = 0;

                //actual sending
                {
                    ___sequence = unchecked(___sequence++);
                    //sequence big endian
                    fullVoicePacket[2] = (byte)((___sequence >> 8));
                    fullVoicePacket[3] = (byte)((___sequence >> 0) & 0xFF);

                    //timestamp big endian
                    fullVoicePacket[4] = (byte)((___timestamp >> 24) & 0xFF);
                    fullVoicePacket[5] = (byte)((___timestamp >> 16) & 0xFF);
                    fullVoicePacket[6] = (byte)((___timestamp >> 8));
                    fullVoicePacket[7] = (byte)((___timestamp >> 0) & 0xFF);

                    Buffer.BlockCopy(fullVoicePacket, 0, nonce, 0, 12); //copy header into nonce

                    //Buffer.BlockCopy(rtpPacket, 2, nonce, 2, 6); //copy 6 bytes for nonce
                    int returnVal = SecretBox.Encrypt(opusAudio, encodedLength, fullVoicePacket, 12, nonce, __secretKey);
                    if (returnVal != 0)
                        return;
                    if (opusAudio == null)
                        throw new ArgumentNullException("opusAudio");

                    int maxSize = encodedLength;
                    int rtpPacketLength = encodedLength + 12 + 16;

#if NETFX4_5
                    //dataSent = _udp.SendAsync(fullVoicePacket, encodedLength + 12 + 16).Result;
                    dataSent = _udp.Send(fullVoicePacket, encodedLength + 12 + 16);
#else
                    dataSent = _udp.Send(fullVoicePacket, rtpPacketLength);
#endif

                    ___timestamp = unchecked(___timestamp + (uint)((opusAudio.Length - 4) / 2));
                }

                timeToSend.Stop(); //stop after completely sending

                //Compensate for however long it took to sent.
                if (timeToSend.ElapsedMilliseconds > 0)
                {
                    //long timeToWait = (msToSend * TimeSpan.TicksPerMillisecond) - (timeToSend.ElapsedMilliseconds * TimeSpan.TicksPerMillisecond);
                    //long timeToWait = (long)(msToSend * TimeSpan.TicksPerMillisecond * 0.80) - (timeToSend.ElapsedMilliseconds * TimeSpan.TicksPerMillisecond);
                    if (timeToSend.ElapsedMilliseconds > 0) //if it's negative then don't bother waiting
                    {
                        //Thread.Sleep((int)(msToSend - timeToSend.ElapsedMilliseconds));
                        Thread.Sleep((int)Math.Floor(msToSend * 0.80));
                    }
                }
                else
                {
                    Thread.Sleep(msToSend);
                }

                VoiceDebugLogger.LogAsync("Sent " + dataSent + " bytes of Opus audio", MessageLevel.Unecessary);
            }
        }

        private void DoWebSocketKeepAlive(CancellationToken token)
        {
            WebsocketKeepAliveThread = new Thread(() => 
            {
                try
                {
                    while (VoiceWebSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
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
                                Thread.Sleep(Params.heartbeat_interval);
                            }
                        }
                    }
                }
                catch (NullReferenceException) { }
            });
            WebsocketKeepAliveThread.Start();
        }

        private void DoUDPKeepAlive(CancellationToken token)
        {

            UDPKeepAliveThread = new Thread(() =>
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
                        _udp.Send(keepAlive, keepAlive.Length);
                        VoiceDebugLogger.Log("Sent UDP keepalive.", MessageLevel.Unecessary);
                        Thread.Sleep(5 * 1000);
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
            UDPKeepAliveThread.Start();
        }
        private async Task InitialUDPConnection()
        {
            try
            {
                _udp = new UdpClient(Params.port); //passes in proper port
                //_udp.
                _udp.DontFragment = false;
                _udp.Connect(VoiceEndpoint.Replace(":80", ""), Params.port);

                VoiceDebugLogger.Log($"Initialized UDP Client at {VoiceEndpoint.Replace(":80", "")}:{Params.port}");

                udpEndpoint = new IPEndPoint(Dns.GetHostAddresses(VoiceEndpoint.Replace(":80", ""))[0], 80);


                byte[] packet = new byte[70]; //the initial packet
                packet[0] = (byte)((Params.ssrc >> 24) & 0xFF);
                packet[1] = (byte)((Params.ssrc >> 16) & 0xFF);
                packet[2] = (byte)((Params.ssrc >> 8) & 0xFF);
                packet[3] = (byte)((Params.ssrc >> 0) & 0xFF);

#if NETFX4_5
                await _udp.SendAsync(packet, packet.Length).ConfigureAwait(false); //sends this initial packet.
                VoiceDebugLogger.Log("Sent ssrc packet.");

                UdpReceiveResult resultingMessage = await _udp.ReceiveAsync().ConfigureAwait(false); //receive a response packet
                Console.WriteLine($"Receiving");
#else
                _udp.Send(packet, packet.Length, udpEndpoint);
                VoiceDebugLogger.Log("Sent ssrc packet.");

                byte[] resultingMessage = _udp.Receive(ref udpEndpoint);
#endif

#if NETFX4_5
                if (resultingMessage != null && resultingMessage.Buffer.Length > 0)
#else
                if(resultingMessage != null && resultingMessage.Length > 0)
#endif
                {
                    VoiceDebugLogger.Log("Received IP packet, reading..");
#if NETFX4_5
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
                        mode = encryptionMode
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
            Buffer.BlockCopy(packet, startingIPIndex, ipArray, 0, ipArray.Length);
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
            byte[] opusAudio = new byte[voice.Length + 4];
            int encodedLength = mainOpusEncoder.EncodeFrame(voice, 0, opusAudio);

            byte[] len = BitConverter.GetBytes(encodedLength);
            opusAudio[voice.Length] = len[0];
            opusAudio[voice.Length + 1] = len[1];
            opusAudio[voice.Length + 2] = len[2];
            opusAudio[voice.Length + 3] = len[3];

            voiceToSend.Enqueue(opusAudio);
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
#if NETFX4_5
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

                VoiceSendThread.Abort();
                UDPKeepAliveThread.Abort();
                WebsocketKeepAliveThread.Abort();

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
