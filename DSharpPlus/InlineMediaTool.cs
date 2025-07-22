using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DSharpPlus;

/// <summary>
/// Tool to detect image formats and convert from binary data to base64 strings.
/// </summary>
public sealed class InlineMediaTool : IDisposable
{
    private const int MAX_SIGNATURE_LENGTH = 16;
    private const byte WILDCARD_BYTE = 0x11;

    private record FileSignature(MediaFormat Format, byte[] MagicBytes);

    private static readonly IReadOnlyList<FileSignature> knownSignatures =
    [
        new(MediaFormat.Png,  [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
        new(MediaFormat.Gif,  [0x47, 0x49, 0x46, 0x38, 0x37, 0x61]),
        new(MediaFormat.Gif,  [0x47, 0x49, 0x46, 0x38, 0x39, 0x61]),
        new(MediaFormat.Jpeg, [0xFF, 0xD8, 0xFF, 0xDB]),
        new(MediaFormat.Jpeg, [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01]),
        new(MediaFormat.Jpeg, [0xFF, 0xD8, 0xFF, 0xEE]),
        new(MediaFormat.Jpeg, [0xFF, 0xD8, 0xFF, 0xE1, WILDCARD_BYTE, WILDCARD_BYTE, 0x45, 0x78, 0x69, 0x66, 0x00, 0x00]),
        new(MediaFormat.Jpeg, [0xFF, 0xD8, 0xFF, 0xE0]),
        new(MediaFormat.WebP, [0x52, 0x49, 0x46, 0x46, WILDCARD_BYTE, WILDCARD_BYTE, WILDCARD_BYTE, WILDCARD_BYTE, 0x57, 0x45, 0x42, 0x50]),
        new(MediaFormat.Avif, [0x00, 0x00, 0x00, WILDCARD_BYTE, 0x66, 0x74, 0x79, 0x70, 0x61, 0x76, 0x69, 0x66, 0x00, 0x00, 0x00, 0x00]),
        new(MediaFormat.Ogg,  [0x4F, 0x67, 0x67, 0x53]),
        new(MediaFormat.Mp3,  [0xFF, 0xFB]),
        new(MediaFormat.Mp3,  [0xFF, 0xF3]),
        new(MediaFormat.Mp3,  [0xFF, 0xF2]),
        new(MediaFormat.Mp3,  [0x49, 0x44, 0x33])
    ];

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

        if (stream.Length < MAX_SIGNATURE_LENGTH)
        {
            throw new InvalidDataException("The stream is too short to be valid media data.");
        }

        this.format = MediaFormat.Unknown;
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

        long originalPosition = this.SourceStream.Position;
        this.SourceStream.Seek(0, SeekOrigin.Begin);

        byte[] first16 = ArrayPool<byte>.Shared.Rent(MAX_SIGNATURE_LENGTH);
        this.SourceStream.ReadExactly(first16, 0, MAX_SIGNATURE_LENGTH);

        try
        {
            foreach (FileSignature sig in knownSignatures)
            {
                bool match = true;

                for (int i = 0; i < sig.MagicBytes.Length; i++)
                {
                    if (sig.MagicBytes[i] == WILDCARD_BYTE)
                    {
                        continue;
                    }

                    if (first16[i] != sig.MagicBytes[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return this.format = sig.Format;
                }
            }

            throw new InvalidDataException("The data within the stream was not valid media data.");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(first16);
            this.SourceStream.Seek(originalPosition, SeekOrigin.Begin);
        }
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
