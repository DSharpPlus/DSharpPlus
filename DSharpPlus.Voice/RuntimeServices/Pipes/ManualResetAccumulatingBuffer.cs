using System;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

/// <summary>
/// A simple utility to aggregate bytes from different writes into a buffer.
/// </summary>
internal struct ManualResetAccumulatingBuffer
{
    private readonly byte[] buffer;
    private int index;

    public ManualResetAccumulatingBuffer(int length)
        => this.buffer = new byte[length];

    /// <summary>
    /// Writes the provided span to the buffer, returning how much was consumed and whether the buffer is now full.
    /// </summary>
    public bool Write(ReadOnlySpan<byte> span, out int consumed)
    {
        // catch scenarios where the caller forgot to clear this buffer out
        if (this.index == this.buffer.Length)
        {
            consumed = 0;
            return true;
        }

        int remainingCapacity = this.buffer.Length - this.index;

        // since this is likely to be the more common case, put it in the first branch. 
        if (span.Length > remainingCapacity)
        {
            span[..remainingCapacity].CopyTo(this.buffer.AsSpan()[this.index..]);
            consumed = remainingCapacity;

            this.index = this.buffer.Length;

            return true;
        }
        else
        {
            span.CopyTo(this.buffer.AsSpan()[this.index..]);
            consumed = span.Length;

            this.index += span.Length;

            return false;
        }
    }

    /// <summary>
    /// Resets this buffer after processing its contained data for reuse.
    /// </summary>
    public void Reset() 
        => this.index = 0;

    public readonly ReadOnlySpan<byte> WrittenSpan => this.buffer.AsSpan()[..this.index];
}
