// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
            //const int TRACK_INFO_VERSION = 2;

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
                decoded.Uri = uri != null && version >= 2 ? new Uri(uri) : null;
            }

            return decoded;
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
        private static readonly Encoding _utf8NoBom = new UTF8Encoding();

        public JavaBinaryReader(Stream ms) : base(ms, _utf8NoBom)
        {
        }

        // https://docs.oracle.com/javase/7/docs/api/java/io/DataInput.html#readUTF()
        public string ReadJavaUtf8()
        {
            var length = this.ReadUInt16(); // string size in bytes
            var bytes = new byte[length];
            var amountRead = this.Read(bytes, 0, length);
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
                    output[strlen++] = (char)value1;
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
                    var value2Chop = value2 & 0b00111111;
                    output[strlen++] = (char)(value1Chop | value2Chop);
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
                    var value3Chop = value3 & 0b00111111;
                    output[strlen++] = (char)(value1Chop | value2Chop | value3Chop);
                    continue;
                }
            }

            return new string(output, 0, strlen);
        }

        // https://github.com/sedmelluq/lavaplayer/blob/b0c536098c4f92e6d03b00f19221021f8f50b19b/main/src/main/java/com/sedmelluq/discord/lavaplayer/tools/DataFormatTools.java#L114-L125
        public string ReadNullableString() => this.ReadBoolean() ? this.ReadJavaUtf8() : null;

        // swap endianness
        public override decimal ReadDecimal() => throw new MissingMethodException("This method does not have a Java equivalent");

        // from https://github.com/Zoltu/Zoltu.EndianAwareBinaryReaderWriter under CC0
        public override float ReadSingle() => this.Read(4, BitConverter.ToSingle);

        public override double ReadDouble() => this.Read(8, BitConverter.ToDouble);

        public override short ReadInt16() => this.Read(2, BitConverter.ToInt16);

        public override int ReadInt32() => this.Read(4, BitConverter.ToInt32);

        public override long ReadInt64() => this.Read(8, BitConverter.ToInt64);

        public override ushort ReadUInt16() => this.Read(2, BitConverter.ToUInt16);

        public override uint ReadUInt32() => this.Read(4, BitConverter.ToUInt32);

        public override ulong ReadUInt64() => this.Read(8, BitConverter.ToUInt64);

        private T Read<T>(int size, Func<byte[], int, T> converter) where T : struct
        {
            //Contract.Requires(size >= 0);
            //Contract.Requires(converter != null);

            var bytes = this.GetNextBytesNativeEndian(size);
            return converter(bytes, 0);
        }

        private byte[] GetNextBytesNativeEndian(int count)
        {
            //Contract.Requires(count >= 0);
            //Contract.Ensures(Contract.Result<Byte[]>() != null);
            //Contract.Ensures(Contract.Result<Byte[]>().Length == count);

            var bytes = this.GetNextBytes(count);
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
            var bytesRead = this.BaseStream.Read(buffer, 0, count);

            return bytesRead != count ? throw new EndOfStreamException() : buffer;
        }
    }
}
