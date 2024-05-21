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

namespace DSharpPlus;

/// <summary>
/// Represents a file sent to Discord as a message attachment.
/// </summary>
public readonly record struct AttachmentData
{
    private readonly Stream stream;

    /// <summary>
    /// The media type of this attachment. Defaults to empty, in which case it will be interpreted according to
    /// the file extension as provided by <see cref="Filename"/>.
    /// </summary>
    public string? MediaType { get; init; }

    /// <summary>
    /// The filename of this attachment.
    /// </summary>
    public required string Filename { get; init; }

    /// <summary>
    /// Specifies whether to encode the attachment as base64.
    /// </summary>
    public bool ConvertToBase64 { get; init; }

    /// <summary>
    /// Creates a new instance of this structure from the provided stream.
    /// </summary>
    /// <param name="stream">A stream containing the attachment.</param>
    /// <param name="filename">The name of this file to upload.</param>
    /// <param name="mediaType">The media type of this file.</param>
    /// <param name="base64">Whether to upload the attachment as base64.</param>
    public AttachmentData
    (
        Stream stream,
        string filename,
        string? mediaType = null,
        bool base64 = false
    )
    {
        this.stream = stream;
        this.MediaType = mediaType;
        this.Filename = filename;
        this.ConvertToBase64 = base64;
    }

    /// <summary>
    /// Creates a new instance of this structure from the provided pipe.
    /// </summary>
    /// <param name="reader">A reader to the pipe.</param>
    /// <param name="filename">The name of this file to upload.</param>
    /// <param name="mediaType">The media type of this file.</param>
    /// <param name="base64">Whether to upload the attachment as base64.</param>
    public AttachmentData
    (
        PipeReader reader,
        string filename,
        string? mediaType = null,
        bool base64 = false
    )
    : this
    (
          reader.AsStream(),
          filename,
          mediaType,
          base64
    )
    {

    }

    public async ValueTask<Stream> GetStreamAsync()
    {
        if (this.ConvertToBase64)
        {
            Pipe pipe = new();
            await this.WriteToPipeAsBase64Async(pipe.Writer);

            return pipe.Reader.AsStream();
        }

        return this.stream;
    }

    /// <summary>
    /// Writes the base64 data to the specified PipeWriter.
    /// </summary>
    private readonly async ValueTask WriteToPipeAsBase64Async(PipeWriter writer)
    {
        const int readSegmentLength = 12288;
        const int writeSegmentLength = 16384;

        PipeReader reader = PipeReader.Create
        (
            this.stream,
            new StreamPipeReaderOptions
            (
                bufferSize: 12288,
                leaveOpen: true,
                useZeroByteReads: true
            )
        );

        byte[] readBuffer = ArrayPool<byte>.Shared.Rent(readSegmentLength);
        byte[] writeBuffer = ArrayPool<byte>.Shared.Rent(writeSegmentLength);

        int readRollover = 0;

        while (true)
        {
            ReadResult result = await reader.ReadAsync();

            if (result.IsCanceled)
            {
                break;
            }

            ProcessResult(result, reader);

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
                        if (!sequence.TryCopyTo(readSpan[readRollover..]))
                        {
                            Trace.Assert(false);
                        }

                        sequence.Advance(readSegmentLength - readRollover);
                        readRollover = 0;
                    }
                    else
                    {
                        if (!sequence.TryCopyTo(readSpan.Slice(readRollover, (int)sequence.Remaining)))
                        {
                            Trace.Assert(false);
                        }

                        readRollover += (int)sequence.Remaining;
                        sequence.Advance((int)sequence.Remaining);
                        break;
                    }
                }
                else if (sequence.Remaining >= readSegmentLength)
                {
                    if (!sequence.TryCopyTo(readSpan))
                    {
                        Trace.Assert(false);
                    }

                    sequence.Advance(readSegmentLength);
                }
                else
                {
                    if (!sequence.TryCopyTo(readSpan[..(int)sequence.Remaining]))
                    {
                        Trace.Assert(false);
                    }

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
}
