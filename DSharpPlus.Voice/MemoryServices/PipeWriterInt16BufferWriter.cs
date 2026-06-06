using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.MemoryServices;

/// <summary>
/// Represents a wrapper around a <see cref="PipeWriter"/> exposing its buffer-writer functionality for <see langword="short"/>.
/// </summary>
internal sealed class PipeWriterInt16BufferWriter : IBufferWriter<short>
{
    private readonly PipeWriter pipeWriter;

    public PipeWriterInt16BufferWriter(PipeWriter writer)
        => this.pipeWriter = writer;

    /// <inheritdoc />
    public void Advance(int count)
        => this.pipeWriter.Advance(count * 2);

    /// <inheritdoc />
    public Memory<short> GetMemory(int sizeHint = 0) 
        => throw new InvalidOperationException("Call GetSpan instead of GetMemory when acquiring a buffer for audio decoding");

    /// <inheritdoc />
    public Span<short> GetSpan(int sizeHint = 0)
    {
        Span<byte> memory = this.pipeWriter.GetSpan(sizeHint * 2);
        return MemoryMarshal.Cast<byte, short>(memory);
    }
}
