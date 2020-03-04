using System;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext.Codec;

namespace DSharpPlus.VoiceNext.Entities
{
    internal class AudioSender : IDisposable
    {
        public uint SSRC { get; }
        public ulong Id => this.User?.Id ?? 0;
        public OpusDecoder Decoder { get; }
        public DiscordUser User { get; set; } = null;
        public ushort LastSequence { get; set; } = 0;

        public AudioSender(uint ssrc, OpusDecoder decoder)
        {
            this.SSRC = ssrc;
            this.Decoder = decoder;
        }

        public void Dispose()
        {
            this.Decoder?.Dispose();
        }
    }
}