using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext.Codec;

namespace DSharpPlus.VoiceNext
{
    public sealed class VoiceConnection
    {
        private UdpClient UdpClient { get; set; }
        private WebSocketClient VoiceWS { get; set; }

        private OpusCodec Opus { get; set; }
        private SodiumCodec Sodium { get; set; }
        private RtpCodec RTP { get; set; }

        private ushort Sequence { get; set; }
        private uint Timestamp { get; set; }
        private uint SSRC { get; set; }
        private byte[] Key { get; set; }

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

        }
    }
}
