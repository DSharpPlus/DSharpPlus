using System;
using System.Buffers.Binary;

namespace DSharpPlus.VoiceNext.Codec;

internal sealed class Rtp : IDisposable
{
    public const int HeaderSize = 12;

    private const byte RtpNoExtension = 0x80;
    private const byte RtpExtension = 0x90;
    private const byte RtpVersion = 0x78;

    public Rtp()
    { }

    public static void EncodeHeader(ushort sequence, uint timestamp, uint ssrc, Span<byte> target)
    {
        if (target.Length < HeaderSize)
        {
            throw new ArgumentException("Header buffer is too short.", nameof(target));
        }

        target[0] = RtpNoExtension;
        target[1] = RtpVersion;

        // Write data big endian
        BinaryPrimitives.WriteUInt16BigEndian(target[2..], sequence);  // header + magic
        BinaryPrimitives.WriteUInt32BigEndian(target[4..], timestamp); // header + magic + sizeof(sequence)
        BinaryPrimitives.WriteUInt32BigEndian(target[8..], ssrc);      // header + magic + sizeof(sequence) + sizeof(timestamp)
    }

    public static bool IsRtpHeader(ReadOnlySpan<byte> source) => source.Length >= HeaderSize && (source[0] == RtpNoExtension || source[0] == RtpExtension) && source[1] == RtpVersion;

    public static void DecodeHeader(ReadOnlySpan<byte> source, out ushort sequence, out uint timestamp, out uint ssrc, out bool hasExtension)
    {
        if (source.Length < HeaderSize)
        {
            throw new ArgumentException("Header buffer is too short.", nameof(source));
        }

        if ((source[0] != RtpNoExtension && source[0] != RtpExtension) || source[1] != RtpVersion)
        {
            throw new ArgumentException("Invalid RTP header.", nameof(source));
        }

        hasExtension = source[0] == RtpExtension;

        // Read data big endian
        sequence = BinaryPrimitives.ReadUInt16BigEndian(source[2..]);
        timestamp = BinaryPrimitives.ReadUInt32BigEndian(source[4..]);
        ssrc = BinaryPrimitives.ReadUInt32BigEndian(source[8..]);
    }

    public static int CalculatePacketSize(int encryptedLength, EncryptionMode encryptionMode) => encryptionMode switch
    {
        EncryptionMode.AeadAes256GcmRtpSize => HeaderSize + encryptedLength + 4,
        _ => throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode)),
    };

    public static void GetDataFromPacket(ReadOnlySpan<byte> packet, out ReadOnlySpan<byte> data, EncryptionMode encryptionMode)
    {
        switch (encryptionMode)
        {
            case EncryptionMode.AeadAes256GcmRtpSize:
                data = packet.Slice(HeaderSize, packet.Length - HeaderSize - 4);
                return;

            default:
                throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode));
        }
    }

    public void Dispose()
    {

    }
}
