using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext.Codec;
using DSharpPlus.VoiceNext.VoiceEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.VoiceNext
{
    public sealed class VoiceConnection : IDisposable
    {
        private const string VOICE_MODE = "xsalsa20_poly1305";

        private DiscordClient Discord { get; set; }

        private UdpClient UdpClient { get; set; }
        private WebSocketClient VoiceWs { get; set; }
        private Thread HeartbeatThread { get; set; }
        private int HeartbeatInterval { get; set; }

        private VoiceServerUpdatePayload ServerData { get; set; }
        private VoiceStateUpdatePayload StateData { get; set; }

        private OpusCodec Opus { get; set; }
        private SodiumCodec Sodium { get; set; }
        private RtpCodec RTP { get; set; }

        private ushort Sequence { get; set; }
        private uint Timestamp { get; set; }
        private uint SSRC { get; set; }
        private byte[] Key { get; set; }
        private IPEndPoint DiscoveredEndpoint { get; set; }
        private DnsEndPoint ConnectionEndpoint { get; set; }

        private TaskCompletionSource<bool> Stage1 { get; set; }
        private TaskCompletionSource<bool> Stage2 { get; set; }
        private bool IsInitialized { get; set; }
        private bool IsDisposed { get; set; }

        internal VoiceConnection(DiscordClient client, VoiceServerUpdatePayload server, VoiceStateUpdatePayload state)
        {
            this.Discord = client;

            this.ServerData = server;
            this.StateData = state;

            var eps = this.ServerData.Endpoint;
            var epi = eps.LastIndexOf(':');
            var eph = string.Empty;
            if (epi != -1)
                eph = eps.Substring(0, epi);
            else
                eph = eps;
            this.ConnectionEndpoint = new DnsEndPoint(eph, 443);

            this.Stage1 = new TaskCompletionSource<bool>();
            this.Stage2 = new TaskCompletionSource<bool>();
            this.IsInitialized = false;
            this.IsDisposed = false;

            this.VoiceWs = new WebSocketClient($"wss://{this.ServerData.Endpoint}");
            this.VoiceWs.SocketClosed += this.VoiceWS_SocketClosed;
            this.VoiceWs.SocketError += this.VoiceWS_SocketError;
            this.VoiceWs.SocketMessage += this.VoiceWS_SocketMessage;
            this.VoiceWs.SocketOpened += this.VoiceWS_SocketOpened;

            this.VoiceWs.Connect();
        }

        ~VoiceConnection()
        {
            this.Dispose();
        }

        internal async Task StartAsync()
        {
            // Let's announce our intentions to the server
            var vdp = new VoiceDispatch
            {
                OpCode = 0,
                Payload = new VoiceIdentifyPayload
                {
                    ServerId = this.ServerData.GuildId,
                    UserId = this.StateData.UserId,
                    SessionId = this.StateData.SessionId,
                    Token = this.ServerData.Token
                }
            };
            var vdj = JsonConvert.SerializeObject(vdp, Formatting.None);
            await Task.Run(() => this.VoiceWs._socket.Send(vdj));
            await this.Stage1.Task; // wait for response

            // Begin heartbeating
            this.HeartbeatThread = new Thread(this.Heartbeat);
            this.HeartbeatThread.SetApartmentState(ApartmentState.STA);
            this.HeartbeatThread.Name = $"Heartbeat for Voice ({this.SSRC})";
            this.HeartbeatThread.Start();

            // IP Discovery
            this.UdpClient.Connect(this.ConnectionEndpoint.Host, this.ConnectionEndpoint.Port);
            this.UdpClient.AllowNatTraversal(true);
            var pck = this.RTP.Encode(this.RTP.Encode(this.Sequence, this.Timestamp, this.SSRC), new byte[70]);
            await this.UdpClient.SendAsync(pck, pck.Length);
            var ipd = await this.UdpClient.ReceiveAsync();
            var ipe = Array.IndexOf(ipd.Buffer, 0, 12);
            var ip = new UTF8Encoding(false).GetString(ipd.Buffer, 12, ipe - 12);
            var port = BitConverter.ToUInt16(ipd.Buffer, 80);
            this.DiscoveredEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);

            // Ready
            var vsp = new VoiceDispatch
            {
                OpCode = 1,
                Payload = new VoiceSelectProtocolPayload
                {
                    Protocol = "udp",
                    Data = new VoiceSelectProtocolPayloadData
                    {
                        Address = this.DiscoveredEndpoint.Address.ToString(),
                        Port = (ushort)this.DiscoveredEndpoint.Port,
                        Mode = VOICE_MODE
                    }
                }
            }
            var vsj = JsonConvert.SerializeObject(vsp, Formatting.None);
            await Task.Run(() => this.VoiceWs._socket.Send(vsj));
            await this.Stage2.Task;

            this.IsInitialized = true;
        }

        public async Task SendAsync(byte[] pcm, int bitrate = 16)
        {
            if (!this.IsInitialized)
                throw new InvalidOperationException("The connection is not yet initialized");

            var rtp = this.RTP.Encode(this.Sequence, this.Timestamp, this.SSRC);

            var dat = this.Opus.Encode(pcm, 0, pcm.Length, bitrate);
            dat = this.Sodium.Encode(dat, this.RTP.MakeNonce(rtp), this.Key);
            dat = this.RTP.Encode(rtp, dat);

            await this.UdpClient.SendAsync(dat, dat.Length);
        }

        public async Task SendSpeaking(bool speaking = true)
        {
            if (!this.IsInitialized)
                throw new InvalidOperationException("The connection is not yet initialized");

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
            await Task.Run(() => this.VoiceWs._socket.Send(plj));
        }

        public void Dispose()
        {
            if (this.IsDisposed)
                return;

            this.IsDisposed = true;
            this.IsInitialized = false;
            try
            {
                this.VoiceWs.Disconnect();
                this.UdpClient.Close();
            }
            catch (Exception)
            { }

            this.Opus.Dispose();
            this.Opus = null;
            this.Sodium = null;
            this.RTP = null;
        }

        private void Heartbeat()
        {
            while(true)
            {
                var hbd = new VoiceDispatch
                {
                    OpCode = 3,
                    Payload = this.HeartbeatInterval
                };
                var hbj = JsonConvert.SerializeObject(hbd);
                this.VoiceWs._socket.Send(hbj);

                Thread.Sleep(this.HeartbeatInterval);
            }
        }

        private async Task HandleDispatch(JObject jo)
        {
            var opc = (int)jo["op"];
            var opp = jo["d"] as JObject;

            switch (opc)
            {
                case 2:
                    var vrp = opp.ToObject<VoiceReadyPayload>();
                    this.SSRC = vrp.SSRC;
                    this.ConnectionEndpoint = new DnsEndPoint(this.ConnectionEndpoint.Host, vrp.Port);
                    this.Stage1.SetResult(true);
                    break;

                case 4:
                    this.Stage2.SetResult(true);
                    break;
            }
        }

        private Task VoiceWS_SocketClosed(WebSocketSharp.CloseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task VoiceWS_SocketError(WebSocketSharp.ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async Task VoiceWS_SocketMessage(WebSocketSharp.MessageEventArgs e)
        {
            await this.HandleDispatch(JObject.Load(e.Data));
        }

        private Task VoiceWS_SocketOpened()
        {
            throw new NotImplementedException();
        }
    }
}
