﻿using System;
using System.Buffers.Binary;

namespace DSharpPlus.VoiceNext.Codec
{
    internal sealed class Rtp : IDisposable
    {
        public const int HeaderSize = 12;

        private const byte RtpNoExtension = 0x80;
        private const byte RtpExtension = 0x90;
        private const byte RtpVersion = 0x78;

        public Rtp()
        { }

        public void EncodeHeader(ushort sequence, uint timestamp, uint ssrc, Span<byte> target)
        {
            if (target.Length < HeaderSize)
                throw new ArgumentException("Header buffer is too short.", nameof(target));

            target[0] = RtpNoExtension;
            target[1] = RtpVersion;

            // Write data big endian
            BinaryPrimitives.WriteUInt16BigEndian(target.Slice(2), sequence);  // header + magic
            BinaryPrimitives.WriteUInt32BigEndian(target.Slice(4), timestamp); // header + magic + sizeof(sequence)
            BinaryPrimitives.WriteUInt32BigEndian(target.Slice(8), ssrc);      // header + magic + sizeof(sequence) + sizeof(timestamp)
        }

#if !NETSTANDARD1_1
        public bool IsRtpHeader(ReadOnlySpan<byte> source)
        {
            if (source.Length < HeaderSize)
                return false;

            if ((source[0] != RtpNoExtension && source[0] != RtpExtension) || source[1] != RtpVersion)
                return false;

            return true;
        }

        public void DecodeHeader(ReadOnlySpan<byte> source, out ushort sequence, out uint timestamp, out uint ssrc, out bool hasExtension)
        {
            if (source.Length < HeaderSize)
                throw new ArgumentException("Header buffer is too short.", nameof(source));

            if ((source[0] != RtpNoExtension && source[0] != RtpExtension) || source[1] != RtpVersion)
                throw new ArgumentException("Invalid RTP header.", nameof(source));

            hasExtension = source[0] == RtpExtension;

            // Read data big endian
            sequence = BinaryPrimitives.ReadUInt16BigEndian(source.Slice(2));
            timestamp = BinaryPrimitives.ReadUInt32BigEndian(source.Slice(4));
            ssrc = BinaryPrimitives.ReadUInt32BigEndian(source.Slice(8));
        }
#endif

        public int CalculatePacketSize(int encryptedLength, EncryptionMode encryptionMode)
        {
            switch (encryptionMode)
            {
                case EncryptionMode.XSalsa20_Poly1305:
                    return HeaderSize + encryptedLength;

                case EncryptionMode.XSalsa20_Poly1305_Suffix:
                    return HeaderSize + encryptedLength + Interop.SodiumNonceSize;

                case EncryptionMode.XSalsa20_Poly1305_Lite:
                    return HeaderSize + encryptedLength + 4;

                default:
                    throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode));
            }
        }

#if !NETSTANDARD1_1
        public void GetDataFromPacket(ReadOnlySpan<byte> packet, out ReadOnlySpan<byte> data, EncryptionMode encryptionMode)
        {
            switch (encryptionMode)
            {
                case EncryptionMode.XSalsa20_Poly1305:
                    data = packet.Slice(HeaderSize);
                    return;

                case EncryptionMode.XSalsa20_Poly1305_Suffix:
                    data = packet.Slice(HeaderSize, packet.Length - HeaderSize - Interop.SodiumNonceSize);
                    return;

                case EncryptionMode.XSalsa20_Poly1305_Lite:
                    data = packet.Slice(HeaderSize, packet.Length - HeaderSize - 4);
                    break;

                default:
                    throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode));
            }
        }
#endif

        public void Dispose()
        {

        }
    }
}
