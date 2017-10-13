using System;

namespace DSharpPlus.VoiceNext.Codec
{
    internal sealed class RtpCodec
    {
        private const byte RtpTypeNoExtension = 0x80;
        private const byte RtpTypeExtension = 0x90;
        private const byte RtpVersion = 0x78;

        private const int OffsetSequence = 2;
        private const int OffsetTimestamp = 4;
        private const int OffsetSsrc = 8;

        private const int SizeNonce = 24;
        public const int SizeHeader = 12;

        public byte[] Encode(ushort sequence, uint timestamp, uint ssrc)
        {
            byte[] header = new byte[SizeHeader];

            header[0] = RtpTypeNoExtension;
            header[1] = RtpVersion;

            var flip = BitConverter.IsLittleEndian;
            var seqnb = BitConverter.GetBytes(sequence);
            var tmspb = BitConverter.GetBytes(timestamp);
            var ssrcb = BitConverter.GetBytes(ssrc);

            if (flip)
            {
                Array.Reverse(seqnb);
                Array.Reverse(tmspb);
                Array.Reverse(ssrcb);
            }

            Array.Copy(seqnb, 0, header, OffsetSequence, seqnb.Length);
            Array.Copy(tmspb, 0, header, OffsetTimestamp, tmspb.Length);
            Array.Copy(ssrcb, 0, header, OffsetSsrc, ssrcb.Length);

            return header;
        }

        public void Decode(byte[] header, out ushort sequence, out uint timestamp, out uint ssrc, out bool hasExtension)
        {
            if (header.Length != SizeHeader)
            {
                throw new ArgumentException(string.Concat("Wrong header size (must be", SizeHeader, ")"), nameof(header));
            }

            if (header[0] != RtpTypeNoExtension && header[0] != RtpTypeExtension || header[1] != RtpVersion)
            {
                throw new ArgumentException("Invalid header", nameof(header));
            }

            hasExtension = header[0] == RtpTypeExtension;

            var flip = BitConverter.IsLittleEndian;
            if (flip)
            {
                Array.Reverse(header, 2, 2);
                Array.Reverse(header, 4, 4);
                Array.Reverse(header, 8, 4);
            }

            sequence = BitConverter.ToUInt16(header, 2);
            timestamp = BitConverter.ToUInt32(header, 4);
            ssrc = BitConverter.ToUInt32(header, 8);
        }

        public byte[] Encode(byte[] header, byte[] data)
        {
            if (header.Length != SizeHeader)
            {
                throw new ArgumentException(nameof(header), string.Concat("Wrong header size (must be", SizeHeader, ")"));
            }

            var buff = new byte[header.Length + data.Length];

            Array.Copy(header, 0, buff, 0, header.Length);
            Array.Copy(data, 0, buff, header.Length, data.Length);

            return buff;
        }

        public byte[] Decode(byte[] data, byte[] header)
        {
            if (header.Length != SizeHeader)
            {
                throw new ArgumentException(nameof(header), string.Concat("Wrong header size (must be", SizeHeader, ")"));
            }

            var buff = new byte[data.Length - header.Length];

            Array.Copy(data, header.Length, buff, 0, buff.Length);
            Array.Copy(data, 0, header, 0, header.Length);

            return buff;
        }

        public byte[] MakeNonce(byte[] header)
        {
            if (header.Length != SizeHeader)
            {
                throw new ArgumentException(nameof(header), string.Concat("Wrong header size (must be", SizeHeader, ")"));
            }

            var nonce = new byte[SizeNonce];

            Array.Copy(header, nonce, header.Length);

            return nonce;
        }
    }
}
