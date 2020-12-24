using System;

namespace DSharpPlus.VideoNext
{
    public class RawVideoPacket
    {
        public RawVideoPacket(Memory<byte> bytes, int duration, bool silence)
        {
            Bytes = bytes;
            Duration = duration;
            Silence = silence;
            RentedBuffer = null;
        }

        public readonly Memory<byte> Bytes;
        public readonly int Duration;
        public readonly bool Silence;

        public readonly byte[] RentedBuffer;
    }
}