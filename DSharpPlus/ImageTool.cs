using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO;
using System.Text;

namespace DSharpPlus;

/// <summary>
/// Tool to detect image formats and convert from binary data to base64 strings.
/// </summary>
public sealed class ImageTool : IDisposable
{
    private const ulong PNG_MAGIC = 0x0A1A_0A0D_474E_5089;
    private const ushort JPEG_MAGIC_1 = 0xD8FF;
    private const ushort JPEG_MAGIC_2 = 0xD9FF;
    private const ulong GIF_MAGIC_1 = 0x0000_6139_3846_4947;
    private const ulong GIF_MAGIC_2 = 0x0000_6137_3846_4947;
    private const uint WEBP_MAGIC_1 = 0x4646_4952;
    private const uint WEBP_MAGIC_2 = 0x5042_4557;

    private const ulong GIF_MASK = 0x0000_FFFF_FFFF_FFFF;
    private const ulong MASK32 = 0x0000_0000_FFFF_FFFF;
    private const uint MASK16 = 0x0000_FFFF;

    /// <summary>
    /// Gets the stream this tool is operating on.
    /// </summary>
    public Stream SourceStream { get; }

    private ImageFormat format;

    /// <summary>
    /// Creates a new image tool from given stream.
    /// </summary>
    /// <param name="stream">Stream to work with.</param>
    public ImageTool(Stream stream)
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
    /// Detects the format of this image.
    /// </summary>
    /// <returns>Detected format.</returns>
    public ImageFormat GetFormat()
    {
        if (this.format != ImageFormat.Unknown)
        {
            return this.format;
        }

        using (BinaryReader br = new(this.SourceStream, Utilities.UTF8, true))
        {
            ulong bgn64 = br.ReadUInt64();
            if (bgn64 == PNG_MAGIC)
            {
                return this.format = ImageFormat.Png;
            }

            bgn64 &= GIF_MASK;
            if (bgn64 == GIF_MAGIC_1 || bgn64 == GIF_MAGIC_2)
            {
                return this.format = ImageFormat.Gif;
            }

            uint bgn32 = (uint)(bgn64 & MASK32);
            if (bgn32 == WEBP_MAGIC_1 && br.ReadUInt32() == WEBP_MAGIC_2)
            {
                return this.format = ImageFormat.WebP;
            }

            ushort bgn16 = (ushort)(bgn32 & MASK16);
            if (bgn16 == JPEG_MAGIC_1)
            {
                this.SourceStream.Seek(-2, SeekOrigin.End);
                if (br.ReadUInt16() == JPEG_MAGIC_2)
                {
                    return this.format = ImageFormat.Jpeg;
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
        ImageFormat fmt = this.GetFormat();

        int contentLength = Base64.GetMaxEncodedToUtf8Length((int)this.SourceStream.Length);

        int formatLength = (int)fmt switch
        {
            < 2 => 3,
            < 5 => 4,
            _ => 7
        };

        byte[] b64Buffer = ArrayPool<byte>.Shared.Rent(formatLength + contentLength + 19);
        byte[] readBuffer = ArrayPool<byte>.Shared.Rent(3072);

        int processed = 0;
        int totalWritten = 0;

        "data:image/"u8.CopyTo(b64Buffer);
        Encoding.UTF8.GetBytes(fmt.ToString().ToLowerInvariant()).CopyTo(b64Buffer, 11);
        ";base64,"u8.CopyTo(b64Buffer.AsSpan()[(11 + formatLength)..]);

        totalWritten += 19;
        totalWritten += formatLength;

        while (processed < this.SourceStream.Length - 3072)
        {
            this.SourceStream.Read(readBuffer);

            Base64.EncodeToUtf8(readBuffer, b64Buffer.AsSpan().Slice(totalWritten, 4096), out int _, out int written, false);

            processed += 3072;
            totalWritten += written;
        }

        int remainingLength = (int)this.SourceStream.Length - processed;

        this.SourceStream.Read(readBuffer, 0, remainingLength);

        Base64.EncodeToUtf8(readBuffer.AsSpan()[..remainingLength], b64Buffer.AsSpan()[totalWritten..], out int _, out int lastWritten);

        string value = Encoding.UTF8.GetString(b64Buffer.AsSpan()[..(totalWritten + lastWritten)]);

        ArrayPool<byte>.Shared.Return(b64Buffer);
        ArrayPool<byte>.Shared.Return(readBuffer);

        return value;
    }

    /// <summary>
    /// Disposes this image tool.
    /// </summary>
    public void Dispose() => this.SourceStream?.Dispose();
}

/// <summary>
/// Represents format of an image.
/// </summary>
public enum ImageFormat : int
{
    Png,
    Gif,
    Jpeg,
    WebP,
    Auto,
    Unknown,
}
