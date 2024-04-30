namespace DSharpPlus.VoiceNext;
using System;

internal readonly struct RawVoicePacket
{
    public RawVoicePacket(Memory<byte> bytes, int duration, bool silence)
    {
        Bytes = bytes;
        Duration = duration;
        Silence = silence;
        RentedBuffer = null;
    }

    public RawVoicePacket(Memory<byte> bytes, int duration, bool silence, byte[] rentedBuffer)
        : this(bytes, duration, silence) => RentedBuffer = rentedBuffer;

    public readonly Memory<byte> Bytes;
    public readonly int Duration;
    public readonly bool Silence;

    public readonly byte[] RentedBuffer;
}
