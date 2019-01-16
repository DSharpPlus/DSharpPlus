#if !NETSTANDARD1_1
using System;
using DSharpPlus.VoiceNext.Codec;

namespace DSharpPlus.VoiceNext.Entities
{
    internal struct AudioSender : IDisposable
    {
        public uint SSRC { get; }
        public ulong Id { get; set; }
        public OpusDecoder Decoder { get; }

        public AudioSender(uint ssrc, OpusDecoder decoder)
        {
            this.SSRC = ssrc;
            this.Decoder = decoder;
            this.Id = 0;
        }

        public void Dispose()
        {
            this.Decoder?.Dispose();
        }
    }
}
#endif