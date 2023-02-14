// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023

 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
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

        public bool IsRtpHeader(ReadOnlySpan<byte> source)
        {
            if (source.Length < HeaderSize)
                return false;

            return (source[0] == RtpNoExtension || source[0] == RtpExtension) && source[1] == RtpVersion;
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

        public int CalculatePacketSize(int encryptedLength, EncryptionMode encryptionMode)
        {
            return encryptionMode switch
            {
                EncryptionMode.XSalsa20_Poly1305 => HeaderSize + encryptedLength,
                EncryptionMode.XSalsa20_Poly1305_Suffix => HeaderSize + encryptedLength + Interop.SodiumNonceSize,
                EncryptionMode.XSalsa20_Poly1305_Lite => HeaderSize + encryptedLength + 4,
                _ => throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode)),
            };
        }

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

        public void Dispose()
        {

        }
    }
}
