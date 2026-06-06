using System;

namespace DSharpPlus.Voice.MemoryServices;

/// <summary>
/// Represents a leased array from an <see cref="AudioBufferManager"/>.
/// </summary>
public unsafe struct AudioBufferLease : IDisposable
{
    private readonly AudioBufferManager pool;
    private readonly void* backing;
    private readonly int allocatedLength;

    /// <summary>
    /// The buffer represented by this lease.
    /// </summary>
    public readonly Span<byte> Buffer => new(this.backing, this.allocatedLength);

    /// <summary>
    /// The amount of 20ms opus frames contained within this buffer.
    /// </summary>
    public int FrameCount { get; set; }

    /// <summary>
    /// Creates a new lease, noting the pool it must return to.
    /// </summary>
    internal AudioBufferLease(AudioBufferManager pool, void* backing, int allocatedLength)
    {
        this.pool = pool;
        this.backing = backing;
        this.allocatedLength = allocatedLength;
    }

    /// <summary>
    /// Disposes of this lease and returns it to the pool.
    /// </summary>
    public readonly void Dispose() 
        => this.pool.Return(this.backing);
}
