using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    public class DiscordAudioPacket
    {
        //ty to JDA developer DV8 for documentating this in his library
        public static int RTP_HEADER_BYTE_LENGTH = 12;

        /**
         * Bit index 0 and 1 represent the RTP Protocol version used. Discord uses the latest RTP protocol version, 2.<br>
         * Bit index 2 represents whether or not we pad. Opus uses an internal padding system, so RTP padding is not used.<br>
         * Bit index 3 represents if we use extensions. Discord does not use RTP extensions.<br>
         * Bit index 4 to 7 represent the CC or CSRC count. CSRC is Combined SSRC. Discord doesn't combine audio streams,
         *      so the Combined count will always be 0 (binary: 0000).<br>
         * This byte should always be the same, no matter the library implementation.
         */
        public static byte RTP_VERSION_PAD_EXTEND = (byte)0x80;  //Binary: 1000 0000
        public static byte RTP_PAYLOAD_TYPE = (byte)0x78;        //Binary: 0100 1000

        public static short RTP_VERSION_PAD_EXTEND_INDEX = 0;
        public static short RTP_PAYLOAD_INDEX = 1;
        public static short SEQ_INDEX = 2;
        public static short TIMESTAMP_INDEX = 4;
        public static int SSRC_INDEX = 8;

        private byte seq;
        private int timestamp;
        private int ssrc;
        private byte[] encodedAudio;
        private byte[] rawPacket;

        public byte Seq => seq;
        public int Timestamp => timestamp;
        public int SSRC => ssrc;

        public DiscordAudioPacket(byte[] raw)
        {
            rawPacket = raw;

            using (MemoryStream ms = new MemoryStream(rawPacket))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    reader.BaseStream.Position = SEQ_INDEX;
                    seq = reader.ReadByte();

                    reader.BaseStream.Position = TIMESTAMP_INDEX;
                    timestamp = reader.ReadInt32();

                    reader.BaseStream.Position = SSRC_INDEX;
                    ssrc = reader.ReadInt32();

                    byte[] audio = new byte[rawPacket.Length - RTP_HEADER_BYTE_LENGTH];
                    Array.Copy(rawPacket, RTP_HEADER_BYTE_LENGTH, audio, 0, audio.Length);
                    encodedAudio = audio;
                }
            }
        }

        public DiscordAudioPacket(char seq, int timestamp, int ssrc, byte[] encodedaudio)
        {
            this.seq = (byte)seq;
            this.timestamp = timestamp;
            this.ssrc = ssrc;
            this.encodedAudio = encodedaudio;

            byte[] fullPacket = new byte[RTP_HEADER_BYTE_LENGTH + encodedAudio.Length];
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.BaseStream.Position = RTP_VERSION_PAD_EXTEND_INDEX;
                    writer.Write((byte)((RTP_VERSION_PAD_EXTEND >> 8) & 0xFF));
                    writer.Write((byte)((RTP_VERSION_PAD_EXTEND >> 0) & 0xFF));

                    writer.BaseStream.Position = RTP_PAYLOAD_INDEX;
                    writer.Write((byte)((RTP_PAYLOAD_TYPE >> 8) & 0xFF));
                    writer.Write((byte)((RTP_PAYLOAD_TYPE >> 0) & 0xFF));

                    writer.BaseStream.Position = SEQ_INDEX;
                    writer.Write((byte)seq);

                    writer.BaseStream.Position = TIMESTAMP_INDEX;
                    writer.Write((byte)((timestamp >> 24) & 0xFF));
                    writer.Write((byte)((timestamp >> 16) & 0xFF));
                    writer.Write((byte)((timestamp >> 8) & 0xFF));
                    writer.Write((byte)((timestamp >> 0) & 0xFF));

                    writer.BaseStream.Position = SSRC_INDEX;
                    writer.Write(IntToBytes(ssrc));

                    writer.BaseStream.Position = RTP_HEADER_BYTE_LENGTH;
                    writer.Write(encodedAudio);
                }
            }
            rawPacket = fullPacket;
        }

        //int is 4
        internal static byte[] IntToBytes(int i)
        {
            byte[] buffer = new byte[4];

            buffer[0] = (byte)((i >> 24) & 0xFF);
            buffer[1] = (byte)((i >> 16) & 0xFF);
            buffer[2] = (byte)((i >> 8) & 0xFF);
            buffer[3] = (byte)((i >> 0) & 0xFF);

            return buffer;
        }

        public byte[] AsRawPacket()
        {
            return rawPacket;
        }

        public byte[] GetEncodedAudio() => encodedAudio;

        public static DiscordAudioPacket EchoPacket(byte[] packet, int ssrc)
        {
            using (MemoryStream ms = new MemoryStream(packet))
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.BaseStream.Position = RTP_VERSION_PAD_EXTEND_INDEX;
                    writer.Write(RTP_VERSION_PAD_EXTEND);

                    writer.BaseStream.Position = RTP_PAYLOAD_INDEX;
                    writer.Write(RTP_PAYLOAD_TYPE);

                    writer.BaseStream.Position = SSRC_INDEX;
                    writer.Write(IntToBytes(ssrc));

                    //writer.BaseStream.Position = SSRC_INDEX + sizeof(int);
                    //writer.Write(packet);
                    byte[] asArray = ms.ToArray();
                    return new DiscordAudioPacket(asArray);
                }
            }
        }

    }
}
