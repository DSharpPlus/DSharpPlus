using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using WebSocketSharp;

namespace DiscordSharp
{
    internal class VoiceConnectionParameters
    {
        public string ssrc { get; internal set; }
        public int port { get; internal set; }
        public string[] modes { get; internal set; }
        public int heartbeat_interval { get; internal set; }
    }
    struct DiscordIpPort
    {
        public IPAddress Address;
        public int port;
    }
    class DiscordVoiceClient
    {
        WebSocket VoiceWebSocket;
        public string SessionID { get; internal set; }
        public string VoiceEndpoint { get; internal set; }
        public string Token { get; internal set; }
        public DiscordServer Guild { get; internal set; }
        public DiscordMember Me { get; internal set; }

        private UdpClient _udp = new UdpClient();
        private VoiceConnectionParameters Params { get; set; }

        public bool Connected { get; internal set; }

        public async Task Initiate()
        {
            VoiceWebSocket = new WebSocket("wss://" + VoiceEndpoint.Replace(":80", ""));
            VoiceWebSocket.OnClose += (sender, e) =>
            {
                if (e.WasClean)
                    return; //for now, till events are hooked up
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("VoiceWebSocket Closed: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"(Code: {e.Code}) {e.Reason}\n");
            };
            VoiceWebSocket.OnError += (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("VoiceWebSocket Error: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(e.Message + "\n");
            };
            VoiceWebSocket.OnMessage += async (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[Voice Debug]: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"({System.Text.Encoding.Default.GetString(e.RawData)})");

                JObject message = JObject.Parse(e.Data);
                if(message["op"].Value<int>() == 2)
                {
                    Params = new VoiceConnectionParameters();
                    Params.ssrc = message["d"]["ssrc"].ToString();
                    Params.port = message["d"]["port"].Value<int>();
                    JArray __modes = (JArray)message["d"]["modes"];
                    List<string> dynModes = new List<string>();
                    foreach (var mode in __modes)
                    {
                        dynModes.Add(mode.ToString());
                    }
                    Params.modes = dynModes.ToArray();
                    Params.heartbeat_interval = message["d"]["heartbeat_interval"].Value<int>();
                    await InitialConnection().ConfigureAwait(false);
                }
                else if(message["op"].Value<int>() == 4)
                {
                    Connected = true;
                    Console.WriteLine("--Voice connected!");
                }
                
            };
            VoiceWebSocket.OnOpen += (sender, e) =>
            {
                string initMsg = JsonConvert.SerializeObject(new
                {
                    op = 0,
                    d = new
                    {
                        user_id = Me.user.id,
                        server_id = Guild.id,
                        session_id = SessionID,
                        token = Token
                    }
                });

                VoiceWebSocket.Send(initMsg);
            };
            VoiceWebSocket.Connect();
        }

        public void Dispose()
        {
            VoiceWebSocket.Close();
            _udp.Close();
        }

        private async Task InitialConnection()
        {
            try
            {
                _udp = new UdpClient(Params.port);
                _udp.Connect(VoiceEndpoint.Replace(":80", ""), Params.port);

                int ssrcAsInt = int.Parse(Params.ssrc);
                byte[] packet = new byte[70];
                packet[0] = (byte)((ssrcAsInt >> 24) & 0xFF);
                packet[1] = (byte)((ssrcAsInt >> 16) & 0xFF);
                packet[2] = (byte)((ssrcAsInt >> 8) & 0xFF);
                packet[3] = (byte)((ssrcAsInt >> 0) & 0xFF);

                await _udp.SendAsync(packet, 70).ConfigureAwait(false);
                UdpReceiveResult resultingMessage = await _udp.ReceiveAsync().ConfigureAwait(false);
                foreach (byte b in resultingMessage.Buffer)
                    Console.Write($"{b} ");

                await SendOurIP(GetIPAndPortFromPacket(resultingMessage.Buffer)).ConfigureAwait(false);

                _udp.Close();
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[UDP Client Error]: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(ex.Message);
            }
        }

        private async Task SendOurIP(DiscordIpPort ipPortStruct)
        {
            string msg = JsonConvert.SerializeObject(new
            {
                op = 1,
                d = new
                {
                    protocol = "udp",
                    data = new
                    {
                        address = ipPortStruct.Address.ToString(),
                        port = ipPortStruct.port,
                        mode = "plain"
                    }
                }
            });
            await Task.Run(()=>
                VoiceWebSocket.SendAsync(msg, (__) => { })).ConfigureAwait(false);
        }

        private DiscordIpPort GetIPAndPortFromPacket(byte[] packet)
        {
            DiscordIpPort returnVal = new DiscordIpPort();
            //quoth thy danny
                //#the ip is ascii starting at the 4th byte and ending at the first null
            int startingIPIndex = 4;
            int endingIPIndex = 4;
            for(int i = startingIPIndex; i < packet.Length; i++)
            {
                if (packet[i] != (byte)0)
                    endingIPIndex++;
                else
                    break;
            }

            byte[] ipArray = new byte[endingIPIndex - startingIPIndex];
            for(int i = 0; i < ipArray.Length; i++)
            {
                ipArray[i] = packet[i + startingIPIndex];
            }
            //quoth thy wise danny part two:
            //# the port is a little endian unsigned short in the last two bytes
            //# yes, this is different endianness from everything else
            int port = packet[packet.Length - 2] | packet[packet.Length - 1] << 8;

            returnVal.Address = IPAddress.Parse(System.Text.Encoding.ASCII.GetString(ipArray));
            returnVal.port = port;

            return returnVal;
        }

    }
}
