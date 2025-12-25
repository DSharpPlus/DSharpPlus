namespace DSharpPlus.Voice.MemoryServices;

/// <summary>
/// A buffer "pool" that doesn't actually pool anything, suitable for very small buffers like silence frames.
/// </summary>
public sealed class NonPoolingBufferPool : AudioBufferPool
{
    private readonly int bufferSize;

    public NonPoolingBufferPool(int bufferSize) 
        => this.bufferSize = bufferSize;

    /// <summary>
    /// Creates a new buffer and wraps it into a pool lease.
    /// </summary>
    public override AudioBufferLease Rent() 
        => new(this, new byte[this.bufferSize]);

    /// <summary>
    /// Discards the returned buffer.
    /// </summary>
    protected internal override void Return(byte[] buffer)
    {

    }
}
