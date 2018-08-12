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
        private static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);

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
            var raw = Convert.FromBase64String(track);

            var msgval = BitConverter.ToInt32(raw, 0);
            msgval = (int)SwapEndianness((uint)msgval);
            var msgFlags = (int)((msgval & 0xC0000000L) >> 30);
            var msgSize = msgval & 0x3FFFFFFF;

            var decoded = new LavalinkTrack
            {
                TrackString = track
            };

            using (var ms = new MemoryStream(msgSize))
            {
                ms.Write(raw, 4, msgSize);
                ms.Position = 0;

                using (var br = new BinaryReader(ms))
                {
                    var version = (msgFlags & 1) == 1 ? br.ReadByte() & 0xFF : 1;

                    var len = br.ReadInt16();
                    len = SwapEndianness(len);
                    raw = br.ReadBytes(len);
                    decoded.Title = UTF8.GetString(raw, 0, len);

                    len = br.ReadInt16();
                    len = SwapEndianness(len);
                    raw = br.ReadBytes(len);
                    decoded.Author = UTF8.GetString(raw, 0, len);

                    decoded._length = (long)SwapEndianness((ulong)br.ReadInt64());

                    len = br.ReadInt16();
                    len = SwapEndianness(len);
                    raw = br.ReadBytes(len);
                    decoded.Identifier = UTF8.GetString(raw, 0, len);

                    decoded.IsStream = br.ReadBoolean();

                    var isthere = br.ReadBoolean();
                    if (isthere)
                    {
                        len = br.ReadInt16();
                        len = SwapEndianness(len);
                        raw = br.ReadBytes(len);
                        decoded.Uri = version >= 2 ? new Uri(UTF8.GetString(raw, 0, len)) : null;
                    }
                }
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
}
