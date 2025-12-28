using System;

namespace DSharpPlus.Voice.MemoryServices;

/// <summary>
/// Represents a leased array from an <see cref="AudioBufferPool"/>.
/// </summary>
public record struct AudioBufferLease : IDisposable
{
    private readonly AudioBufferPool pool;

    /// <summary>
    /// The buffer represented by this lease.
    /// </summary>
    public readonly byte[] Buffer { get; }

    /// <summary>
    /// The actually utilized length within the leased buffer.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// The amount of 20ms opus frames contained within this buffer.
    /// </summary>
    public int FrameCount { get; set; }

    /// <summary>
    /// Creates a new lease, noting the pool it must return to.
    /// </summary>
    internal AudioBufferLease(AudioBufferPool pool, byte[] buffer)
    {
        this.pool = pool;
        this.Buffer = buffer;
    }

    /// <summary>
    /// Disposes of this lease and returns it to the pool.
    /// </summary>
    public readonly void Dispose() 
        => this.pool.Return(this.Buffer);
}
