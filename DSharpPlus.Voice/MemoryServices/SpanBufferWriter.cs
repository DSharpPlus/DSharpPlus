using System;

namespace DSharpPlus.Voice.MemoryServices;

/// <summary>
/// A helper for writing to a span.
/// </summary>
internal ref struct SpanBufferWriter
{
    private readonly Span<byte> target;
    private int written;

    public SpanBufferWriter(Span<byte> target)
    {
        this.target = target;
        this.written = 0;
    }

    public readonly Span<byte> GetSpan() 
        => this.target[this.written..];

    public void Advance(int bytes)
    {
        this.written += bytes;

        if (this.written > this.target.Length)
        {
            throw new OverflowException("Cannot exceed the length of the target span");
        }
    }

    public readonly int Written => this.written;
}
