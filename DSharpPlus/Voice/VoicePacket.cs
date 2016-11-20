using Sodium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Voice
{
    public class VoicePacket
    {
        private ByteBuffer buffer;
        private uint ssrc;
        private ushort seq;
        private uint timestamp;

        public ushort GetSequence() => seq;
        public uint GetTimestamp() => timestamp;
        public uint GetSSRC() => ssrc;
        public byte[] GetHeader() => buffer.ReadByteArrayFromBuffer(0, VoiceConstants.RTP_HEADER_LENGTH);
        public byte[] GetData() => buffer.ReadByteArrayFromBuffer(VoiceConstants.RTP_HEADER_LENGTH, buffer.GetBuffer().Length - VoiceConstants.RTP_HEADER_LENGTH);

        public byte[] GetPacket() => buffer.GetBuffer();

        public static VoicePacket Create(byte[] data)
        {
            var packet = new VoicePacket(data, 0, 0, 0);
            var buff = new ByteBuffer(data);
            packet.SetBuffer(buff);

            return packet;
        }

        private VoicePacket(byte[] data, uint ssrc, ushort seq, uint timestamp)
        {
            this.ssrc = ssrc;
            this.seq = seq;
            this.timestamp = timestamp;

            initBuffer(data);
        }

        private void initBuffer(byte[] data)
        {
            var header = NewHeader();
            var nonce = new ByteBuffer(24);
            nonce.WriteByteArrayToBuffer(header.GetBuffer(), 0);

            data = SecretBox.Create(data, nonce.GetBuffer(), DiscordVoiceClient.__secretKey);

            buffer = new ByteBuffer(header.GetBuffer().Length + data.Length);
            buffer.WriteByteArrayToBuffer(header.GetBuffer(), 0);
            buffer.WriteByteArrayToBuffer(data, VoiceConstants.RTP_HEADER_LENGTH);
        }

        private ByteBuffer NewHeader()
        {
            ByteBuffer header = new ByteBuffer(VoiceConstants.RTP_HEADER_LENGTH);
            header.WriteByteToBuffer(0x80, 0); // Type
            header.WriteByteToBuffer(0x78, 1); // Version
            header.WriteUShortToBuffer(seq, VoiceConstants.SEQUENCE_INDEX); // Sequence
            header.WriteUIntToBuffer(timestamp, VoiceConstants.TIMESTAMP_INDEX); // Timestamp
            header.WriteUIntToBuffer(ssrc, VoiceConstants.SSRC_INDEX); // SSRC

            return header;
        }

        public VoicePacket SetBuffer(ByteBuffer buffer)
        {
            this.buffer = buffer;

            seq = this.buffer.ReadUShortFromBuffer(VoiceConstants.SEQUENCE_INDEX);
            timestamp = this.buffer.ReadUIntFromBuffer(VoiceConstants.TIMESTAMP_INDEX);
            ssrc = this.buffer.ReadUIntFromBuffer(VoiceConstants.SSRC_INDEX);

            return this;
        }
    }
}
