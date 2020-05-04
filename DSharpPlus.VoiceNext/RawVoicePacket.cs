using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.VoiceNext
{
    internal readonly struct RawVoicePacket
    {
        public RawVoicePacket(Memory<byte> bytes, int duration, bool silence)
        {
            Bytes = bytes;
            Duration = duration;
            Silence = silence;

            ArrayPoolData = null;
            ReturnToArrayPool = false;
        }

        public RawVoicePacket(Memory<byte> bytes, int duration, bool silence, byte[] array)
            : this(bytes, duration, silence)
        {
            ArrayPoolData = array;
            ReturnToArrayPool = true;
        }

        public readonly Memory<byte> Bytes;
        public readonly int Duration;
        public readonly bool Silence;

        public readonly byte[] ArrayPoolData;
        public readonly bool ReturnToArrayPool;
    }
}
