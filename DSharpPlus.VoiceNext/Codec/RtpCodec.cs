using System;

namespace DSharpPlus.VoiceNext.Codec
{
    public sealed class RtpCodec
    {
        private const byte RTP_TYPE_NO_EXTENSION = 0x80;
        private const byte RTP_TYPE_EXTENSION = 0x90;
        private const byte RTP_VERSION = 0x78;

        private const int OFFSET_SEQUENCE = 2;
        private const int OFFSET_TIMESTAMP = 4;
        private const int OFFSET_SSRC = 8;

        private const int SIZE_NONCE = 24;
        public const int SIZE_HEADER = 12;

        public byte[] Encode(ushort sequence, uint timestamp, uint ssrc)
        {
            byte[] header = new byte[SIZE_HEADER];

            header[0] = RTP_TYPE_NO_EXTENSION;
            header[1] = RTP_VERSION;

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

            Array.Copy(seqnb, 0, header, OFFSET_SEQUENCE, seqnb.Length);
            Array.Copy(tmspb, 0, header, OFFSET_TIMESTAMP, tmspb.Length);
            Array.Copy(ssrcb, 0, header, OFFSET_SSRC, ssrcb.Length);

            return header;
        }

        public void Decode(byte[] header, out ushort sequence, out uint timestamp, out uint ssrc, out bool has_extension)
        {
            if (header.Length != SIZE_HEADER)
                throw new ArgumentException(nameof(header), string.Concat("Wrong header size (must be", SIZE_HEADER, ")"));

            if ((header[0] != RTP_TYPE_NO_EXTENSION && header[0] != RTP_TYPE_EXTENSION) || header[1] != RTP_VERSION)
                throw new ArgumentException(nameof(header), "Invalid header");

            has_extension = header[0] == RTP_TYPE_EXTENSION;

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
            if (header.Length != SIZE_HEADER)
                throw new ArgumentException(nameof(header), string.Concat("Wrong header size (must be", SIZE_HEADER, ")"));

            var buff = new byte[header.Length + data.Length];

            Array.Copy(header, 0, buff, 0, header.Length);
            Array.Copy(data, 0, buff, header.Length, data.Length);

            return buff;
        }

        public byte[] Decode(byte[] data, byte[] header)
        {
            if (header.Length != SIZE_HEADER)
                throw new ArgumentException(nameof(header), string.Concat("Wrong header size (must be", SIZE_HEADER, ")"));

            var buff = new byte[data.Length - header.Length];

            Array.Copy(data, header.Length, buff, 0, buff.Length);
            Array.Copy(data, 0, header, 0, header.Length);

            return buff;
        }

        public byte[] MakeNonce(byte[] header)
        {
            if (header.Length != SIZE_HEADER)
                throw new ArgumentException(nameof(header), string.Concat("Wrong header size (must be", SIZE_HEADER, ")"));

            var nonce = new byte[SIZE_NONCE];

            Array.Copy(header, nonce, header.Length);

            return nonce;
        }
    }
}
