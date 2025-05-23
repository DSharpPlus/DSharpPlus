// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0072

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus;

/// <summary>
/// Represents an image sent to Discord as part of JSON payloads.
/// </summary>
public readonly record struct InlineMediaData
{
    private static ReadOnlySpan<byte> PngString => "data:image/png;base64,"u8;
    private static ReadOnlySpan<byte> JpegString => "data:image/jpeg;base64,"u8;
    private static ReadOnlySpan<byte> GifString => "data:image/gif;base64,"u8;
    private static ReadOnlySpan<byte> WebpString => "data:image/webp;base64,"u8;
    private static ReadOnlySpan<byte> AvifString => "data:image/avif;base64,"u8;
    private static ReadOnlySpan<byte> OggString => "data:audio/ogg;base64,"u8;
    private static ReadOnlySpan<byte> Mp3String => "data:audio/mpeg;base64,"u8;
    private static ReadOnlySpan<byte> AutoString => "data:image/auto;base64,"u8;

    private readonly Stream reader;
    private readonly MediaFormat format;

    /// <summary>
    /// Creates a new instance of this struct from the provided stream.
    /// </summary>
    /// <param name="reader">The Stream to convert to base64.</param>
    /// <param name="format">The format of this image.</param>
    public InlineMediaData(Stream reader, MediaFormat format)
    {
        this.reader = reader;
        this.format = format;
    }

    /// <summary>
    /// Creates a new instance of this struct from the provided pipe.
    /// </summary>
    /// <param name="reader">The pipe to conver to base64.</param>
    /// <param name="format">The format of this image.</param>
    public InlineMediaData(PipeReader reader, MediaFormat format) : this(reader.AsStream(), format)
    {

    }

    /// <summary>
    /// Creates a new instance of this struct from the provided buffer.
    /// </summary>
    /// <param name="data">The buffer to convert to base64.</param>
    /// <param name="format">The format of this image.</param>
    public InlineMediaData(ReadOnlySequence<byte> data, MediaFormat format) : this(PipeReader.Create(data), format)
    {

    }

    /// <summary>
    /// Writes the base64 data to the specified array pool buffer writer.
    /// </summary>
    public readonly void WriteTo(ArrayPoolBufferWriter<byte> writer)
    {
        // chosen because a StreamPipeReader buffers to 4096
        const int readSegmentLength = 12288;
        const int writeSegmentLength = 16384;

        writer.Write
        (
            this.format switch
            {
                MediaFormat.Png => PngString,
                MediaFormat.Gif => GifString,
                MediaFormat.Jpeg => JpegString,
                MediaFormat.WebP => WebpString,
                MediaFormat.Avif => AvifString,
                MediaFormat.Ogg => OggString,
                MediaFormat.Mp3 => Mp3String,
                _ => AutoString
            }
        );

        byte[] readBuffer = ArrayPool<byte>.Shared.Rent(readSegmentLength);
        byte[] writeBuffer = ArrayPool<byte>.Shared.Rent(writeSegmentLength);

        scoped Span<byte> readSpan = readBuffer.AsSpan()[..readSegmentLength];
        scoped Span<byte> writeSpan = writeBuffer.AsSpan()[..writeSegmentLength];

        int readRollover = 0;

        while (true)
        {
            int read = this.reader.Read(readSpan[readRollover..]);
            int currentLength = read + readRollover;

            if (read == 0)
            {
                break;
            }

            OperationStatus status = Base64.EncodeToUtf8
            (
                bytes: readSpan[..currentLength],
                utf8: writeSpan,
                bytesConsumed: out int consumed,
                bytesWritten: out int written,
                isFinalBlock: false
            );

            Debug.Assert(status is OperationStatus.Done or OperationStatus.NeedMoreData);
            Debug.Assert(read - consumed < 3);

            writer.Write(writeSpan[..written]);

            readSpan[consumed..currentLength].CopyTo(readSpan[0..]);
            readRollover = currentLength - consumed;
        }

        OperationStatus lastStatus = Base64.EncodeToUtf8(readSpan[..readRollover], writeSpan, out int _, out int lastWritten);

        Debug.Assert(lastStatus == OperationStatus.Done);

        writer.Write(writeSpan[..lastWritten]);

        ArrayPool<byte>.Shared.Return(readBuffer);
        ArrayPool<byte>.Shared.Return(writeBuffer);
    }
}
