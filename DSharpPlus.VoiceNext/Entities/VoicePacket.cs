using System;

namespace DSharpPlus.VoiceNext.Entities
{
    internal struct VoicePacket
    {
        public ReadOnlyMemory<byte> Bytes { get; }
        public int MillisecondDuration { get; }

        public VoicePacket(ReadOnlyMemory<byte> bytes, int msDuration)
        {
            this.Bytes = bytes;
            this.MillisecondDuration = msDuration;
        }
    }
}
