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
using WebSocketSharp;

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
    public class DiscordVoiceClient
    {
        private DiscordClient _parent;

        public bool Connected { get; internal set; }
        public string SessionID { get; internal set; }
        public string VoiceEndpoint { get; internal set; }
        public string Token { get; internal set; }
        public DiscordServer Guild { get; internal set; }
        public DiscordMember Me { get; internal set; }

        private UdpClient _udp = new UdpClient();
        private VoiceConnectionParameters Params { get; set; }
        private Logger VoiceDebugLogger = new Logger();
        private WebSocket VoiceWebSocket;

        private Task voiceSocketKeepAlive;
        private CancellationTokenSource voiceSocketTaskSource = new CancellationTokenSource();

        #region Events
        public event EventHandler<LoggerMessageReceivedArgs> DebugMessageReceived;
        public event EventHandler<EventArgs> Disposed;
        #endregion

        public DiscordVoiceClient(DiscordClient parentClient)
        {
            _parent = parentClient;
        }

        public async Task Initiate()
        {
            VoiceDebugLogger.LogMessageReceived += (sender, e) =>
            {
                if (DebugMessageReceived != null)
                    DebugMessageReceived(this, e);
            };

            VoiceWebSocket = new WebSocket(VoiceEndpoint.StartsWith("wss://") ? VoiceEndpoint.Replace(":80", "") :
                "wss://" + VoiceEndpoint.Replace(":80", ""));
            VoiceWebSocket.OnClose += VoiceWebSocket_OnClose;
            VoiceWebSocket.OnError += VoiceWebSocket_OnError;

            VoiceWebSocket.OnMessage += async (s, e) =>
            {
                await VoiceWebSocket_OnMessage(s, e).ConfigureAwait(false);
            };
            VoiceWebSocket.OnOpen += (sender, e) =>
            {
                string initMsg = JsonConvert.SerializeObject(new
                {
                    op = 0,
                    d = new
                    {
                        user_id = Me.ID,
                        server_id = Guild.id,
                        session_id = SessionID,
                        token = Token
                    }
                });

                VoiceDebugLogger.Log("VoiceWebSocket opened, sending initial json. ( " + initMsg + ") ");

                VoiceWebSocket.Send(initMsg);
            };

            VoiceWebSocket.Connect();
        }

        private async Task VoiceWebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            VoiceDebugLogger.Log(e.Data);

            JObject message = JObject.Parse(e.Data);
            switch(message["op"].Value<int>())
            {
                case 2:
                    await OpCode2(message).ConfigureAwait(false); //do opcode 2 events
                    //ok, now that we have opcode 2 we have to send a packet and configure the UDP
                    await InitialUDPConnection().ConfigureAwait(false);
                    break;
                case 4:
                    //post initializing the UDP client, we will receive opcode 4 and will now do the final things
                    await OpCode4(message).ConfigureAwait(false);
                    break;
                case 5:
                    await OpCode5(message).ConfigureAwait(false);
                    break;
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

        private async Task OpCode5(JObject message)
        {
            //not yet! :)
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
            await SendWebSocketKeepalive().ConfigureAwait(false); //sends an initial keepalive right away.
            voiceSocketKeepAlive = Task.Run(async () =>
            {
                Thread.Sleep(Params.heartbeat_interval);
                Console.WriteLine(DateTime.Now + " kekekekeke");
                await SendWebSocketKeepalive().ConfigureAwait(false);
                if (voiceSocketTaskSource.Token.IsCancellationRequested)
                    voiceSocketTaskSource.Token.ThrowIfCancellationRequested();
            }, voiceSocketTaskSource.Token);
        }

        private void VoiceWebSocket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void VoiceWebSocket_OnClose(object sender, CloseEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private static DateTime Epoch = new DateTime(1970, 1, 1);
        /// <summary>
        /// Sends the WebSocket KeepAlive
        /// </summary>
        /// <returns></returns>
        private async Task SendWebSocketKeepalive()
        {
            if(VoiceWebSocket != null)
            {
                if(VoiceWebSocket.IsAlive)
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
                    VoiceDebugLogger.Log("VoiceWebSocket not alive?", MessageLevel.Critical);
            }
            else
                VoiceDebugLogger.Log("VoiceWebSocket null?", MessageLevel.Critical);
        }

        public async Task Dispose()
        {
            VoiceWebSocket.Close();
            voiceSocketTaskSource.Cancel(); //cancels the task

            if (Disposed != null)
                Disposed(this, new EventArgs());
        }
    }
}
