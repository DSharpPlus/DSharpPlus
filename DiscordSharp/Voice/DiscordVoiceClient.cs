using Discord.Audio.Opus;
using DiscordSharp.Audio;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using Microsoft.Win32.SafeHandles;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

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

    struct DiscordIpPort
    {
        public IPAddress Address;
        public int port;
    }

    public class DiscordAudioPacketEventArgs
    {
        public DiscordAudioPacket Packet { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordMember FromUser { get; internal set; }
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

        public Logger GetDebugLogger => VoiceDebugLogger;

        private Task voiceSocketKeepAlive, udpReceiveTask, udpKeepAliveTask, sendTask;
        private CancellationTokenSource voiceSocketTaskSource = new CancellationTokenSource();
        private CancellationTokenSource udpReceiveSource = new CancellationTokenSource();
        private CancellationTokenSource udpKeepAliveSource = new CancellationTokenSource();
        private CancellationTokenSource udpSendSource = new CancellationTokenSource();
        private ConcurrentQueue<byte[]> voiceToSend = new ConcurrentQueue<byte[]>();

        #region Events
        public event EventHandler<LoggerMessageReceivedArgs> DebugMessageReceived;
        public event EventHandler<EventArgs> Disposed;
        public event EventHandler<DiscordVoiceUserSpeakingEventArgs> UserSpeaking;
        public event EventHandler<DiscordAudioPacketEventArgs> PacketReceived;
        public event EventHandler<EventArgs> ErrorReceived;
        #endregion

        #region voice sending stuff
        //private ConcurrentQueue<byte[]> sendBuffer;
        //private ushort _sequence;
        #endregion

        public DiscordVoiceClient(DiscordClient parentClient)
        {
            _parent = parentClient;
            //sendBuffer = new ConcurrentQueue<byte[]>();
        }

        public void Initiate()
        {
            VoiceDebugLogger.LogMessageReceived += (sender, e) =>
            {
                if (DebugMessageReceived != null)
                    DebugMessageReceived(this, e);
            };

            VoiceWebSocket = new WebSocket(VoiceEndpoint.StartsWith("wss://") ? VoiceEndpoint.Replace(":80", "") :
                "wss://" + VoiceEndpoint.Replace(":80", ""));
            //VoiceWebSocket.Log.File = "VOICESOCKETLOG.txt";
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

        public void EnqueueVoice(byte[] voice)
        {
            //maybe ??????
            //while (voiceToSend.IsAddingCompleted == false) ;
            //voiceToSend.Add(voice);
        }

        public void FinishedQueing()
        {
            //voiceToSend.CompleteAdding();
        }

        public void SendVoice(byte[] voice)
        {
            voiceToSend.Enqueue(voice);
        }

        static int msToSend = 20;

        internal ushort ___sequence = 0;
        internal uint ___timestamp = 0;

        private async Task SendVoiceAsync(CancellationToken cancelToken)
        {
            byte[] voiceToEncode;
            voiceToSend.TryDequeue(out voiceToEncode);
            if (voiceToEncode != null)
            {
                Stopwatch timeToSend = Stopwatch.StartNew();

                byte[] opusAudio = new byte[voiceToEncode.Length];
                int encodedLength = mainOpusEncoder.EncodeFrame(voiceToEncode, 0, opusAudio);

                byte[] udpHeader = new byte[12];
                udpHeader[0] = 0x80;
                udpHeader[1] = 0x78;

                //big endian
                udpHeader[8] = (byte)((Params.ssrc >> 24) & 0xFF);
                udpHeader[9] = (byte)((Params.ssrc >> 16) & 0xFF);
                udpHeader[10] = (byte)((Params.ssrc >> 8) & 0xFF);
                udpHeader[11] = (byte)((Params.ssrc >> 0) & 0xFF);
                int dataSent = 0;

                //actual sending
                {
                    //sequence big endian
                    udpHeader[2] = (byte)((___sequence >> 8));
                    udpHeader[3] = (byte)((___sequence >> 0) & 0xFF);

                    //timestamp big endian
                    udpHeader[4] = (byte)((___timestamp >> 24) & 0xFF);
                    udpHeader[5] = (byte)((___timestamp >> 16) & 0xFF);
                    udpHeader[6] = (byte)((___timestamp >> 8));
                    udpHeader[7] = (byte)((___timestamp >> 0) & 0xFF);

                    if (opusAudio == null)
                        throw new ArgumentNullException("opusAudio");

                    int maxSize = encodedLength;
                    byte[] buffer = new byte[udpHeader.Length + maxSize]; //make big thing
                    System.Buffer.BlockCopy(udpHeader, 0, buffer, 0, udpHeader.Length);
                    System.Buffer.BlockCopy(opusAudio, 0, buffer, udpHeader.Length /*12*/, encodedLength);

                    dataSent = await _udp.SendAsync(buffer, buffer.Length);

                    ___sequence = unchecked(___sequence++);
                    ___timestamp = unchecked(___timestamp + (UInt32)(voiceToEncode.Length / 2));
                }

                timeToSend.Stop(); //stop after completely sending

                //Compensate for however long it took to sent.
                if (timeToSend.ElapsedMilliseconds > 0)
                {
                    long timeToWait = msToSend - timeToSend.ElapsedMilliseconds;
                    if (!(timeToWait < 0)) //if it's negative then don't bother waiting
                        await Task.Delay((int)timeToWait);
                }
                else
                    await Task.Delay(msToSend);

                VoiceDebugLogger.Log("Sent " + dataSent + " bytes of Opus audio", MessageLevel.Unecessary);
            }
        }

        private async Task VoiceWebSocket_OnMessage(object sender, MessageReceivedEventArgs e)
        {
            JObject message = JObject.Parse(e.Message);
            switch(message["op"].Value<int>())
            {
                case 2:
                    VoiceDebugLogger.Log(e.Message);
                    await OpCode2(message).ConfigureAwait(false); //do opcode 2 events
                    //ok, now that we have opcode 2 we have to send a packet and configure the UDP
                    await InitialUDPConnection().ConfigureAwait(false);
                    break;
                case 3:
                    VoiceDebugLogger.Log("KeepAlive echoed back successfully!", MessageLevel.Unecessary);
                    break;
                case 4:
                    VoiceDebugLogger.Log(e.Message);
                    //post initializing the UDP client, we will receive opcode 4 and will now do the final things
                    await OpCode4(message).ConfigureAwait(false);
                    udpKeepAliveTask = Task.Factory.StartNew(async () =>
                    {
                        await DoUDPKeepAlive().ConfigureAwait(false);
                    }, udpKeepAliveSource.Token, TaskCreationOptions.None, TaskScheduler.Default);
                    udpReceiveTask = Task.Factory.StartNew(async () => 
                    {
                        try
                        {
                            while (!udpReceiveSource.IsCancellationRequested)
                            {
                                if (udpReceiveSource.IsCancellationRequested)
                                    udpReceiveSource.Token.ThrowIfCancellationRequested();
                                if (_udp.Available > 0)
                                {
                                    byte[] packet = new byte[_udp.Available];
                                    VoiceDebugLogger.Log("Received packet!! Length: " + _udp.Available, MessageLevel.Unecessary);
                                    UdpReceiveResult d = await _udp.ReceiveAsync().ConfigureAwait(false);
                                    packet = d.Buffer;

                                    DiscordAudioPacketEventArgs ae = new DiscordAudioPacketEventArgs();
                                    ae.FromUser = LastSpoken;
                                    ae.Channel = Channel;
                                    ae.Packet = new DiscordAudioPacket(packet);

                                    if (PacketReceived != null)
                                        PacketReceived(this, ae);

                                    //VoiceDebugLogger.Log("Echoing back..");
                                    //DiscordAudioPacket echo = DiscordAudioPacket.EchoPacket(packet, Params.ssrc);
                                    //await _udp.SendAsync(echo.AsRawPacket(), echo.AsRawPacket().Length).ConfigureAwait(false);
                                    //VoiceDebugLogger.Log("Sent!");
                                }
                            }
                        }
                        catch (ObjectDisposedException)
                        { }
                        catch(OperationCanceledException)
                        { }
                        catch (Exception ex)
                        {
                            VoiceDebugLogger.Log($"Error in udpReceiveTask\n\t{ex.Message}\n\t{ex.StackTrace}",
                                MessageLevel.Critical);
                        }
                    }, udpReceiveSource.Token, TaskCreationOptions.None, TaskScheduler.Default);
                    sendTask = Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            while(!udpSendSource.IsCancellationRequested)
                            {
                                if (udpSendSource.IsCancellationRequested)
                                    udpSendSource.Token.ThrowIfCancellationRequested();
                                await SendVoiceAsync(udpSendSource.Token);
                                if (voiceToSend.IsEmpty)
                                {
                                    //reset sequence and timestamp so the client doesn't break
                                    ___sequence = 0;
                                    ___timestamp = 0;
                                }
                            }
                        }
                        catch (ObjectDisposedException) { }
                        catch (OperationCanceledException) { }
                        catch(Exception ex)
                        {
                            VoiceDebugLogger.Log($"Error in sendTask\n\t{ex.Message}\n\t{ex.StackTrace}",
                                MessageLevel.Critical);
                        }
                    });
                    SendSpeaking(true);
                    break;
                case 5:
                    VoiceDebugLogger.Log(e.Message);
                    OpCode5(message);
                    break;
            }
        }

        public async Task EchoPacket(DiscordAudioPacket packet)
        {
            await SendPacket(DiscordAudioPacket.EchoPacket(packet.AsRawPacket(), Params.ssrc)).ConfigureAwait(false);
        }

        private async Task DoUDPKeepAlive()
        {
            try
            {
                long seq = 0;
                while (VoiceWebSocket.State == WebSocketState.Open && !udpKeepAliveSource.Token.IsCancellationRequested)
                {
                    if (udpKeepAliveSource.IsCancellationRequested)
                        udpKeepAliveSource.Token.ThrowIfCancellationRequested();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (BinaryWriter bw = new BinaryWriter(ms))
                        {
                            bw.Write(0xC9);
                            bw.Write((long)seq);

                            await _udp.SendAsync(ms.ToArray(), ms.ToArray().Length).ConfigureAwait(false);
                            VoiceDebugLogger.Log("Sent UDP Keepalive");
                            Thread.Sleep(5 * 1000); //5 seconds
                        }
                    }
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
        }

        public async Task SendPacket(DiscordAudioPacket packet)
        {
            if(_udp != null && VoiceWebSocket.State == WebSocketState.Open)
            {
                await _udp.SendAsync(packet.AsRawPacket(), packet.AsRawPacket().Length).ConfigureAwait(false);
                VoiceDebugLogger.Log("Sent packet through SendPacket task.", MessageLevel.Unecessary);
            }
        }

        private async Task InitialUDPConnection()
        {
            try
            {
                _udp = new UdpClient(Params.port); //passes in proper port
                _udp.Connect(VoiceEndpoint.Replace(":80", ""), Params.port);

                VoiceDebugLogger.Log($"Initialized UDP Client at {VoiceEndpoint.Replace(":80", "")}:{Params.port}");

                byte[] packet = new byte[70]; //the initial packet
                packet[0] = (byte)((Params.ssrc >> 24) & 0xFF);
                packet[1] = (byte)((Params.ssrc >> 16) & 0xFF);
                packet[2] = (byte)((Params.ssrc >> 8) & 0xFF);
                packet[3] = (byte)((Params.ssrc >> 0) & 0xFF);

                await _udp.SendAsync(packet, packet.Length).ConfigureAwait(false); //sends this initial packet.
                VoiceDebugLogger.Log("Sent ssrc packet.");

                UdpReceiveResult resultingMessage = await _udp.ReceiveAsync().ConfigureAwait(false); //receive a response packet

                if (resultingMessage != null || resultingMessage.Buffer.Length > 0)
                {
                    VoiceDebugLogger.Log("Received IP packet, reading..");
                    await SendIPOverUDP(GetIPAndPortFromPacket(resultingMessage.Buffer)).ConfigureAwait(false);
                }
                else
                    VoiceDebugLogger.Log("No IP packet received.", MessageLevel.Critical);
            }
            catch(Exception ex)
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
            await Task.Run(()=>VoiceWebSocket.Send(msg)).ConfigureAwait(false); //idk lets try it
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

        private void OpCode5(JObject message)
        {
            DiscordVoiceUserSpeakingEventArgs e = new DiscordVoiceUserSpeakingEventArgs();
            e.Channel = Channel;
            e.UserSpeaking = Guild.members.Find(x => x.ID == message["d"]["user_id"].ToString());
            e.Speaking = message["d"]["speaking"].ToObject<bool>();
            e.ssrc = message["d"]["ssrc"].ToObject<int>();

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

        private async Task OpCode2(JObject message)
        {
            Params = JsonConvert.DeserializeObject<VoiceConnectionParameters>(message["d"].ToString());
            //await SendWebSocketKeepalive().ConfigureAwait(false); //sends an initial keepalive right away.
            SendWebSocketKeepalive();
            voiceSocketKeepAlive = Task.Run(() =>
            {
                ///TODO: clean this up
                try
                {
                    while (VoiceWebSocket != null && VoiceWebSocket.State == WebSocketState.Open && !voiceSocketTaskSource.Token.IsCancellationRequested)
                    {
                        SendWebSocketKeepalive();
                        if (voiceSocketTaskSource.Token.IsCancellationRequested)
                            voiceSocketTaskSource.Token.ThrowIfCancellationRequested();
                        Thread.Sleep(Params.heartbeat_interval);
                    }
                }
                catch (ObjectDisposedException)
                { }
                catch (OperationCanceledException ex)
                { }
            }, voiceSocketTaskSource.Token);
        }

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

        private static DateTime Epoch = new DateTime(1970, 1, 1);
        /// <summary>
        /// Sends the WebSocket KeepAlive
        /// </summary>
        /// <returns></returns>
        private void SendWebSocketKeepalive()
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
                    VoiceDebugLogger.Log("Sending voice keepalive ( " + keepAliveJson + " ) ");
                    VoiceWebSocket.Send(keepAliveJson);
                }
            }
        }

        [Obsolete]
        public async Task SendLargeOpusAudioPacket(byte[] opusAudio, int rate, int size)
        {
            UInt16 sequence = 0;
            UInt32 timestamp = 0;
            byte[] udpHeader = new byte[12];
            udpHeader[0] = 0x80;
            udpHeader[1] = 0x78;

            //big endian
            //sizeof(int) = 4;
            byte[] intTobytes = DiscordAudioPacket.IntToBytes(Params.ssrc);
            udpHeader[8] = (byte)((Params.ssrc >> 24) & 0xFF);
            udpHeader[9] = (byte)((Params.ssrc >> 16) & 0xFF);
            udpHeader[10] = (byte)((Params.ssrc >> 8) & 0xFF);
            udpHeader[11] = (byte)((Params.ssrc >> 0) & 0xFF);
            
            int loopMax = (DateTime.Now.Millisecond * (size / (rate / 100)));
            for(int i = 0; i < loopMax; i++)
            {
                //sequence big endian
                udpHeader[2] = (byte)((sequence >> 8) & 0xFF);
                udpHeader[3] = (byte)((sequence >> 0) & 0xFF);

                //timestamp big endian
                udpHeader[4] = (byte)((sequence >> 24) & 0xFF);
                udpHeader[5] = (byte)((sequence >> 16) & 0xFF);
                udpHeader[6] = (byte)((sequence >> 8) & 0xFF);
                udpHeader[7] = (byte)((sequence >> 0) & 0xFF);

                if (opusAudio == null)
                    return;
                byte[] buffer = new byte[udpHeader.Length + opusAudio.Length]; //make big thing
                System.Buffer.BlockCopy(udpHeader, 0, buffer, 0, udpHeader.Length);
                System.Buffer.BlockCopy(opusAudio, 0, buffer, udpHeader.Length, opusAudio.Length);
                
                int x = await _udp.SendAsync(buffer, buffer.Length).ConfigureAwait(false);
                
                if (sequence == 0xFFFF)
                    sequence = 0;
                else
                    sequence++;

                if (timestamp + (UInt32)size >= 0xFFFFFF)
                    timestamp = 0;
                else
                    timestamp += (UInt32)size;
            }
            SendSpeaking(false);
        }

        private OpusEncoder mainOpusEncoder;

        /// <summary>
        /// Initializes the Opus encoder for encoding frames.
        /// Please run this again if you want to change the frame length, bitrate, or channels.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="channels"></param>
        /// <param name="frameLengthMs"></param>
        /// <param name="bitRate">If null, recommended values will be used.</param>
        public void InitializeOpusEncoder(int rate, int channels, int frameLengthMs, int? bitRate)
        {
            mainOpusEncoder = new OpusEncoder(rate, channels, frameLengthMs, bitRate, OpusApplication.MusicOrMixed);
            mainOpusEncoder.SetForwardErrorCorrection(true);
        }
        
        /// <summary>
        /// Sends a single PCM Audio packet re-encoded as opus.
        /// </summary>
        /// <param name="pcmAudio">The byte[] containing enough bytes to encode.</param>
        /// <param name="rate">The rate in kHz of the audio.</param>
        /// <param name="size">The size you're sending.</param>
        /// <param name="timestampSeq">The TimestampSequenceReturn struct that this function returns. If it's your first time using it, you can create a new one with 0 for all values.</param>
        /// <returns>A TimestampSequenceReturn with the updated sequence and timestamp values.</returns>
        [Obsolete]
        public TimestampSequenceReturn SendSmallOpusAudioPacket(byte[] pcmAudio, int rate, int size, TimestampSequenceReturn timestampSeq) //async Task<TimestampSequenceReturn>
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (mainOpusEncoder == null)
                throw new NullReferenceException("Please initialize Opus Encoder first.");

            byte[] opusAudio = new byte[pcmAudio.Length];
            int encodedLength = mainOpusEncoder.EncodeFrame(pcmAudio, 0, opusAudio);

            byte[] udpHeader = new byte[12];
            udpHeader[0] = 0x80;
            udpHeader[1] = 0x78;

            //big endian
            udpHeader[8] = (byte)((Params.ssrc >> 24) & 0xFF);
            udpHeader[9] = (byte)((Params.ssrc >> 16) & 0xFF);
            udpHeader[10] = (byte)((Params.ssrc >> 8) & 0xFF);
            udpHeader[11] = (byte)((Params.ssrc >> 0) & 0xFF);
            
            int dataSent = 0;
            {
                //sequence big endian
                udpHeader[2] = (byte)((timestampSeq.sequence >> 8));
                udpHeader[3] = (byte)((timestampSeq.sequence >> 0) & 0xFF);

                //timestamp big endian
                udpHeader[4] = (byte)((timestampSeq.timestamp >> 24) & 0xFF);
                udpHeader[5] = (byte)((timestampSeq.timestamp >> 16) & 0xFF);
                udpHeader[6] = (byte)((timestampSeq.timestamp >> 8));
                udpHeader[7] = (byte)((timestampSeq.timestamp >> 0) & 0xFF);

                if (opusAudio == null)
                    throw new ArgumentNullException("opusAudio");

                int maxSize = encodedLength; //0
                //for (int i = 0; i < opusAudio.Length; i++)
                //{
                //    if (opusAudio[i] != '\0')
                //        maxSize++;
                //}
                byte[] buffer = new byte[udpHeader.Length + maxSize]; //make big thing
                System.Buffer.BlockCopy(udpHeader, 0, buffer, 0, udpHeader.Length);
                System.Buffer.BlockCopy(opusAudio, 0, buffer, udpHeader.Length /*12*/, encodedLength);

                _udp.Send(buffer, buffer.Length);

                timestampSeq.sequence = unchecked(timestampSeq.sequence++);
                timestampSeq.timestamp = unchecked(timestampSeq.timestamp + (UInt32)(pcmAudio.Length / 2));
            }
            stopwatch.Stop();
            timestampSeq.SentBytes = dataSent;
            timestampSeq.MsTookToEncode = stopwatch.Elapsed.Milliseconds;
            return timestampSeq;
        }

        public void SendSpeaking(bool speaking)
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
                voiceSocketTaskSource.Cancel(); //cancels the task
                udpReceiveSource.Cancel();
                udpKeepAliveSource.Cancel();
                udpSendSource.Cancel();

                sendTask.Dispose();
                udpReceiveTask.Dispose();
                voiceSocketKeepAlive.Dispose();
                udpKeepAliveTask.Dispose();

                voiceSocketTaskSource.Dispose();
                udpReceiveSource.Dispose();
                udpKeepAliveSource.Dispose();
                udpSendSource.Dispose();
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
    }
    public struct TimestampSequenceReturn
    {
        public UInt16 sequence;
        public UInt32 timestamp;
        public int SentBytes;
        public int MsTookToEncode;
    }
}
