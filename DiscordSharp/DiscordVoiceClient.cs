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
    public class DiscordVoiceClient
    {
        WebSocket VoiceWebSocket;
        public string SessionID { get; internal set; }
        public string VoiceEndpoint { get; internal set; }
        public string Token { get; internal set; }
        public DiscordServer Guild { get; internal set; }
        public DiscordMember Me { get; internal set; }

        private UdpClient _udp = new UdpClient();
        private VoiceConnectionParameters Params { get; set; }
        private Task keepAliveTask;
        private CancellationToken cancelToken;
        private Logger VoiceDebugLogger = new Logger();

        public event EventHandler<LoggerMessageReceivedArgs> DebugMessageReceived;

        public bool Connected { get; internal set; }

#pragma warning disable 1998
        public async Task Initiate()
        {
            VoiceDebugLogger.LogMessageReceived += (sender, e) =>
            {
                if (DebugMessageReceived != null)
                    DebugMessageReceived(this, e);
            };

            VoiceWebSocket = new WebSocket("wss://" + VoiceEndpoint.Replace(":80", ""));
            VoiceWebSocket.OnClose += (sender, e) =>
            {
                if (e.WasClean)
                    return; //for now, till events are hooked up
                VoiceDebugLogger.Log($"VoiceWebSocket Closed: (Code: {e.Code}) {e.Reason}", MessageLevel.Critical);
                Dispose();
            };
            VoiceWebSocket.OnError += (sender, e) =>
            {
                VoiceDebugLogger.Log($"VoiceWebSocket Error: {e.Message}", MessageLevel.Error);
            };
            VoiceWebSocket.OnMessage += async (sender, e) =>
            {
                VoiceDebugLogger.Log(e.Data);

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
                    string speakingJson = JsonConvert.SerializeObject(new { op = 5, d = new { speaking = true, delay = 0 } });
                    await Task.Run(()=>VoiceWebSocket.SendAsync(speakingJson, (__) => { })).ConfigureAwait(false);

                    cancelToken = new CancellationToken();
                    keepAliveTask = Task.Factory.StartNew(() => 
                    {
                        while(true)
                        {
                            Thread.Sleep(Params.heartbeat_interval);
                            cancelToken.ThrowIfCancellationRequested();
                            SendKeepAlive().ConfigureAwait(false);
                        }
                    }, cancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
                
            };
            VoiceWebSocket.OnOpen += (sender, e) =>
            {
                VoiceDebugLogger.Log("VoiceWebSocket opened, sending initial json.");
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

        public async void Dispose()
        {
            await Task.Run(()=>VoiceWebSocket.CloseAsync(CloseStatusCode.Normal));
            _udp.Close();
            VoiceDebugLogger.Dispose();
        }

        private async Task InitialConnection()
        {
            try
            {
                _udp = new UdpClient(Params.port);
                _udp.Connect(VoiceEndpoint.Replace(":80", ""), Params.port);

                VoiceDebugLogger.Log($"Initialized UDP Client at {VoiceEndpoint}");

                int ssrcAsInt = int.Parse(Params.ssrc);
                byte[] packet = new byte[70];
                packet[0] = (byte)((ssrcAsInt >> 24) & 0xFF);
                packet[1] = (byte)((ssrcAsInt >> 16) & 0xFF);
                packet[2] = (byte)((ssrcAsInt >> 8) & 0xFF);
                packet[3] = (byte)((ssrcAsInt >> 0) & 0xFF);

                await _udp.SendAsync(packet, 70).ConfigureAwait(false);

                VoiceDebugLogger.Log("Sent ssrc packet.");

                UdpReceiveResult resultingMessage = await _udp.ReceiveAsync().ConfigureAwait(false);

                VoiceDebugLogger.Log("Received IP packet, reading..");
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
            VoiceDebugLogger.Log("Sending our IP over WebSocket...");
            await Task.Run(()=>
                VoiceWebSocket.SendAsync(msg, (__) => { })).ConfigureAwait(false);
        }

        private static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private async Task SendKeepAlive()
        {
            if(VoiceWebSocket != null)
            {
                if(VoiceWebSocket.IsAlive)
                {
                    //int unixTime = (int)(DateTime.UtcNow - epoch).TotalMilliseconds;
                    Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    string keepAliveJson = JsonConvert.SerializeObject(new
                    {
                        op = 3,
                        d = (string)null
                    });
                    VoiceDebugLogger.Log("Sending voice keepalive. (" + keepAliveJson + ")");
                    //await Task.Run(()=>VoiceWebSocket.SendAsync(keepAliveJson, (__) => { }));
                    VoiceWebSocket.SendAsync(keepAliveJson, (__) => { });
                }
            }
        }

        public async Task<string> EncodeMp3(string pathToMp3)
        {
            //WaveOut waveOutDev = new WaveOut();
            Mp3FileReader m = new Mp3FileReader(pathToMp3);
            VoiceDebugLogger.Log($"Loading MP3 file {pathToMp3}");
            var outFormat = new WaveFormat(480000, 16, 2);
            using (var resampler = new MediaFoundationResampler(m, outFormat))
            {
                resampler.ResamplerQuality = 60;
                await Task.Run(()=>WaveFileWriter.CreateWaveFile("a.wav", m)).ConfigureAwait(false);
                return "a.wav";
            }
        }

        public async Task SendWav(string pathToWav)
        {
            using (var reader = new MediaFoundationReader(pathToWav))
            {
                byte[] buffer = new byte[reader.Length];
                await reader.ReadAsync(buffer, 0, buffer.Length);
                await SendPacket(GeneratePacket(buffer, 0));
            }
        }

        private byte[] GeneratePacket(byte[] voicesequence, int sequence)
        {
            byte[] returnVal = new byte[voicesequence.Length + 12]; //to accomodate the extra bytes
            using (MemoryStream ms = new MemoryStream(returnVal))
            {
                returnVal[0] = 0x80;
                returnVal[1] = 0x78;
                //sequence
                ms.Write(sequence.ToByteArray<int>(ByteOrder.Little), 2, 2);
                ms.Write(GetUnixTimestamp().ToByteArray<double>(ByteOrder.Little), 4, 4);
                ms.Write(int.Parse(Params.ssrc).ToByteArray<int>(ByteOrder.Little), 8, 4);

                ms.Write(voicesequence, 12, voicesequence.Length);

                return ms.ToArray();
            }
        }

        private double GetUnixTimestamp()
        {
            return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        private async Task BroadcastSpeaking()
        {
            string speakingJson = JsonConvert.SerializeObject(new { op = 5, d = new { speaking = true, delay = 0 } });
            await Task.Run(() => VoiceWebSocket.SendAsync(speakingJson, (__) => { })).ConfigureAwait(false); ;
        }

        public async Task SendPacket(byte[] packet)
        {
            if (Connected)
            {
                VoiceDebugLogger.Log($"Sending packet with size {packet.Length} to UDP endpoint..");
                int res = await _udp.SendAsync(packet, packet.Length);
                VoiceDebugLogger.Log("Broadcasting speaking");
                await BroadcastSpeaking();
                
                VoiceDebugLogger.Log($"res was {res}");
            }
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

            VoiceDebugLogger.Log($"Our IP is {returnVal.Address} and we're using port {returnVal.port}.");
            return returnVal;
        }
#pragma warning restore 1998
    }
}
