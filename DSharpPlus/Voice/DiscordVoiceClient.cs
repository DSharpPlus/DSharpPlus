using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sodium;

namespace DSharpPlus.Voice
{
    internal class VoiceConstants
    {
        internal const int RTP_HEADER_LENGTH    = 12;
        internal const int SEQUENCE_INDEX       = 2;
        internal const int TIMESTAMP_INDEX      = 4;
        internal const int SSRC_INDEX           = 8;
    }

    public class DiscordVoiceClient : IDisposable
    {
        public event EventHandler<UserSpeakingEventArgs> UserSpeaking;
        public event EventHandler<VoiceReceivedEventArgs> VoiceReceived;

        internal static List<byte> _sendBuffer;
        internal static List<byte> _receiveBuffer;

        internal static OpusDecoder _opusDecoder;
        internal static OpusEncoder _opusEncoder;

        internal static ulong _guildId;
        internal static string _endpoint;

        internal static WebSocketClient _websocketClient;
        internal static int _heartbeatInterval;
        internal Thread _heartbeatThread;
        internal static DateTime _lastHeartbeat;

        internal static UdpClient __udpClient;
        internal static ushort __sequence = 0;
        internal static uint __ssrc;
        internal static string __ip = "";
        internal static int __port;

        internal static string __localIp = "";
        internal static int __localPort;

        internal static byte[] __secretKey;
        internal static string __mode;

        internal DiscordVoiceClient() { }

        internal async Task Init(string token, ulong guild_id, string endpoint)
        {
            if (DiscordClient.config.VoiceSettings == VoiceSettings.None)
                throw new NotSupportedException(nameof(DiscordClient.config.VoiceSettings));
            
            if (DiscordClient.config.VoiceSettings == VoiceSettings.Both || DiscordClient.config.VoiceSettings == VoiceSettings.Receiving)
                _opusDecoder = OpusDecoder.Create(48000, 2);
            if (DiscordClient.config.VoiceSettings == VoiceSettings.Both || DiscordClient.config.VoiceSettings == VoiceSettings.Sending)
                _opusEncoder = OpusEncoder.Create(48000, 2, DiscordClient.config.VoiceApplication);

            DiscordClient._sessionToken = token;
            _guildId = guild_id;
            _endpoint = endpoint.Replace(":80", "");
            
            await Connect();
        }

        internal async Task Connect()
        {
            await Task.Run(() =>
            {
                _websocketClient = new WebSocketClient($"wss://{_endpoint}");
                _websocketClient.SocketOpened += async (sender, e) =>
                {
                    await SendIndentifyPacket();
                };
                _websocketClient.SocketClosed += async (sender, e) =>
                {
                    await Task.Run(() =>
                    {
                        DiscordClient._debugLogger.LogMessage((e.WasClean ? LogLevel.Debug : LogLevel.Critical), "Voice-Websocket", $"Connection closed [WasClean: {e.WasClean}]", DateTime.Now);
                    });
                };
                _websocketClient.SocketMessage += async (sender, e) => await HandleSocketMessage(e.Data);
                _websocketClient.Connect();
            });
        }

        private async Task HandleSocketMessage(string data)
        {
            JObject obj = JObject.Parse(data);
            switch(obj.Value<int>("op"))
            {
                case 2: await OnReadyEvent(obj); break;
                case 3: await OnHeartbeatAckEvent(obj); break;
                case 4: await OnSessionDescriptionEvent(obj); break;
                case 5: await OnUserSpeakingEvent(obj); break;
                default:
                    {
                        await Task.Run(() =>
                        {
                            DiscordClient._debugLogger.LogMessage(LogLevel.Warning, "Voice-Websocket", $"Unknown OP-Code: {obj.Value<int>("op")}\n{obj}", DateTime.Now);
                        });
                        break;
                    }
            }
        }

        #region Events
        internal async Task OnReadyEvent(JObject obj)
        {
            await Task.Run(() =>
            {
                DiscordClient._debugLogger.LogMessage(LogLevel.Debug, "Voice-Websocket", "Received OP 2, Connecting UDP", DateTime.UtcNow);
                __ssrc = obj["d"]["ssrc"].ToObject<uint>();
                __port = obj["d"]["port"].ToObject<int>();
                __ip = obj["d"]["ip"].ToString();

                _heartbeatInterval = obj["d"]["heartbeat_interval"].ToObject<int>();
                _heartbeatThread = new Thread(async () => { await StartHeartbeating(); });
                _heartbeatThread.Start();

                __udpClient = new UdpClient();
                __udpClient.Connect(_endpoint, __port);
                DiscordClient._debugLogger.LogMessage(LogLevel.Debug, "Voice-UDP", "Connected UDP.", DateTime.UtcNow);
            });


            await IPDiscovery();
            await SendSelectProtocol();
            if (DiscordClient.config.VoiceSettings == VoiceSettings.Receiving || DiscordClient.config.VoiceSettings == VoiceSettings.Both)
                __udpClient.BeginReceive(ReceiveAudio, null);
        }
        internal async Task OnHeartbeatAckEvent(JObject obj)
        {
            await Task.Run(() =>
            {
                _heartbeatInterval = obj["d"].ToObject<int>();

                DiscordClient._debugLogger.LogMessage(LogLevel.Unnecessary, "Voice-Websocket", "Received Heartbeat Ack", DateTime.Now);
                DiscordClient._debugLogger.LogMessage(LogLevel.Debug, "Voice-Websocket", $"Ping {(DateTime.Now - _lastHeartbeat).Milliseconds}ms", DateTime.Now);
            });
        }
        internal async Task OnSessionDescriptionEvent(JObject obj)
        {
            await Task.Run(() =>
            {
                DiscordClient._debugLogger.LogMessage(LogLevel.Debug, "Voice-Websocket", "Received OP 4, got secret key! Sending OP 5 for speaking", DateTime.UtcNow);
                __secretKey = obj["d"]["secret_key"].ToObject<byte[]>();
                __mode = obj["d"]["mode"].ToString();

                JObject j = new JObject
                {
                    { "op", 5 },
                    { "d", new JObject
                        {
                            { "speaking", true },
                            { "delay", 0 }
                        }
                    }
                };
                DiscordClient._debugLogger.LogMessage(LogLevel.Debug, "Voice-Websocket", "Sent OP 5. We should be live now.", DateTime.UtcNow);
                _websocketClient._socket.Send(j.ToString());
            });
        }
        internal async Task OnUserSpeakingEvent(JObject obj)
        {
            await Task.Run(() =>
            {
                if (!DiscordClient._ssrcDict.ContainsKey(obj["d"]["ssrc"].ToObject<uint>()))
                    DiscordClient._ssrcDict.Add(obj["d"]["ssrc"].ToObject<uint>(), obj["d"]["user_id"].ToObject<ulong>());

                UserSpeaking?.Invoke(this, new UserSpeakingEventArgs
                {
                    Speaking = obj["d"]["speaking"].ToObject<bool>(),
                    ssrc = obj["d"]["ssrc"].ToObject<uint>(),
                    UserID = obj["d"]["user_id"].ToObject<ulong>()
                });
            });
        }
        #endregion

        internal async Task StartHeartbeating()
        {
            DiscordClient._debugLogger.LogMessage(LogLevel.Unnecessary, "Voice-Wbsocket", "Starting Heartbeat", DateTime.Now);
            while (!DiscordClient._cancelToken.IsCancellationRequested)
            {
                await SendHeartbeat();
                await Task.Delay(_heartbeatInterval);
            }
        }

        internal async Task SendHeartbeat()
        {
            await Task.Run(() =>
            {
                DiscordClient._debugLogger.LogMessage(LogLevel.Unnecessary, "Voice-Websocket", "Sending Heartbeat", DateTime.Now);
                JObject obj = new JObject
                {
                    { "op", 3 },
                    { "d", _heartbeatInterval }
                };
                _websocketClient._socket.Send(obj.ToString());
                _lastHeartbeat = DateTime.Now;
            });
        }

        internal async Task SendIndentifyPacket()
        {
            await Task.Run(() =>
            {
                JObject obj = new JObject
                {
                    { "op", 0 },
                    { "d", new JObject
                        {
                            { "server_id", _guildId },
                            { "user_id", DiscordClient._me.ID },
                            { "session_id", DiscordClient._sessionID },
                            { "token", DiscordClient._sessionToken }
                        }
                    }
                };
                _websocketClient._socket.Send(obj.ToString());
            });
        }

        internal async Task IPDiscovery()
        {
            ByteBuffer buffer = new ByteBuffer(70);
            buffer.WriteUIntToBuffer(__ssrc, 66);

            await __udpClient.SendAsync(buffer.GetBuffer(), buffer.GetBuffer().Length);
            buffer = new ByteBuffer((await __udpClient.ReceiveAsync()).Buffer, !BitConverter.IsLittleEndian);

            __localIp = buffer.ReadStringFromBuffer(0, 68).Trim('\0');
            __localPort = buffer.ReadUShortFromBuffer(68);
        }

        internal async Task SendSelectProtocol()
        {
            await Task.Run(() =>
            {
                JObject obj = new JObject
                {
                    { "op", 1 },
                    { "d", new JObject
                        {
                            {"protocol", "udp" },
                            {"data", new JObject
                                {
                                    { "address", __localIp },
                                    { "port", __localPort },
                                    { "mode", "xsalsa20_poly1305" }
                                }
                            }
                        }
                    }
                };
                _websocketClient._socket.Send(obj.ToString());
            });
        }

        private void ReceiveAudio(IAsyncResult res)
        {
            byte[] buffer;

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, __localPort);
            buffer = __udpClient.EndReceive(res, ref RemoteIpEndPoint);

            VoicePacket packet = VoicePacket.Create(buffer);
            ByteBuffer nonce = new ByteBuffer(24);
            nonce.WriteByteArrayToBuffer(packet.GetHeader(), 0);

            buffer = SecretBox.Open(packet.GetData(), nonce.GetBuffer(), __secretKey);

            byte[] output;
            int outputLength;

            output = _opusDecoder.Decode(buffer, buffer.Length, out outputLength);

            uint ssrc = packet.GetSSRC();
            ulong userId = 0;
            if (DiscordClient._ssrcDict.ContainsKey(ssrc))
                userId = DiscordClient._ssrcDict[ssrc];

            VoiceReceived?.Invoke(this, new VoiceReceivedEventArgs(ssrc, userId, output, outputLength));

            __udpClient.BeginReceive(ReceiveAudio, null);
        }

        public async Task SendAsync(byte[] data)
        {
            if (DiscordClient.config.VoiceSettings == VoiceSettings.Sending || DiscordClient.config.VoiceSettings == VoiceSettings.Both)
            {
                ByteBuffer buffer = new ByteBuffer(12 + data.Length);
                buffer.WriteByteToBuffer(0x80, 0);
                buffer.WriteByteToBuffer(0x78, 1);
                buffer.WriteUShortToBuffer(__sequence, VoiceConstants.SEQUENCE_INDEX);
                buffer.WriteUIntToBuffer(0, VoiceConstants.TIMESTAMP_INDEX);
                buffer.WriteUIntToBuffer(__ssrc, VoiceConstants.SSRC_INDEX);

                int encodedLength;
                byte[] encoded = _opusEncoder.Encode(data, data.Length, out encodedLength);

                buffer.WriteByteArrayToBuffer(encoded, 12);

                VoicePacket packet = VoicePacket.Create(buffer.GetBuffer());

                await SendVoicePacket(packet);
            }
            else throw new NotSupportedException(nameof(DiscordClient.config.VoiceSettings));
        }

        internal async Task SendVoicePacket(VoicePacket packet)
        {
            await __udpClient.SendAsync(packet.GetPacket(), packet.GetPacket().Length);
        }

        ~DiscordVoiceClient()
        {
            Dispose();
        }

        private bool disposed;
        public void Dispose()
        {
            if (disposed)
                return;

            GC.SuppressFinalize(this);

            __udpClient?.Close();
            _websocketClient?.Dispose();
            _opusDecoder?.Dispose();
            _opusEncoder?.Dispose();

            disposed = true;
        }
    }
}
