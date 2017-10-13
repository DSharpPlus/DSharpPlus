using System;
using System.IO;
using System.Text;

namespace DSharpPlus
{
    /// <summary>
    /// Tool to detect image formats and convert from binary data to base64 strings.
    /// </summary>
    public sealed class ImageTool : IDisposable
    {
        private const ulong PngMagic = 0x0A1A_0A0D_474E_5089;
        private const ushort JpegMagic1 = 0xD8FF;
        private const ushort JpegMagic2 = 0xD9FF;
        private const ulong GifMagic1 = 0x0000_6139_3846_4947;
        private const ulong GifMagic2 = 0x0000_6137_3846_4947;
        private const uint WebpMagic1 = 0x4646_4952;
        private const uint WebpMagic2 = 0x5042_4557;

        private const ulong GifMask = 0x0000_FFFF_FFFF_FFFF;
        private const ulong Mask32 = 0x0000_0000_FFFF_FFFF;
        private const uint Mask16 = 0x0000_FFFF;

        /// <summary>
        /// Gets the stream this tool is operating on.
        /// </summary>
        private Stream SourceStream { get; }

        private ImageFormat _ifcache;
        private string _b64Cache;

        /// <summary>
        /// Creates a new image tool from given stream.
        /// </summary>
        /// <param name="stream">Stream to work with.</param>
        public ImageTool(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead || !stream.CanSeek)
            {
                throw new ArgumentException("The stream needs to be both readable and seekable.", nameof(stream));
            }

            SourceStream = stream;
            SourceStream.Seek(0, SeekOrigin.Begin);

            _ifcache = 0;
            _b64Cache = null;
        }

        /// <summary>
        /// Detects the format of this image.
        /// </summary>
        /// <returns>Detected format.</returns>
        private ImageFormat GetFormat()
        {
            if (_ifcache != ImageFormat.Unknown)
            {
                return _ifcache;
            }

            using (var br = new BinaryReader(SourceStream, new UTF8Encoding(false), true))
            {
                var bgn64 = br.ReadUInt64();
                if (bgn64 == PngMagic)
                {
                    return _ifcache = ImageFormat.Png;
                }

                bgn64 &= GifMask;
                if (bgn64 == GifMagic1 || bgn64 == GifMagic2)
                {
                    return _ifcache = ImageFormat.Gif;
                }

                var bgn32 = (uint)(bgn64 & Mask32);
                if (bgn32 == WebpMagic1 && br.ReadUInt32() == WebpMagic2)
                {
                    return _ifcache = ImageFormat.WebP;
                }

                var bgn16 = (ushort)(bgn32 & Mask16);
                if (bgn16 == JpegMagic1)
                {
                    SourceStream.Seek(-2, SeekOrigin.End);
                    if (br.ReadUInt16() == JpegMagic2)
                    {
                        return _ifcache = ImageFormat.Jpeg;
                    }
                }
            }

            throw new InvalidDataException("The data within the stream was not valid image data.");
        }

        /// <summary>
        /// Converts this image into base64 data format string.
        /// </summary>
        /// <returns>Data-scheme base64 string.</returns>
        public string GetBase64()
        {
            if (_b64Cache != null)
            {
                return _b64Cache;
            }

            var fmt = GetFormat();
            var sb = new StringBuilder();

            sb.Append("data:image/")
                .Append(fmt.ToString().ToLowerInvariant())
                .Append(";base64,");

            SourceStream.Seek(0, SeekOrigin.Begin);
            var buff = new byte[SourceStream.Length];
            var br = 0;
            while (br < buff.Length)
            {
                br += SourceStream.Read(buff, br, (int)SourceStream.Length - br);
            }

            sb.Append(Convert.ToBase64String(buff));

            return _b64Cache = sb.ToString();
        }

        /// <summary>
        /// Disposes this image tool.
        /// </summary>
        public void Dispose()
        {
            SourceStream?.Dispose();
        }
    }

    /// <summary>
    /// Represents format of an image.
    /// </summary>
    public enum ImageFormat
    {
        Unknown = 0,
        Jpeg = 1,
        Png = 2,
        Gif = 3,
        WebP = 4
    }
}
