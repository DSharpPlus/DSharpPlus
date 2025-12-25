using System;
using System.Buffers.Binary;

namespace DSharpPlus.Voice.Protocol.Rtp;

internal static class RtpHelper
{
    public static void WriteRtpHeader(Span<byte> target, ushort sequence, uint timestamp, uint ssrc)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(target.Length, 12);

        // rtp header and version
        target[0] = 0x80;
        target[1] = 0x78;

        BinaryPrimitives.WriteUInt16BigEndian(target[2..], sequence);
        BinaryPrimitives.WriteUInt32BigEndian(target[4..], timestamp);
        BinaryPrimitives.WriteUInt32BigEndian(target[8..], ssrc);
    }
}
