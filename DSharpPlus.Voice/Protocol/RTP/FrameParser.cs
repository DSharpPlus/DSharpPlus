using System;
using System.Buffers.Binary;

using CommunityToolkit.HighPerformance.Helpers;

namespace DSharpPlus.Voice.Protocol.RTP;

/// <summary>
/// Provides ways to obtain <see cref="RTPFrameInfo"/> instances depending on the encryption method used and its specific requirements.
/// </summary>
internal static class FrameParser
{
    /// <summary>
    /// Parses a frame with Discord RTP header and a suffixed nonce of the specified length.
    /// </summary>
    public static RTPFrameInfo ParseRtpsizeSuffixedNonce(ReadOnlySpan<byte> frame, int nonceSize)
    {
        const int discordRtpHeaderSize = 12;
        int csrcCount = frame[0] & 0x0F;
        int headerLength = discordRtpHeaderSize + (csrcCount * 4);

        Range header, cipher, nonce;
        uint ssrc, timestamp;
        ushort sequence;

        if (BitHelper.HasFlag(frame[0], 4))
        {
            Span<byte> extensionWords = stackalloc byte[2];
            frame[2..4].CopyTo(extensionWords);
            ushort extensionLength = BinaryPrimitives.ReadUInt16BigEndian(extensionWords);

            extensionLength *= 4;

            // the extension header itself is encrypted along with the opus packet, so we have overlapping headers and ciphertext. it's very fun.

            // the header length is the base header length + magic 0xBEDE + the length of the extension header. we need to skip the extension header
            // later when decrypting from E2EE (or handling the opus data directly if E2EE is disabled).
            header = 0..(headerLength + 4);
            cipher = (headerLength + 4)..(frame.Length - nonceSize);
            nonce = (frame.Length - nonceSize)..frame.Length;
            sequence = BinaryPrimitives.ReadUInt16BigEndian(frame[header][2..4]);
            timestamp = BinaryPrimitives.ReadUInt32BigEndian(frame[header][4..8]);
            ssrc = BinaryPrimitives.ReadUInt32BigEndian(frame[header][8..12]);

            return new RTPFrameInfo
            {
                Header = header,
                VoiceData = cipher,
                Nonce = nonce,
                ExtensionHeaderLength = extensionLength,
                SSRC = ssrc,
                Timestamp = timestamp,
                Sequence = sequence
            };
        }

        // this path is considerably simpler; nothing overlaps and we don't have extra dynamic lengths

        header = 0..headerLength;
        cipher = headerLength..(frame.Length - nonceSize);
        nonce = (frame.Length - nonceSize)..frame.Length;
        sequence = BinaryPrimitives.ReadUInt16BigEndian(frame[header][2..4]);
        timestamp = BinaryPrimitives.ReadUInt32BigEndian(frame[header][4..8]);
        ssrc = BinaryPrimitives.ReadUInt32BigEndian(frame[header][8..12]);

        return new RTPFrameInfo
        {
            Header = header,
            VoiceData = cipher,
            Nonce = nonce,
            ExtensionHeaderLength = 0,
            SSRC = ssrc,
            Timestamp = timestamp,
            Sequence = sequence
        };
    }
}
