using System;
using System.IO;

using DSharpPlus.Voice.RuntimeServices.Pipes;

namespace DSharpPlus.Voice;

/// <summary>
/// Provides a write-only stream for users as a compatibility wrapper. This is primarily intended for use in conjunction with
/// <see cref="Stream.CopyTo(Stream)"/> and its friends.
/// </summary>
internal sealed class AudioWriteStream : Stream
{
    private readonly int sampleSize;

    private readonly AbstractAudioWriter writer;
    private readonly byte[] overflowBuffer;
    private int currentOverflow;

    public AudioWriteStream(AbstractAudioWriter writer, int sampleSize)
    {
        this.sampleSize = sampleSize;
        this.writer = writer;
        this.overflowBuffer = new byte[3];
        this.currentOverflow = 0;
    }

    /// <inheritdoc/>
    public override bool CanRead => false;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanWrite => true;

    /// <inheritdoc/>
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    // this exists mostly to flush the overflow buffer in case we're about to go write bytes to the main writer. generally, bytes are only valid in
    // bits of four at a time, so, calling this is a bit of a warning signal.
    public override void Flush()
    {
        if (this.currentOverflow == 0)
        {
            return;
        }

        Span<byte> target = this.writer.GetSpan(this.currentOverflow);

        this.overflowBuffer.AsSpan()[..this.currentOverflow].CopyTo(target);
        this.writer.Advance(this.currentOverflow);
    }

    public override void Write(byte[] buffer, int offset, int count)
        => Write(buffer.AsSpan().Slice(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        int toWrite = this.currentOverflow + buffer.Length;

        // ensure we're aligned properly
        long targetPosition = this.Position + toWrite;
        int overflow = toWrite % this.sampleSize;
        toWrite -= overflow;

        // copy first the overflow buffer, then the rest of the buffer
        Span<byte> target = this.writer.GetSpan(toWrite);
        this.overflowBuffer.CopyTo(target);
        buffer.CopyTo(target[this.currentOverflow..]);

        // repopulate the overflow buffer with whatever we're overflowing with
        this.overflowBuffer.AsSpan().Clear();
        buffer[^overflow..].CopyTo(this.overflowBuffer);
        this.currentOverflow = overflow;
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();
}
