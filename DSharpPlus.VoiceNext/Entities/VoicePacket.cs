using System;

namespace DSharpPlus.VoiceNext.Entities
{
    internal struct VoicePacket
    {
        public ReadOnlyMemory<byte> Bytes { get; }
        public int MillisecondDuration { get; }
        public bool IsSilence { get; set; }

        public VoicePacket(ReadOnlyMemory<byte> bytes, int msDuration, bool isSilence = false)
        {
            this.Bytes = bytes;
            this.MillisecondDuration = msDuration;
            this.IsSilence = isSilence;
        }
    }
}
