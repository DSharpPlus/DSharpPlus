using System;
using System.Diagnostics;

namespace DSharpPlus.Voice.Codec;

/// <inheritdoc cref="IAudioCodec"/>
public sealed class OpusCodec : IAudioCodec
{
    private IAudioEncoder? encoder;

    /// <inheritdoc/>
    public IAudioEncoder CreateEncoder(int bitrate, AudioType type)
    {
        IAudioEncoder encoder = new OpusEncoder(bitrate, type);

        this.encoder = encoder;
        return encoder;
    }

    /// <inheritdoc/>
    public IAudioEncoder GetEncoder()
    {
        if (this.encoder is null)
        {
            throw new InvalidOperationException("This may only be called after a connection was established.");
        }

        return this.encoder;
    }

    /// <inheritdoc/>
    public IAudioDecoder CreateDecoder()
        => new OpusDecoder();

    /// <inheritdoc/>
    public TimeSpan CalculatePacketLength(ReadOnlySpan<byte> packet)
    {
        Debug.Assert(packet.Length > 0);

        // the five topmost bits of the first byte identify the bandwidth, codec mode and, most importantly, frame size
        // refer to RFC 6716, section 3.1
        int config = packet[0] >> 3;

        TimeSpan frameSize = config switch
        {
            16 or 20 or 24 or 28 => TimeSpan.FromMilliseconds(2.5),
            17 or 21 or 25 or 29 => TimeSpan.FromMilliseconds(5),
            0 or 4 or 8 or 12 or 14 or 18 or 22 or 26 or 30 => TimeSpan.FromMilliseconds(10),
            1 or 5 or 9 or 13 or 15 or 19 or 23 or 27 or 31 => TimeSpan.FromMilliseconds(20),
            2 or 6 or 10 => TimeSpan.FromMilliseconds(40),
            3 or 7 or 11 => TimeSpan.FromMilliseconds(60),
            _ => throw new UnreachableException()
        };

        // then, we figure out how many frames are in the packet. the size of the frames must be the same within one packet
        // refer to RFC 6716, sections 3.2.2 through 3.2.5
        int framePackingCode = packet[0] & 0b11;

        if (framePackingCode == 0)
        {
            // a single frame in the packet
            return frameSize;
        }
        else if (framePackingCode is 1 or 2)
        {
            // two frames in the packet, either with identical or different byte count - the exact byte count is immaterial to us
            return frameSize * 2;
        }
        else
        {
            // a variable amount of frames in the packet, encoded in the second byte of the packet
            // if the packet isn't long enough to support this, we're clearly trying to get the length of a packet that isn't an opus
            // packet - explode
            Debug.Assert(packet.Length > 1);

            int frameCount = packet[1] & 0b0011_1111;
            TimeSpan total =  frameSize * frameCount;

            // likewise, explode if the packet would contain more audio than opus allows (RFC 6716 Section 3.4 R5)
            Debug.Assert(total <= TimeSpan.FromMilliseconds(120));

            return total;
        }
    }
}
