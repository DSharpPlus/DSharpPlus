using System;

namespace DSharpPlus.VoiceNext;

internal readonly struct RawVoicePacket
{
    public RawVoicePacket(Memory<byte> bytes, int duration, bool silence)
    {
        this.Bytes = bytes;
        this.Duration = duration;
        this.Silence = silence;
        this.RentedBuffer = null;
    }

    public RawVoicePacket(Memory<byte> bytes, int duration, bool silence, byte[] rentedBuffer)
        : this(bytes, duration, silence) => this.RentedBuffer = rentedBuffer;

    public readonly Memory<byte> Bytes;
    public readonly int Duration;
    public readonly bool Silence;

    public readonly byte[] RentedBuffer;
}
