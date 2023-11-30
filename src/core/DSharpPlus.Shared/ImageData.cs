// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus;

/// <summary>
/// Represents an image sent to Discord as part of JSON payloads.
/// </summary>
public readonly record struct ImageData
{
    private static ReadOnlySpan<byte> PngString => "data:image/png;base64,"u8;
    private static ReadOnlySpan<byte> JpegString => "data:image/jpeg;base64,"u8;
    private static ReadOnlySpan<byte> GifString => "data:image/gif;base64,"u8;
    private static ReadOnlySpan<byte> WebpString => "data:image/webp;base64,"u8;
    private static ReadOnlySpan<byte> AutoString => "data:image/auto;base64,"u8;

    private readonly PipeReader reader;
    private readonly ImageFormat format;

    /// <summary>
    /// Creates a new instance of this struct from the provided pipe.
    /// </summary>
    /// <param name="reader">The PipeReader to convert to base64.</param>
    /// <param name="format">The format of this image.</param>
    public ImageData(PipeReader reader, ImageFormat format)
    {
        this.reader = reader;
        this.format = format;
    }

    /// <summary>
    /// Creates a new instance of this struct from the provided stream.
    /// </summary>
    /// <param name="stream">The stream to conver to base64.</param>
    /// <param name="format">The format of this image.</param>
    public ImageData(Stream stream, ImageFormat format) : this(PipeReader.Create(stream), format)
    {

    }

    /// <summary>
    /// Creates a new instance of this struct from the provided buffer.
    /// </summary>
    /// <param name="data">The buffer to convert to base64.</param>
    /// <param name="format">The format of this image.</param>
    public ImageData(ReadOnlySequence<byte> data, ImageFormat format) : this(PipeReader.Create(data), format)
    {

    }

    /// <summary>
    /// Writes the base64 data to the specified PipeWriter.
    /// </summary>
    public readonly async ValueTask WriteToAsync(PipeWriter writer)
    {
        // chosen because a StreamPipeReader buffers to 4096
        const int readSegmentLength = 4096;
        const int writeSegmentLength = 5120;

        writer.Write
        (
            this.format switch
            {
                ImageFormat.Png => PngString,
                ImageFormat.Gif => GifString,
                ImageFormat.Jpeg => JpegString,
                ImageFormat.WebP => WebpString,
                _ => AutoString
            }
        );

        byte[] readBuffer = ArrayPool<byte>.Shared.Rent(readSegmentLength);
        byte[] writeBuffer = ArrayPool<byte>.Shared.Rent(writeSegmentLength);

        int readRollover = 0;

        while (true)
        {
            ReadResult result = await this.reader.ReadAsync();

            if (result.IsCanceled)
            {
                break;
            }

            ProcessResult(result, this.reader);

            if (result.IsCompleted)
            {
                break;
            }
        }

        ArrayPool<byte>.Shared.Return(readBuffer);
        ArrayPool<byte>.Shared.Return(writeBuffer);

        void ProcessResult(ReadResult result, PipeReader reader)
        {
            SequenceReader<byte> sequence = new(result.Buffer);

            scoped Span<byte> readSpan = readBuffer.AsSpan()[..readSegmentLength];
            scoped Span<byte> writeSpan = writeBuffer.AsSpan()[..writeSegmentLength];

            while (!sequence.End)
            {
                if (readRollover != 0)
                {
                    if (sequence.Remaining + readRollover >= readSegmentLength)
                    {
                        sequence.TryCopyTo(readSpan[readRollover..]);

                        sequence.Advance(readSegmentLength - readRollover);
                        readRollover = 0;
                    }
                    else
                    {
                        sequence.TryCopyTo(readSpan.Slice(readRollover, (int)sequence.Remaining));

                        readRollover += (int)sequence.Remaining;
                        sequence.Advance((int)sequence.Remaining);
                        break;
                    }
                }
                else if (sequence.Remaining >= readSegmentLength)
                {
                    sequence.TryCopyTo(readSpan);

                    sequence.Advance(readSegmentLength);
                }
                else
                {
                    sequence.TryCopyTo(readSpan[..(int)sequence.Remaining]);

                    readRollover += (int)sequence.Remaining;
                    sequence.AdvanceToEnd();
                    break;
                }

                OperationStatus status = Base64.EncodeToUtf8(readSpan, writeSpan, out int consumed, out int written, false);

                Debug.Assert
                (
                    consumed != readSegmentLength || written != writeSegmentLength,
                    "Buffer management error while converting to base64. Aborting."
                );

                Debug.Assert(status == OperationStatus.Done);

                writer.Write(writeSpan[..written]);
            }

            if (result.IsCompleted)
            {
                OperationStatus status = Base64.EncodeToUtf8(readSpan[..readRollover], writeSpan, out int _, out int written, true);

                Debug.Assert(status == OperationStatus.Done);

                writer.Write(writeSpan[..written]);

                writer.Complete();
                reader.Complete();
                return;
            }

            reader.AdvanceTo(result.Buffer.End);
        }
    }

    /// <summary>
    /// Writes the base64 data to the specified ArrayPoolBufferWriter.
    /// </summary>
    public readonly async ValueTask WriteToAsync(ArrayPoolBufferWriter<byte> writer)
    {
        const int readSegmentLength = 4096;
        const int writeSegmentLength = 5120;

        writer.Write
        (
            this.format switch
            {
                ImageFormat.Png => PngString,
                ImageFormat.Gif => GifString,
                ImageFormat.Jpeg => JpegString,
                ImageFormat.WebP => WebpString,
                _ => AutoString
            }
        );

        byte[] readBuffer = ArrayPool<byte>.Shared.Rent(readSegmentLength);
        byte[] writeBuffer = ArrayPool<byte>.Shared.Rent(writeSegmentLength);

        int readRollover = 0;

        while (true)
        {
            ReadResult result = await this.reader.ReadAsync();

            if (result.IsCanceled)
            {
                break;
            }

            ProcessResult(result, this.reader);

            if (result.IsCompleted)
            {
                break;
            }
        }

        ArrayPool<byte>.Shared.Return(readBuffer);
        ArrayPool<byte>.Shared.Return(writeBuffer);

        void ProcessResult(ReadResult result, PipeReader reader)
        {
            SequenceReader<byte> sequence = new(result.Buffer);

            scoped Span<byte> readSpan = readBuffer.AsSpan()[..readSegmentLength];
            scoped Span<byte> writeSpan = writeBuffer.AsSpan()[..writeSegmentLength];

            while (!sequence.End)
            {
                if (readRollover != 0)
                {
                    if (sequence.Remaining + readRollover >= readSegmentLength)
                    {
                        sequence.TryCopyTo(readSpan[readRollover..]);

                        sequence.Advance(readSegmentLength - readRollover);
                        readRollover = 0;
                    }
                    else
                    {
                        sequence.TryCopyTo(readSpan.Slice(readRollover, (int)sequence.Remaining));

                        readRollover += (int)sequence.Remaining;
                        sequence.Advance((int)sequence.Remaining);
                        break;
                    }
                }
                else if (sequence.Remaining >= readSegmentLength)
                {
                    sequence.TryCopyTo(readSpan);

                    sequence.Advance(readSegmentLength);
                }
                else
                {
                    sequence.TryCopyTo(readSpan[..(int)sequence.Remaining]);

                    readRollover += (int)sequence.Remaining;
                    sequence.AdvanceToEnd();
                    break;
                }

                OperationStatus status = Base64.EncodeToUtf8(readSpan, writeSpan, out int consumed, out int written, false);

                Debug.Assert
                (
                    consumed != readSegmentLength || written != writeSegmentLength,
                    "Buffer management error while converting to base64. Aborting."
                );

                Debug.Assert(status == OperationStatus.Done);

                writer.Write(writeSpan[..written]);
            }

            if (result.IsCompleted)
            {
                OperationStatus status = Base64.EncodeToUtf8(readSpan[..readRollover], writeSpan, out int _, out int written, true);

                Debug.Assert(status == OperationStatus.Done);

                writer.Write(writeSpan[..written]);

                reader.Complete();
                return;
            }

            reader.AdvanceTo(result.Buffer.End);
        }
    }
}
