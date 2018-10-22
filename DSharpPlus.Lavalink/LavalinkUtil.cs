﻿using System;
using System.IO;
using System.Text;
using DSharpPlus.Lavalink.EventArgs;

namespace DSharpPlus.Lavalink
{
    /// <summary>
    /// Various utilities for Lavalink.
    /// </summary>
    public static class LavalinkUtilities
    {
        /// <summary>
        /// Indicates whether a new track should be started after reciving this TrackEndReason. If this is false, either this event is
        /// already triggered because another track started (REPLACED) or because the player is stopped (STOPPED, CLEANUP).
        /// </summary>
        public static bool MayStartNext(this TrackEndReason reason)
            => reason == TrackEndReason.Finished || reason == TrackEndReason.LoadFailed;
        
        /// <summary>
        /// Decodes a Lavalink track string.
        /// </summary>
        /// <param name="track">Track string to decode.</param>
        /// <returns>Decoded Lavalink track.</returns>
        public static LavalinkTrack DecodeTrack(string track)
        {
            // https://github.com/sedmelluq/lavaplayer/blob/804cd1038229230052d9b1dee5e6d1741e30e284/main/src/main/java/com/sedmelluq/discord/lavaplayer/player/DefaultAudioPlayerManager.java#L63-L64
            const int TRACK_INFO_VERSIONED = 1;
            const int TRACK_INFO_VERSION = 2;

            var raw = Convert.FromBase64String(track);

            var decoded = new LavalinkTrack
            {
                TrackString = track
            };
            
            using (var ms = new MemoryStream(raw))
            using (var br = new JavaBinaryReader(ms))
            {
                // https://github.com/sedmelluq/lavaplayer/blob/b0c536098c4f92e6d03b00f19221021f8f50b19b/main/src/main/java/com/sedmelluq/discord/lavaplayer/tools/io/MessageInput.java#L37-L39
                var messageHeader = br.ReadInt32();
                var messageFlags = (int) ((messageHeader & 0xC0000000L) >> 30);
                var messageSize = messageHeader & 0x3FFFFFFF;
                //if (messageSize != raw.Length)
                //    Warn($"Size conflict: {messageSize} but was {raw.Length}");
                
                // https://github.com/sedmelluq/lavaplayer/blob/804cd1038229230052d9b1dee5e6d1741e30e284/main/src/main/java/com/sedmelluq/discord/lavaplayer/player/DefaultAudioPlayerManager.java#L268

                // java bytes are signed
                // https://docs.oracle.com/javase/7/docs/api/java/io/DataInput.html#readByte()
                var version = (messageFlags & TRACK_INFO_VERSIONED) != 0 ? (br.ReadSByte() & 0xFF) : 1;
                //if (version != TRACK_INFO_VERSION)
                //    Warn($"Version conflict: Expected {TRACK_INFO_VERSION} but got {version}");

                decoded.Title = br.ReadJavaUtf8();

                decoded.Author = br.ReadJavaUtf8();

                decoded._length = br.ReadInt64();

                decoded.Identifier = br.ReadJavaUtf8();

                decoded.IsStream = br.ReadBoolean();

                var uri = br.ReadNullableString();
                if (uri != null && version >= 2)
                    decoded.Uri = new Uri(uri);
                else
                    decoded.Uri = null;
            }

            return decoded;
        }

        private static uint SwapEndianness(uint v)
        {
            v = (v << 16) | (v >> 16);
            return ((v & 0xFF00FF00) >> 8 | ((v & 0x00FF00FF) << 8));
        }

        private static short SwapEndianness(short v)
        {
            return (short)((v << 8) | (v >> 8));
        }

        private static ulong SwapEndianness(ulong v)
        {
            v = (v >> 32) | (v << 32);
            v = ((v & 0xFFFF0000FFFF0000) >> 16) | ((v & 0x0000FFFF0000FFFF) << 16);
            return ((v & 0xFF00FF00FF00FF00) >> 8) | ((v & 0x00FF00FF00FF00FF) << 8);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Java's DataOutputStream always uses big-endian, while BinaryReader always uses little-endian.
    /// This class converts a big-endian stream to little-endian, and includes some helper methods
    /// for interacting with Lavaplayer/Lavalink.
    /// </summary>
    internal class JavaBinaryReader : BinaryReader
    {
        private static readonly Encoding Utf8NoBom = new UTF8Encoding();

        public JavaBinaryReader(Stream ms) : base(ms, Utf8NoBom)
        {
        }

        // https://docs.oracle.com/javase/7/docs/api/java/io/DataInput.html#readUTF()
        public string ReadJavaUtf8()
        {
            var length = ReadUInt16(); // string size in bytes
            var bytes = new byte[length];
            var amountRead = Read(bytes, 0, length);
            if (amountRead < length)
                throw new InvalidDataException("EOS unexpected");

            var output = new char[length];
            var strlen = 0;

            // i'm gonna blindly assume here that the javadocs had the correct endianness

            for (var i = 0; i < length; i++)
            {
                var value1 = bytes[i];
                if ((value1 & 0b10000000) == 0) // highest bit 1 is false
                {
                    output[strlen++] = (char) value1;
                    continue;
                }

                // remember to skip one byte for every extra byte
                var value2 = bytes[++i];
                if ((value1 & 0b00100000) == 0 && // highest bit 3 is false
                    (value1 & 0b11000000) != 0 && // highest bit 1 and 2 are true
                    (value2 & 0b01000000) == 0 && // highest bit 2 is false
                    (value2 & 0b10000000) != 0) //   highest bit 1 is true
                {
                    var value1Chop = (value1 & 0b00011111) << 6;
                    var value2Chop = (value2 & 0b00111111);
                    output[strlen++] = (char) (value1Chop | value2Chop);
                    continue;
                }
                
                var value3 = bytes[++i];
                if ((value1 & 0b00010000) == 0 && // highest bit 4 is false
                    (value1 & 0b11100000) != 0 && // highest bit 1,2,3 are true
                    (value2 & 0b01000000) == 0 && // highest bit 2 is false
                    (value2 & 0b10000000) != 0 && // highest bit 1 is true
                    (value3 & 0b01000000) == 0 && // highest bit 2 is false
                    (value3 & 0b10000000) != 0) //   highest bit 1 is true
                {
                    var value1Chop = (value1 & 0b00001111) << 12;
                    var value2Chop = (value2 & 0b00111111) << 6;
                    var value3Chop = (value3 & 0b00111111);
                    output[strlen++] = (char) (value1Chop | value2Chop | value3Chop);
                    continue;
                }
            }
            
            return new string(output, 0, strlen);
        }

        // https://github.com/sedmelluq/lavaplayer/blob/b0c536098c4f92e6d03b00f19221021f8f50b19b/main/src/main/java/com/sedmelluq/discord/lavaplayer/tools/DataFormatTools.java#L114-L125
        public string ReadNullableString()
        {
            return ReadBoolean() ? ReadJavaUtf8() : null;
        }

        // swap endianness
        public override decimal ReadDecimal()
        {
            throw new MissingMethodException("This method does not have a Java equivalent");
        }

        // from https://github.com/Zoltu/Zoltu.EndianAwareBinaryReaderWriter under CC0
        public override float ReadSingle() => Read(4, BitConverter.ToSingle);

        public override double ReadDouble() => Read(8, BitConverter.ToDouble);

        public override short ReadInt16() => Read(2, BitConverter.ToInt16);

        public override int ReadInt32() => Read(4, BitConverter.ToInt32);

        public override long ReadInt64() => Read(8, BitConverter.ToInt64);

        public override ushort ReadUInt16() => Read(2, BitConverter.ToUInt16);

        public override uint ReadUInt32() => Read(4, BitConverter.ToUInt32);

        public override ulong ReadUInt64() => Read(8, BitConverter.ToUInt64);

        private T Read<T>(int size, Func<byte[], int, T> converter) where T : struct
        {
            //Contract.Requires(size >= 0);
            //Contract.Requires(converter != null);

            var bytes = GetNextBytesNativeEndian(size);
            return converter(bytes, 0);
        }

        private byte[] GetNextBytesNativeEndian(int count)
        {
            //Contract.Requires(count >= 0);
            //Contract.Ensures(Contract.Result<Byte[]>() != null);
            //Contract.Ensures(Contract.Result<Byte[]>().Length == count);

            var bytes = GetNextBytes(count);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        private byte[] GetNextBytes(int count)
        {
            //Contract.Requires(count >= 0);
            //Contract.Ensures(Contract.Result<Byte[]>() != null);
            //Contract.Ensures(Contract.Result<Byte[]>().Length == count);

            var buffer = new byte[count];
            var bytesRead = BaseStream.Read(buffer, 0, count);

            if (bytesRead != count)
                throw new EndOfStreamException();

            return buffer;
        }
    }
}
