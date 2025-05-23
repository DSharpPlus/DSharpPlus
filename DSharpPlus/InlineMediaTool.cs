using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO;
using System.Text;

namespace DSharpPlus;

/// <summary>
/// Tool to detect image formats and convert from binary data to base64 strings.
/// </summary>
public sealed class InlineMediaTool : IDisposable
{
    private const ulong PNG_MAGIC = 0x0A1A_0A0D_474E_5089;
    private const ushort JPEG_MAGIC_1 = 0xD8FF;
    private const ushort JPEG_MAGIC_2 = 0xD9FF;
    private const ulong GIF_MAGIC_1 = 0x0000_6139_3846_4947;
    private const ulong GIF_MAGIC_2 = 0x0000_6137_3846_4947;
    private const uint WEBP_MAGIC_1 = 0x4646_4952;
    private const uint WEBP_MAGIC_2 = 0x5042_4557;
    private const ulong AVIF_MAGIC_PART_1 = 0x7079_7466_0000_0000;
    private const uint AVIF_MAGIC_PART_2 = 0x6669_7661;
    private const uint OGG_MAGIC = 0x5367_674F;
    private const ushort MP3_MAGIC_1 = 0xFBFF;
    private const ushort MP3_MAGIC_2 = 0xF3FF;
    private const ushort MP3_MAGIC_3 = 0xF2FF;

    // this one is stupid and only 24 bits are assigned.
    private const uint MP3_MAGIC_4 = 0x0033_4499;

    private const ulong GIF_MASK = 0x0000_FFFF_FFFF_FFFF;
    private const ulong MASK32 = 0x0000_0000_FFFF_FFFF;
    private const ulong MASK32_LESSER = 0xFFFF_FFFF_0000_0000;
    private const uint MASK16 = 0x0000_FFFF;
    private const uint MASK24 = 0x00FF_FFFF;

    /// <summary>
    /// Gets the stream this tool is operating on.
    /// </summary>
    public Stream SourceStream { get; }

    private MediaFormat format;

    /// <summary>
    /// Creates a new media tool from given stream.
    /// </summary>
    /// <param name="stream">Stream to work with.</param>
    public InlineMediaTool(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead || !stream.CanSeek)
        {
            throw new ArgumentException("The stream needs to be both readable and seekable.", nameof(stream));
        }

        this.SourceStream = stream;
        this.SourceStream.Seek(0, SeekOrigin.Begin);

        this.format = 0;
    }

    /// <summary>
    /// Detects the format of this media item.
    /// </summary>
    /// <returns>Detected format.</returns>
    public MediaFormat GetFormat()
    {
        if (this.format != MediaFormat.Unknown)
        {
            return this.format;
        }

        using (BinaryReader br = new(this.SourceStream, Utilities.UTF8, true))
        {
            ulong bgn64 = br.ReadUInt64();

            if (bgn64 == PNG_MAGIC)
            {
                return this.format = MediaFormat.Png;
            }

            if ((bgn64 & MASK32_LESSER) == AVIF_MAGIC_PART_1 && br.ReadUInt32() == AVIF_MAGIC_PART_2)
            {
                return this.format = MediaFormat.Avif;
            }

            bgn64 &= GIF_MASK;

            if (bgn64 is GIF_MAGIC_1 or GIF_MAGIC_2)
            {
                return this.format = MediaFormat.Gif;
            }

            uint bgn32 = (uint)(bgn64 & MASK32);

            if (bgn32 == WEBP_MAGIC_1 && br.ReadUInt32() == WEBP_MAGIC_2)
            {
                return this.format = MediaFormat.WebP;
            }

            if (bgn32 == OGG_MAGIC)
            {
                return this.format = MediaFormat.Ogg;
            }

            uint bgn24 = bgn32 & MASK24;

            if (bgn24 == MP3_MAGIC_4)
            {
                return this.format = MediaFormat.Mp3;
            }

            ushort bgn16 = (ushort)(bgn32 & MASK16);

            if (bgn16 == JPEG_MAGIC_1)
            {
                this.SourceStream.Seek(-2, SeekOrigin.End);

                if (br.ReadUInt16() == JPEG_MAGIC_2)
                {
                    return this.format = MediaFormat.Jpeg;
                }
            }

            if (bgn16 is MP3_MAGIC_1 or MP3_MAGIC_2 or MP3_MAGIC_3)
            {
                return this.format = MediaFormat.Mp3;
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
        const int readLength = 12288;
        const int writeLength = 16384;

        MediaFormat fmt = GetFormat();

        int contentLength = Base64.GetMaxEncodedToUtf8Length((int)this.SourceStream.Length);

        int formatLength = fmt.ToString().Length;

        byte[] b64Buffer = ArrayPool<byte>.Shared.Rent(formatLength + contentLength + 19);
        byte[] readBufferBacking = ArrayPool<byte>.Shared.Rent(readLength);

        Span<byte> readBuffer = readBufferBacking.AsSpan()[..readLength];

        int processed = 0;
        int totalWritten = 0;

        (fmt switch
        {
            MediaFormat.Png => "data:image/png;base64,"u8,
            MediaFormat.Jpeg => "data:image/jpeg;base64,"u8,
            MediaFormat.Gif => "data:image/gif;base64,"u8,
            MediaFormat.WebP => "data:image/webp;base64,"u8,
            MediaFormat.Avif => "data:image/avif;base64,"u8,
            MediaFormat.Ogg => "data:audio/ogg;base64,"u8,
            MediaFormat.Mp3 => "data:audio/mp3;base64,"u8,
            MediaFormat.Auto => "data:image/auto;base64,"u8,
            _ => "data:image/unknown;base64"u8
        }).CopyTo(b64Buffer);

        totalWritten += 19;
        totalWritten += formatLength;

        while (processed < this.SourceStream.Length - readLength)
        {
            this.SourceStream.ReadExactly(readBuffer);

            Base64.EncodeToUtf8(readBuffer, b64Buffer.AsSpan().Slice(totalWritten, writeLength), out int _, out int written, false);

            processed += readLength;
            totalWritten += written;
        }

        int remainingLength = (int)this.SourceStream.Length - processed;

        this.SourceStream.ReadExactly(readBufferBacking, 0, remainingLength);

        Base64.EncodeToUtf8(readBufferBacking.AsSpan()[..remainingLength], b64Buffer.AsSpan()[totalWritten..], out int _, out int lastWritten);

        string value = Encoding.UTF8.GetString(b64Buffer.AsSpan()[..(totalWritten + lastWritten)]);

        ArrayPool<byte>.Shared.Return(b64Buffer);
        ArrayPool<byte>.Shared.Return(readBufferBacking);

        return value;
    }

    /// <summary>
    /// Disposes this media tool.
    /// </summary>
    public void Dispose() => this.SourceStream?.Dispose();
}

/// <summary>
/// Represents format of an inline media item.
/// </summary>
public enum MediaFormat : int
{
    Png,
    Gif,
    Jpeg,
    WebP,
    Avif,
    Ogg,
    Mp3,
    Auto,
    Unknown,
}
