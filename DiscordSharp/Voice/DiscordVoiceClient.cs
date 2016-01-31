using DiscordSharp.Events;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
    
    public class DiscordVoiceClient
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

        private Task voiceSocketKeepAlive, udpReceiveTask, udpKeepAliveTask;
        private CancellationTokenSource voiceSocketTaskSource = new CancellationTokenSource();
        private CancellationTokenSource udpReceiveSource = new CancellationTokenSource();
        private CancellationTokenSource udpKeepAliveSource = new CancellationTokenSource();

        #region Events
        public event EventHandler<LoggerMessageReceivedArgs> DebugMessageReceived;
        public event EventHandler<EventArgs> Disposed;
        public event EventHandler<DiscordVoiceUserSpeakingEventArgs> UserSpeaking;
        public event EventHandler<DiscordAudioPacketEventArgs> PacketReceived;
        public event EventHandler<EventArgs> ErrorReceived;
        #endregion

        public DiscordVoiceClient(DiscordClient parentClient)
        {
            _parent = parentClient;
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
                await VoiceWebSocket_OnMessage(s, e).ConfigureAwait(false);
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
                    }, udpKeepAliveSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    udpReceiveTask = Task.Factory.StartNew(async () => 
                    {
                        try
                        {
                            while (true)
                            {
                                if (udpReceiveSource.IsCancellationRequested)
                                    udpReceiveSource.Token.ThrowIfCancellationRequested();
                                if (_udp.Available > 0)
                                {
                                    byte[] packet = new byte[1920];
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
                    }, udpReceiveSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
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
            await SendPacket(DiscordAudioPacket.EchoPacket(packet.AsRawPacket(), Params.ssrc));
        }

        private async Task DoUDPKeepAlive()
        {
            try
            {
                long seq = 0;
                while (VoiceWebSocket.State == WebSocketState.Open)
                {
                    if (udpKeepAliveSource.IsCancellationRequested)
                        udpKeepAliveSource.Token.ThrowIfCancellationRequested();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (BinaryWriter bw = new BinaryWriter(ms))
                        {
                            bw.Write(0xC9);
                            bw.Write((long)seq);

                            await _udp.SendAsync(ms.ToArray(), ms.ToArray().Length);
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
                await _udp.SendAsync(packet.AsRawPacket(), packet.AsRawPacket().Length);
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
            await Task.Run(()=>VoiceWebSocket.Send(msg)); //idk lets try it
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
                while (VoiceWebSocket != null && VoiceWebSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        SendWebSocketKeepalive();
                        if (voiceSocketTaskSource.Token.IsCancellationRequested)
                            voiceSocketTaskSource.Token.ThrowIfCancellationRequested();
                        Thread.Sleep(Params.heartbeat_interval);
                    }
                    catch(ObjectDisposedException)
                    {}
                    catch(OperationCanceledException ex)
                    {}
                }
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
                else
                {
                    VoiceDebugLogger.Log("VoiceWebSocket not alive?", MessageLevel.Critical);
                }
            }
            //more than likely this is due to something being disposed and is non-critical
            //else 
            //{
            //    VoiceDebugLogger.Log("VoiceWebSocket null?", MessageLevel.Critical);
            //}
        }

        private void SendSpeaking(bool speaking)
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
                    VoiceDebugLogger.Log("Sending voice speaking ( " + speakingJson + " ) ");
                    VoiceWebSocket.Send(speakingJson);
                }
                else
                    VoiceDebugLogger.Log("VoiceWebSocket not alive?", MessageLevel.Critical);
            }
            else
                VoiceDebugLogger.Log("VoiceWebSocket null?", MessageLevel.Critical);
        }

        public void Dispose()
        {
            VoiceWebSocket.Closed -= VoiceWebSocket_OnClose;
            VoiceWebSocket.Error -= VoiceWebSocket_OnError;
            VoiceWebSocket.Close();
            VoiceWebSocket = null;
            voiceSocketTaskSource.Cancel(); //cancels the task
            udpReceiveSource.Cancel();
            udpKeepAliveSource.Cancel();

            voiceSocketTaskSource.Dispose();
            udpReceiveSource.Dispose();
            udpKeepAliveSource.Cancel();
            _udp.Close();
            _udp = null;

            if (Disposed != null)
                Disposed(this, new EventArgs());
        }
    }
}
