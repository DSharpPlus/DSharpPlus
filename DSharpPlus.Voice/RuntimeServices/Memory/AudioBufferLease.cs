using System;

namespace DSharpPlus.Voice.RuntimeServices.Memory;

/// <summary>
/// Represents a leased array from an <see cref="AudioBufferPool"/>.
/// </summary>
public readonly record struct AudioBufferLease : IDisposable
{
    private readonly AudioBufferPool pool;

    /// <summary>
    /// The buffer represented by this lease.
    /// </summary>
    public byte[] Buffer { get; }

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
    public void Dispose() 
        => this.pool.Return(this.Buffer);
}
