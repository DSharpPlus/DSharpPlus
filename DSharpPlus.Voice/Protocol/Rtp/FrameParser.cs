using System;
using System.Buffers.Binary;

using CommunityToolkit.HighPerformance.Helpers;

namespace DSharpPlus.Voice.Protocol.Rtp;

/// <summary>
/// Provides ways to obtain <see cref="RtpFrameInfo"/> instances depending on the encryption method used and its specific requirements.
/// </summary>
internal static class FrameParser
{
    /// <summary>
    /// Parses a frame with Discord RTP header and a suffixed nonce of the specified length.
    /// </summary>
    public static RtpFrameInfo ParseRtpsizeSuffixedNonce(ReadOnlySpan<byte> frame, int nonceSize)
    {
        const int discordRtpHeaderSize = 12;
        int csrcCount = frame[0] & 0x0F;
        int headerLength = discordRtpHeaderSize + (csrcCount * 4);

        Range header, cipher, nonce;

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

            return new RtpFrameInfo
            {
                Header = header,
                VoiceData = cipher,
                Nonce = nonce,
                ExtensionHeaderLength = extensionLength
            };
        }

        // this path is considerably simpler; nothing overlaps and we don't have extra dynamic lengths

        header = 0..headerLength;
        cipher = headerLength..(frame.Length - nonceSize);
        nonce = (frame.Length - nonceSize)..frame.Length;

        return new RtpFrameInfo
        {
            Header = header,
            VoiceData = cipher,
            Nonce = nonce,
            ExtensionHeaderLength = 0
        };
    }
}
