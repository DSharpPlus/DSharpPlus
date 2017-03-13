using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext.Codec;
using DSharpPlus.VoiceNext.VoiceEntities;
using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext
{
    public sealed class VoiceConnection
    {
        private UdpClient UdpClient { get; set; }
        private WebSocketClient VoiceWs { get; set; }

        private VoiceServerUpdatePayload ServerData { get; set; }
        private VoiceStateUpdatePayload StateData { get; set; }

        private OpusCodec Opus { get; set; }
        private SodiumCodec Sodium { get; set; }
        private RtpCodec RTP { get; set; }

        private ushort Sequence { get; set; }
        private uint Timestamp { get; set; }
        private uint SSRC { get; set; }
        private byte[] Key { get; set; }

        internal VoiceConnection(VoiceServerUpdatePayload server, VoiceStateUpdatePayload state)
        {
            this.ServerData = server;
            this.StateData = state;

            this.VoiceWs = new WebSocketClient($"wss://{this.ServerData.EndPoint}");
            this.VoiceWs.SocketClosed += this.VoiceWS_SocketClosed;
            this.VoiceWs.SocketError += this.VoiceWS_SocketError;
            this.VoiceWs.SocketMessage += this.VoiceWS_SocketMessage;
            this.VoiceWs.SocketOpened += this.VoiceWS_SocketOpened;

            this.VoiceWs.Connect();
        }

        internal async Task StartAsync()
        {
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


        }

        public async Task SendAsync(byte[] pcm, int bitrate = 16)
        {
            var rtp = this.RTP.Encode(this.Sequence, this.Timestamp, this.SSRC);

            var dat = this.Opus.Encode(pcm, 0, pcm.Length, bitrate);
            dat = this.Sodium.Encode(dat, this.RTP.MakeNonce(rtp), this.Key);
            dat = this.RTP.Encode(rtp, dat);

            await this.UdpClient.SendAsync(dat, dat.Length);
        }

        public async Task SendSpeaking(bool speaking = true)
        {
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

        private Task VoiceWS_SocketClosed(WebSocketSharp.CloseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task VoiceWS_SocketError(WebSocketSharp.ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task VoiceWS_SocketMessage(WebSocketSharp.MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task VoiceWS_SocketOpened()
        {
            throw new NotImplementedException();
        }
    }
}
